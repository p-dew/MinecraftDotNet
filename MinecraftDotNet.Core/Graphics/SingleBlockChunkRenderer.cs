using MinecraftDotNet.Core.Blocks.Chunks;
using MinecraftDotNet.Core.Graphics.Shaders;
using ObjectTK.Buffers;
using ObjectTK.Shaders;
using ObjectTK.Textures;
using ObjectTK.Tools.Cameras;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace MinecraftDotNet.Core.Graphics
{
    public class SingleBlockChunkRenderer : IChunkRenderer<ChunkRenderContext>
    {
        private readonly Camera _camera;

        private static readonly Vector3d[] CubeVertices = new Vector3d[]
        {
            new Vector3d(0, 0, 0),
            new Vector3d(0, 0, 1),
            new Vector3d(0, 1, 0),
            new Vector3d(0, 1, 1),
            new Vector3d(1, 0, 0),
            new Vector3d(1, 0, 1),
            new Vector3d(1, 1, 0),
            new Vector3d(1, 1, 1),
        };

        private static readonly byte[] CubeEbo = new byte[]
        {
            4, 5, 6, 7,
            0, 1, 2, 3,
            2, 3, 6, 7,
            0, 1, 4, 5,
            1, 3, 5, 7,
            0, 2, 4, 6,
        };

        private static readonly Vector2d[] Uvs = new Vector2d[]
        {
            new Vector2d(0, 0),
            new Vector2d(0, 1),
            new Vector2d(1, 0),
            new Vector2d(1, 1),
        };

        private static readonly byte[] TexEbo = new byte[]
        {
            
        };

        private readonly VertexArray _vao;
        private readonly BlockProgram _program;

        public SingleBlockChunkRenderer(Camera camera)
        {
            _camera = camera;
            _program = ProgramFactory.Create<BlockProgram>();
            
            var vbo = new Buffer<Vector3d>();
            vbo.Init(BufferTarget.ArrayBuffer, CubeVertices);
            
            var vertebo = new Buffer<byte>();
            vertebo.Init(BufferTarget.ElementArrayBuffer, CubeEbo);

            var texebo = new Buffer<byte>();
            texebo.Init(BufferTarget.ElementArrayBuffer, TexEbo);
            
            _vao = new VertexArray();
            
            _vao.BindElementBuffer(vertebo);
            _vao.BindAttribute(_program.InVertex, vbo);
            
        }
        
        public void Render(ChunkRenderContext context, Chunk chunk, ChunkCoords chunkCoords)
        {
            _vao.Bind();
            _program.Use();
            
            _program.MvpMatrix.Value = _camera.GetCameraTransform();
            
            for (var x = 0; x < Chunk.Width; x++) 
            for (var y = 0; y < Chunk.Height; y++) 
            for (var z = 0; z < Chunk.Depth; z++)
            {
                var blockInfo = chunk.Blocks[x, y, z];

                var blockX = chunkCoords.X * Chunk.Width + x;
                var blockY = y;
                var blockZ = chunkCoords.Z * Chunk.Depth + z;
                
                _program.BlockPosition.Value = new Vector3d(blockX, blockY, blockZ);
                
                for (var i = 0; i < 6; i++)
                {
                    var tex = blockInfo.Sides.Textures[i];
                    _program.Side.BindTexture(TextureUnit.Texture0, tex);
                    
                    _vao.DrawElementsIndirect(PrimitiveType.TriangleStrip, DrawElementsType.UnsignedByte, i * 4);
                    _vao.DrawElements(PrimitiveType.TriangleStrip, 4);
                }
                
                _vao.DrawElements(PrimitiveType.LineStrip, 8);
            }
            
            GL.BindVertexArray(0);
        }
    }
}