using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace netnje.Structures
{
    class DataRecord
    {
        public byte[] Data { get; set; }

        public byte[] DLSTX { get; set; }

        public byte[] FCS { get; set; }

        public byte ServerSequence { get; set; }

        public List<IRecord> Records { get; set; }

        

        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DataRecord(byte[] Data)
        {
            Records = new List<IRecord>();
            this.Data = Data;
            ParseRecord();
            FCS = new byte[2];
        }

        private void ParseRecord()
        {
            log.Debug("Attempting to parse record.");

            if (Data.Length == 6)
            {
                Records.Add(new HeartbeatRecord());
            } else
            {
                if (Data.Length > 2)
                {
                    DLSTX = new byte[2];
                    Array.Copy(Data, 0, DLSTX, 0, 2);
                    ServerSequence = Data[2];

                    FCS[0] = Data[3];
                    FCS[1] = Data[4];



                }
            }

            log.Debug("Record parsed.");
        }
    }
}
