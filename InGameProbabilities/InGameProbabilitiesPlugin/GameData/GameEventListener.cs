using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace InGameProbabilitiesPlugin.GameData
{
    public class GameEventListener
    {
        private UdpClient client;
        private IPEndPoint ipep;

        public GameEventListener(int port)
        {
            client = new UdpClient(port);
            ipep = new IPEndPoint(IPAddress.Any, port);
        }

        /**
         * Returns a single UDP message from the socket listener
         */
        public string GetMessage()
        {
            return Encoding.UTF8.GetString(client.Receive(ref ipep));
        }
    }
}
