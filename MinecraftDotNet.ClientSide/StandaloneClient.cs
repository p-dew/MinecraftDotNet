using MinecraftDotNet.ClientSide.Graphics;
using MinecraftDotNet.Core;
using MinecraftDotNet.Core.Blocks;
using MinecraftDotNet.Core.Blocks.Chunks;
using MinecraftDotNet.Core.Worlds;
using ObjectTK.Tools.Cameras;
using OpenTK;

namespace MinecraftDotNet.ClientSide
{
    public class StandaloneClient : IClient
    {
        private readonly MinecraftGameWindow _window;
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
            _camera.SetBehavior(new FreeLookBehavior());

            _window = new MinecraftGameWindow(_camera);
            
            _camera.Enable(_window);

            var chunkRepository = new MemoryChunkRepository(new ChessChunkGenerator());
            var blockRepository = new ChunkBlockRepository(chunkRepository);
            _currentWorld = new World(
                () => chunkRepository, 
                () => blockRepository);
            
            _chunkRenderer = new SingleBlockChunkRenderer(_camera);
            
            _window.AddRenderAction((projection, modelView) =>
            {
                var chunkCoords = new ChunkCoords(0, 0);
                var chunk = chunkRepository.GetChunk(chunkCoords);
                _chunkRenderer.Render(new ChunkRenderContext(projection, modelView), chunk, chunkCoords);
            });
        }

        public void Run()
        {
            _window.Run();
        }
    }
}