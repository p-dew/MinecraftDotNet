using System.Collections.Generic;
using MinecraftDotNet.Core;
using MinecraftDotNet.Core.Worlds;

namespace MinecraftDotNet.ServerSide
{
    public class Server : IServer
    {
        public Server(IWorld world)
        {
            World = world;
            Clients = new List<IClient>();
        }

        public IWorld World { get; }
        
        public IReadOnlyCollection<IClient> Clients { get; }
    }
}