using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace netnje
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            log.InfoFormat("Starting netnje version {0}", typeof(Program).Assembly.GetName().Version);

            
            //NjeClient client = new NjeClient("UKYJVMS", "FRYJLX1", "bsdm.yvanj.me", 175);
            //client.Connect();

            Console.WriteLine("Hit enter");
            Console.ReadLine();
        }
    }
}
