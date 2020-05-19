using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace netnje.Structures
{
    class HeartbeatRecord : IRecord
    {
        public HeartbeatRecord()
        {

        }

        public byte[] Data => new byte[6];

        public byte[] GetBytes()
        {
            return new byte[6];
        }

        public void ParseBytes(byte[] SignInRecordBytes)
        {
            
        }
    }
}
