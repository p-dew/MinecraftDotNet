using System.Collections.Generic;
using MinecraftDotNet.Core.Worlds;

namespace MinecraftDotNet.Core
{
    public interface IServer
    {
        IWorld CurrentWorld { get; }
        
        IReadOnlyCollection<IClient> Clients { get; }
    }
}