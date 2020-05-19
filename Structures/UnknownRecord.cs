using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace netnje.Structures
{
    class UnknownRecord : IRecord
    {
        private byte[] _receivedData;
        public byte[] Data => _receivedData;

        public byte[] GetBytes()
        {
            return _receivedData;
        }

        public void ParseBytes(byte[] RecordBytes)
        {
            _receivedData = RecordBytes;
        }

        public UnknownRecord()
        {

        }

        public UnknownRecord(byte[] RecordBytes)
        {
            ParseBytes(RecordBytes);
        }
    }
}
