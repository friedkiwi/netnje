using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace netnje.Structures
{
    interface IRecord
    {
        void ParseBytes(byte[] RecordBytes);
        byte[] GetBytes();
        byte[] Data { get; }
    }
}
