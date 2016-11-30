
namespace InGameProbabilitiesPlugin.GameData
{
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    public class GameEventListener
    {
        private readonly UdpClient _client;
        private IPEndPoint _ipep;

        public GameEventListener(int port)
        {
            this._client = new UdpClient(port);
            this._ipep = new IPEndPoint(IPAddress.Any, port);
        }

        /**
         * Returns a single UDP message from the socket listener
         */
        public string GetMessage()
        {
            return Encoding.UTF8.GetString(this._client.Receive(ref this._ipep));
        }
    }
}
