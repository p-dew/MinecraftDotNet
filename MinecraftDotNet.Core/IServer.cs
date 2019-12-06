using System.Collections.Generic;

namespace MinecraftDotNet.Core
{
    public interface IServer
    {
        IReadOnlyCollection<IClient> Clients { get; }
    }
}