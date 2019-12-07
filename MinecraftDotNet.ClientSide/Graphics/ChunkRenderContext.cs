using OpenTK;

namespace MinecraftDotNet.ClientSide.Graphics
{
    public class ChunkRenderContext : IChunkRenderContext
    {
        public ChunkRenderContext(Matrix4 projection, Matrix4 modelView)
        {
            Projection = projection;
            ModelView = modelView;
        }

        public Matrix4 Projection { get; }
        
        public Matrix4 ModelView { get; }
    }
}