using System.Net;
using System.Net.Sockets;

namespace Models
{
    public class Client
    {
        public Guid Id { get; set; }

        public int Port { get; set; }

        public IPEndPoint EndPoint { get; set; }

        public Socket Listen { get; set; } =
            new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public Socket Handler { get; set; }

    }
}