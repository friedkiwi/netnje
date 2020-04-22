using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using netnje.Structures;
using System.Threading;
using System.IO;

namespace netnje
{
    class NjeClient
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private TcpClient tcpClient;
        private NetworkStream tcpStream;
        private bool _isSuccessfullyConnected = false;
        private UInt32 sequence = 0x80;

        public string ClientNodeID { get; set; }
        public string ServerNodeID { get; set; }
        public string ServerHost { get; set; }
        public int ServerPort { get; set; }

        public bool IsConnected
        {
            get
            {
                return _isSuccessfullyConnected;
            }
        }


        public NjeClient(string ClientNodeID, string ServerNodeID, string ServerHost, int ServerPort)
        {
            this.ClientNodeID = ClientNodeID;
            this.ServerNodeID = ServerNodeID;
            this.ServerPort = ServerPort;
            this.ServerHost = ServerHost;
            

            tcpClient = new TcpClient();
            

            log.Debug("Instance of NjeClient instantiated.");
        }

        public void Connect()
        {
            ControlRecord sendRecord;
            ControlRecord receiveRecord;
            byte[] buffer;

            log.InfoFormat("Connecting to {0}:{1}", this.ServerHost, this.ServerPort);
            tcpClient.Connect(this.ServerHost, this.ServerPort);
            tcpStream = tcpClient.GetStream();

            sendRecord = new ControlRecord("OPEN", this.ClientNodeID, "127.0.0.1", this.ServerNodeID, this.ServerHost, 0);
            buffer = sendRecord.GetBytes();

            tcpStream.Write(buffer, 0, 33);

            tcpStream.Read(buffer, 0, 33);

            receiveRecord = new ControlRecord(buffer);

            log.DebugFormat("Reason code: {0}, response type: {1}.", receiveRecord.ReasonCode, receiveRecord.RequestType);

            if (receiveRecord.RequestType != "ACK")
            {
                log.ErrorFormat("Could not log into server {0}:{1} with Node ID {2}, server responded: {3}.", this.ServerHost, this.ServerPort, this.ServerNodeID, receiveRecord.RequestType);
            } else
            {
                this._isSuccessfullyConnected = true;
                log.InfoFormat("Connected to {0}", this.ServerNodeID);
            }

            // send SOH ENQ

            log.InfoFormat("Sending SOH ENQ to {0}", this.ServerNodeID);
            byte[] sohenq = MakeTTB(MakeTTR(new byte[] { 0x01, 0x2d }));
            tcpStream.Write(sohenq, 0, sohenq.Length);

            while(!tcpStream.DataAvailable)
            {
                log.InfoFormat("Waiting for response to SOH ENQ from {0}...", this.ServerNodeID);
                Thread.Sleep(500);
            }

            MemoryStream responseMS = new MemoryStream();

            while(tcpStream.DataAvailable)
            {
                byte[] rbuffer = new byte[256];
                int nR = tcpStream.Read(rbuffer, 0, 256);
                responseMS.Write(rbuffer, 0, nR);
            }

            List<DataRecord> result = ProcessData(responseMS.ToArray());


            if (result[0].Data[0] == 0x10 && result[0].Data[1] == 0x70)
            {
                log.InfoFormat("Received DLE ACK0 from {0}.", this.ServerNodeID);
            } else
            {
                log.InfoFormat("Did not DLE ACK0 from {0}!", this.ServerNodeID);
            }

            
        }

        public void Poll()
        {
            log.InfoFormat("Pollling for data on connection to node {0}", this.ServerNodeID);
            if (tcpStream.DataAvailable)
            {
                log.InfoFormat("Data available on connection to node {0}.", this.ServerNodeID);
            }
        }

        private void IncreaseSequence()
        {
            UInt32 prev = this.sequence;
            this.sequence = (this.sequence & 0x0F) + 1 | 0x80;
            log.InfoFormat("Increased sequence from {0:X} to {1:X} on connection with node {2}.", prev, this.sequence, this.ServerNodeID);
        }


        private byte[] MakeTTR(byte[] data, bool EndOfRecord)
        {
            UInt16 dataLength = (UInt16) data.Length;
            byte[] dataLengthBytes = BitConverter.GetBytes(dataLength);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(dataLengthBytes);
            }

            byte[] result;
            if (EndOfRecord)
            {
                result = new byte[5 + dataLength];
            } else
            {
                result = new byte[5 + dataLength];
            }

            Array.Copy(dataLengthBytes, 0, result, 2, 2);
            Array.Copy(data, 0, result, 4, dataLength);

            return result;
        }

        private byte[] MakeTTR(byte[] data)
        {
            return MakeTTR(data, false);
        }

        private byte[] MakeTTB(byte[] data)
        {
            UInt16 dataLength = (UInt16)(data.Length + 12);
            byte[] dataLengthBytes = BitConverter.GetBytes(dataLength);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(dataLengthBytes);
            }

            byte[] result = new byte[12 + data.Length];

            Array.Copy(dataLengthBytes, 0, result, 2, 2);
            Array.Copy(data, 0, result, 8, data.Length);

            return result;
        }

        private List<DataRecord> ProcessData(byte[] dataReceived)
        {
            // extract record length
            byte[] dataLen = new byte[2];
            Array.Copy(dataReceived, 2, dataLen, 0, 2);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(dataLen);
            }
            int offset = 0;
            UInt16 i16_totalLength = BitConverter.ToUInt16(dataLen, 0);
            byte[] recordSet = new byte[dataReceived.Length - 12];
            Array.Copy(dataReceived, 8, recordSet, 0, dataReceived.Length - 12);

            log.DebugFormat("[{0}] Total length received: {1}", this.ServerNodeID, i16_totalLength);

            List<DataRecord> receivedRecords = new List<DataRecord>();

            while (offset  < dataReceived.Length - 12)
            {
                bool lastRecord = false;

                byte[] recordLengthBytes = new byte[2];
                Array.Copy(recordSet, offset + 2, recordLengthBytes, 0, 2);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(recordLengthBytes);
                }
                offset += 4;

                UInt16 recordLength = BitConverter.ToUInt16(recordLengthBytes, 0);
                log.DebugFormat("[{0}] Record length received: {1}", this.ServerNodeID, recordLength);

                if (offset + recordLength == dataReceived.Length - 12)
                {
                    lastRecord = true;
                    recordLength--;
                }


                byte[] individualRecord = new byte[recordLength];
                Array.Copy(recordSet, offset, individualRecord, 0, recordLength);
                receivedRecords.Add(new DataRecord(individualRecord));
                offset += recordLength;
                if (lastRecord)
                    offset++;
            }


            return receivedRecords;

        }
        
    }
}
