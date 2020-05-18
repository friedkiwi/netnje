using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace netnje.Structures
{
    interface IRecord
    {
        void ParseBytes(byte[] SignInRecordBytes);
        byte[] GetBytes();
    }
}
