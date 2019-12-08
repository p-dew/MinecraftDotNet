using System;
using MinecraftDotNet.ClientSide.Graphics.Shaders;
using MinecraftDotNet.Core.Blocks;
using MinecraftDotNet.Core.Blocks.Chunks;
using MinecraftDotNet.Core.Math;
using ObjectTK.Buffers;
using ObjectTK.Shaders;
using ObjectTK.Tools.Cameras;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Buffer = System.Buffer;

namespace MinecraftDotNet.ClientSide.Graphics
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
        
        private static readonly Vector2d[] CubeUv = new Vector2d[]
        {
            new Vector2d(0, 0),
            new Vector2d(0, 1),
            new Vector2d(1, 0),
            new Vector2d(1, 1),
            new Vector2d(0, 0),
            new Vector2d(0, 1),
            new Vector2d(1, 0),
            new Vector2d(1, 1),
        };

        private static readonly int[] CubeEbo = new int[]
        {
            // 4, 5, 6, 7,
            // 0, 1, 2, 3,
            // 2, 3, 6, 7,
            // 0, 1, 4, 5,
            // 1, 3, 5, 7,
            // 0, 2, 4, 6,
            
            0, 2, 3,
            3, 1, 0,

            1, 3, 7,
            7, 5, 1,

            5, 7, 6,
            6, 4, 5,

            4, 6, 2,
            2, 0, 4,

            0, 1, 5,
            5, 4, 0,

            7, 3, 2,
            2, 6, 7
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
        private readonly Buffer<Vector3d> _cubeVertexBuffer;
        private readonly Buffer<int> _cubeElementBuffer;
        private readonly Buffer<Vector2d> _cubeUvBuffer;

        public SingleBlockChunkRenderer(Camera camera)
        {
            _camera = camera;
            _program = ProgramFactory.Create<BlockProgram>();
            
            _cubeVertexBuffer = new Buffer<Vector3d>();
            _cubeVertexBuffer.Init(BufferTarget.ArrayBuffer, CubeVertices);
            
            _cubeElementBuffer = new Buffer<int>();
            _cubeElementBuffer.Init(BufferTarget.ElementArrayBuffer, CubeEbo);
            
            _cubeUvBuffer = new Buffer<Vector2d>();
            _cubeUvBuffer.Init(BufferTarget.ArrayBuffer, CubeUv);
            
            _vao = new VertexArray();
            _vao.Bind();

            _vao.BindAttribute(_program.InVertex, _cubeVertexBuffer);
            _vao.BindAttribute(_program.InUv, _cubeUvBuffer);
            
            _vao.BindElementBuffer(_cubeElementBuffer);
        }

        private void RenderCube(ChunkRenderContext context, BlockInfo blockInfo, int x, int y, int z)
        {
            _program.MvpMatrix.Set(
                Matrix4.CreateTranslation(new Vector3(x, y, z)) *
                context.ModelView * 
                context.Projection);

            for (var i = 0; i < 6; i++)
            {
                var tex = blockInfo.Sides.Textures[i];
                    
                _program.Side.BindTexture(TextureUnit.Texture0, tex);

                _vao.DrawElements(PrimitiveType.Triangles, _cubeElementBuffer.ElementCount);
            }
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
                
                // Air-skipping
                if (blockInfo.ItemInfo.Id == "air")
                    continue;

                var blockX = chunkCoords.X * Chunk.Width + x;
                var blockY = y;
                var blockZ = chunkCoords.Z * Chunk.Depth + z;
                
                RenderCube(context, blockInfo, blockX, blockY, blockZ);
            }
            
            GL.BindVertexArray(0);
        }
    }
}