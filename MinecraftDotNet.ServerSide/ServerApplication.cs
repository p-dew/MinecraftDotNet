using System.Collections.Generic;
using MinecraftDotNet.Core;
using MinecraftDotNet.Core.Worlds;

namespace MinecraftDotNet.ServerSide
{
    public class ServerApplication : IServerApplication
    {
        public ServerApplication(IWorld world)
        {
            World = world;
            Clients = new List<IClientApplication>();
        }

        public IWorld World { get; }
        
        public IReadOnlyCollection<IClientApplication> Clients { get; }
        
        public void Run()
        {
            throw new System.NotImplementedException();
        }
    }
}