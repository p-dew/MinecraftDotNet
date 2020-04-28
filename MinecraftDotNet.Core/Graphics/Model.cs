using System.Collections.Generic;
using System.Numerics;
using MinecraftDotNet.Core.Resources;

namespace MinecraftDotNet.Core.Graphics
{
    public interface IModel
    {
        IReadOnlyList<Vector3> Vertices { get; }
        IReadOnlyList<ResourceId> TextureNames { get; }
        IReadOnlyList<int> Uvs { get; }
    }

    public sealed class SolidBlockModel : IModel
    {
        private static readonly IReadOnlyList<Vector3> BlockVertices = new[]
        {
            new Vector3(),
        };

        public IReadOnlyList<Vector3> Vertices => BlockVertices;
        public IReadOnlyList<ResourceId> TextureNames { get; }
        public IReadOnlyList<int> Uvs { get; }
    }
}