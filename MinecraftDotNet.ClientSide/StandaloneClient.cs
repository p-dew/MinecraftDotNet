using MinecraftDotNet.ClientSide.Graphics;
using MinecraftDotNet.Core;
using MinecraftDotNet.Core.Blocks;
using MinecraftDotNet.Core.Blocks.Chunks;
using MinecraftDotNet.Core.Worlds;
using ObjectTK.Tools.Cameras;

namespace MinecraftDotNet.ClientSide
{
    public class StandaloneClient : IClient
    {
        private readonly MinecraftGameWindow _window;
        private readonly IWorld _currentWorld;
        private SingleBlockChunkRenderer _chunkRenderer;
        private readonly Camera _camera;

        public StandaloneClient()
        {
            _camera = new Camera();
            _camera.SetBehavior(new FreeLookBehavior());
            _camera.DefaultState.Position.Z -= 20;

            _window = new MinecraftGameWindow(_camera);
            
            _camera.Enable(_window);

            var chunkRepository = new MemoryChunkRepository(new FlatChunkGenerator(60));
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