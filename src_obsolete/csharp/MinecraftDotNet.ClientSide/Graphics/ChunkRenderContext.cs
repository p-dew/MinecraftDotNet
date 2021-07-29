using OpenTK;

namespace MinecraftDotNet.ClientSide.Graphics
{
    public class ChunkRenderContext : IChunkRenderContext
    {
        public ChunkRenderContext(Matrix4 projectionMatrix, Matrix4 viewMatrix)
        {
            ProjectionMatrix = projectionMatrix;
            ViewMatrix = viewMatrix;
        }

        public Matrix4 ProjectionMatrix { get; }
        
        public Matrix4 ViewMatrix { get; }
    }
}