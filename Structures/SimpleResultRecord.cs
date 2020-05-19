using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace netnje.Structures
{
    class SimpleResultRecord : IRecord
    {
        private byte[] _receivedBytes;
        public byte[] Data => _receivedBytes;

        public byte[] GetBytes()
        {
            throw new NotImplementedException();
        }

        public void ParseBytes(byte[] RecordBytes)
        {
            
        }

        public SimpleResultRecord()
        {
        
        }

        public SimpleResultRecord(byte[] DataBytes)
        {
            _receivedBytes = DataBytes;
            ParseBytes(DataBytes);
        }
    }
}
