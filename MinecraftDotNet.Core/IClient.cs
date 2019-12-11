using MinecraftDotNet.Core.Entities;

namespace MinecraftDotNet.Core
{
    public interface IClient
    {
        // IServer Server { get; }
        Player Player { get; }
    }
}