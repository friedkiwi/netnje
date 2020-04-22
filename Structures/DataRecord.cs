using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace netnje.Structures
{
    class DataRecord
    {
        public byte[] RCB { get; set; }
        public byte[] SRCB { get; set; }
        public byte[] Data { get; set; }

        public byte[] DLSTX { get; set; }

        public byte ServerSequence { get; set; }

        public bool IsHeartbeat { get; set; }
        public bool IsUnknown { get; set; }

        

        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DataRecord(byte[] Data)
        {
            this.Data = Data;
            ParseRecord();
        }

        private void ParseRecord()
        {
            log.Debug("Attempting to parse record.");

            if (Data.Length == 6)
            {
                this.IsHeartbeat = true;
            } else
            {
                if (Data.Length > 2)
                {
                    DLSTX = new byte[2];
                    Array.Copy(Data, 0, DLSTX, 0, 2);
                    ServerSequence = Data[2];


                } else
                {
                    this.IsUnknown = true;
                }
            }

            log.Debug("Record parsed.");
        }
    }
}
