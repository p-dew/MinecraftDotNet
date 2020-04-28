using System.Collections.Generic;
using MinecraftDotNet.Core.Worlds;

namespace MinecraftDotNet.Core
{
    public interface IServerApplication
    {
        IWorld World { get; }
        
        IReadOnlyCollection<IClientApplication> Clients { get; }

        void Run();
    }
}