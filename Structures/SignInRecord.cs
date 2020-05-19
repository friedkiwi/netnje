using JonSkeet.Ebcdic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace netnje.Structures
{
    class SignInRecord : IRecord
    {
        public UInt16 BufferSize { get; set; }
        
        public string RemoteNode { get; set; }
        public string LocalNode { get; set; }

        private byte[] _originalData;

        public byte[] Data => _originalData;

        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SignInRecord(byte[] RecordBytes)
        {
            _originalData = RecordBytes;
            this.ParseBytes(RecordBytes);
        }

        public SignInRecord()
        {

        }

        public byte[] GetBytes()
        {
            byte[] RecordBytes = new byte[41];

            byte[] RemoteNodeEBCDIC;
            byte[] LocalNodeEBCDIC;
            byte[] BufferSizeBytes;


            RemoteNodeEBCDIC = EbcdicEncoding.Convert(Encoding.ASCII, EbcdicEncoding.GetEncoding("EBCDIC-US"), Encoding.ASCII.GetBytes(RemoteNode.PadRight(8, ' ')));
            LocalNodeEBCDIC = EbcdicEncoding.Convert(Encoding.ASCII, EbcdicEncoding.GetEncoding("EBCDIC-US"), Encoding.ASCII.GetBytes(LocalNode.PadRight(8, ' ')));
            BufferSizeBytes = BitConverter.GetBytes(BufferSize);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(BufferSizeBytes);
            }

                RecordBytes[0] = 0x29;
            Array.Copy(LocalNodeEBCDIC, 0, RecordBytes, 1, 8);
            Array.Copy(RemoteNodeEBCDIC, 0, RecordBytes, 9, 8);
            RecordBytes[18] = 0x64;
            Array.Copy(BufferSizeBytes, 0, RecordBytes, 19, 2);
            RecordBytes[38] = 0x15; 

            return RecordBytes;
        }

        public void ParseBytes(byte[] SignInRecordBytes)
        {
            byte[] RemoteNodeEBCDIC = new byte[8];
            byte[] LocalNodeEBCDIC = new byte[8];
            byte[] BufferSizeBytes = new byte[8];

            Array.Copy(SignInRecordBytes, 1, LocalNodeEBCDIC, 0, 8);
            Array.Copy(SignInRecordBytes, 9, RemoteNodeEBCDIC, 0, 8);
            Array.Copy(SignInRecordBytes, 19, BufferSizeBytes, 0, 2);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(BufferSizeBytes);
            }

            this.BufferSize = BitConverter.ToUInt16(BufferSizeBytes, 0);

            RemoteNodeEBCDIC = EbcdicEncoding.Convert(EbcdicEncoding.GetEncoding("EBCDIC-US"), Encoding.ASCII, RemoteNodeEBCDIC);
            LocalNodeEBCDIC = EbcdicEncoding.Convert(EbcdicEncoding.GetEncoding("EBCDIC-US"), Encoding.ASCII, LocalNodeEBCDIC);

            this.RemoteNode = ASCIIEncoding.ASCII.GetString(RemoteNodeEBCDIC).Trim();
            this.LocalNode = ASCIIEncoding.ASCII.GetString(LocalNodeEBCDIC).Trim();
        }
    }
}
