using System.Numerics;
using MinecraftDotNet.ClientSide.Graphics.Core;

namespace MinecraftDotNet.ClientSide.Graphics.OpenGl
{
    public class GlMesh : Mesh
    {
        public GlMesh(int vaoHandle, Vector3[] vertices, Vector2[] uvs, int[] elements) : base(vertices, uvs, elements)
        {
            VaoHandle = vaoHandle;
        }

        public GlMesh(int vaoHandle, MeshVertex[] meshVertices, int[] elements) : base(meshVertices, elements)
        {
            VaoHandle = vaoHandle;
        }
        
        public int VaoHandle { get; }
    }
}