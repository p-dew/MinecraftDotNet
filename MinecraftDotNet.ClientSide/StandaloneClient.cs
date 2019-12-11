using MinecraftDotNet.ClientSide.Graphics;
using MinecraftDotNet.Core;
using MinecraftDotNet.Core.Blocks;
using MinecraftDotNet.Core.Blocks.Chunks;
using MinecraftDotNet.Core.Entities;
using MinecraftDotNet.Core.Worlds;
using ObjectTK.Tools.Cameras;
using OpenTK;

namespace MinecraftDotNet.ClientSide
{
    public class StandaloneClient : IClient
    {
        private readonly McGameWindow _window;
        private readonly IWorld _currentWorld;
        private readonly SingleBlockChunkRenderer _chunkRenderer;
        private readonly Camera _camera;

        public StandaloneClient()
        {
            _camera = new Camera
            {
                State =
                {
                    Position = new Vector3(2, 2, 2), 
                    LookAt = Vector3.One
                }
            };
            
            _window = new McGameWindow(_camera);

            _camera.MoveSpeed /= 2.0f;
            _camera.MouseMoveSpeed /= 2.0f; 
            _camera.SetBehavior(new FreeLookBehavior());
            
            _camera.Enable(_window);

            var chunkRepository = new MemoryChunkRepository(new ChessChunkGenerator(c => HcBlocks.Dirt));
            var blockRepository = new ChunkBlockRepository(chunkRepository);
            _currentWorld = 
                new WorldBuilder()
                    .UseBlockRepository(() => blockRepository)
                    .UseChunkRepository(() => chunkRepository)
                    .Build();
            
            _chunkRenderer = new SingleBlockChunkRenderer(_camera);
            
            _window.AddRenderAction((projection, modelView) =>
            {
                var chunkCoords = new ChunkCoords(0, 0);
                var chunk = chunkRepository.GetChunk(chunkCoords);
                _chunkRenderer.Render(new ChunkRenderContext(projection, modelView), chunk, chunkCoords);
            });
        }

        public Player Player { get; }

        public void Run()
        {
            _window.Run();
        }
    }
}