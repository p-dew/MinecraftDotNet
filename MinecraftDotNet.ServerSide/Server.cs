using System.Collections.Generic;
using MinecraftDotNet.Core;

namespace MinecraftDotNet.ServerSide
{
    public class Server : IServer
    {
        public Server()
        {
            Clients = new List<IClient>();
        }

        public IReadOnlyCollection<IClient> Clients { get; }
    }
}