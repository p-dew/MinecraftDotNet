using System.Collections.Generic;
using System.IO;

namespace MinecraftDotNet.Core.Resources
{
    public sealed class Sound : IResource
    {
        public Sound(ResourceId id, IReadOnlyList<byte> data)
        {
            Id = id;
            Data = data;
        }

        public ResourceId Id { get; }
        
        public IReadOnlyList<byte> Data { get; }

        public void Dispose()
        {
        }
    }
}