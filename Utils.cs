using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace netnje
{
    class Utils
    {
        /// <summary>
        /// Converts a string containing either a host name or IPv4 address in dotted notation to a UInt32 for NJE usage
        /// </summary>
        /// <param name="ipstr"></param>
        /// <returns>UInt32 representation of the supplied IP or hostname</returns>
        public static UInt32 stringToNjeIP(string ipstr)
        {
            UInt32 result = 0x80000001 ; // default to localhost/127.0.0.1 in case we fail

            if (ipstr.Count(f => f == '.') == 3)
            {
                IPAddress ipa = IPAddress.Parse(ipstr);
                byte[] aBytes = ipa.GetAddressBytes();

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(aBytes);
                
                result = BitConverter.ToUInt32(aBytes, 0);
                
                return result;
            }

            IPHostEntry hostEntry = Dns.GetHostEntry(ipstr);

            if (hostEntry.AddressList.Length > 0)
            {
                byte[] addressBytes = hostEntry.AddressList[0].GetAddressBytes();
                result = BitConverter.ToUInt32(addressBytes, 0);
            }


            return result;
        }
    }
}
