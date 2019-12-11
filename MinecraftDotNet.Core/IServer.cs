using System.Collections.Generic;
using MinecraftDotNet.Core.Worlds;

namespace MinecraftDotNet.Core
{
    public interface IServer
    {
        IWorld World { get; }
        
        IReadOnlyCollection<IClient> Clients { get; }
    }
}