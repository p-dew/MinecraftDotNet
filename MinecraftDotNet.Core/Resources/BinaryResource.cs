using System;
using System.Collections.Generic;

namespace MinecraftDotNet.Core.Resources
{
    public class BinaryResource : IResource
    {
        public BinaryResource(ResourceId id, IReadOnlyList<byte> data)
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