using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using netnje.Structures;

namespace netnje
{
    class NjeClient
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private TcpClient tcpClient;
        private NetworkStream tcpStream;

        public string ClientNodeID { get; set; }
        public string ServerNodeID { get; set; }
        public string ServerHost { get; set; }
        public int ServerPort { get; set; }


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

            log.InfoFormat("Connected to {0}", this.ServerNodeID);
            
        }

        public void Poll()
        {

        }
    }
}
