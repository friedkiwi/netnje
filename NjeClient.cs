using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace netnje
{
    class NjeClient
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private TcpClient tcpClient;

        public string ClientNodeID { get; set; }
        public string ServerNodeID { get; set; }
        public string ServerHost { get; set; }
        public int ServerPort { get; set; }


        public NjeClient(string ClientNodeID, string ServerNodeID, string ServerHost, int ServerPort)
        {
            this.ClientNodeID = ClientNodeID;
            this.ServerNodeID = ServerNodeID;
            this.ServerPort = ServerPort;
            this.ServerHost = ServerHost;
            

            tcpClient = new TcpClient();
            

            log.Debug("Instance of NjeClient instantiated.");
        }

        public void Connect()
        {
            log.InfoFormat("Connecting to {0}:{1}", this.ServerHost, this.ServerPort);
            tcpClient.Connect(this.ServerHost, this.ServerPort);
        }

        public void Poll()
        {

        }
    }
}
