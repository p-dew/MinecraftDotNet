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
        private readonly Window _window;
        private readonly IWorld _currentWorld;
        private readonly SingleBlockChunkRenderer _chunkRenderer;
        private readonly Camera _camera;
        
        public StandaloneClient()
        {
            _window = new Window();

            var chunkRepository = new MemoryChunkRepository(new FlatChunkGenerator(60));
            var blockRepository = new ChunkBlockRepository(chunkRepository);
            _currentWorld = new World(
                () => chunkRepository, 
                () => blockRepository);
            
            _camera = new Camera();
            _camera.SetBehavior(new FreeLookBehavior());
            _camera.Enable(_window);
            
            _chunkRenderer = new SingleBlockChunkRenderer(_camera);

            _window.RenderFrame += OnWindowRenderFrame;
        }

        private void OnWindowRenderFrame(object sender, FrameEventArgs e)
        {
            _camera.Update();

            var chunkCoords = new ChunkCoords(0, 0);
            var chunk = _currentWorld.ChunkRepository.GetChunk(chunkCoords);
            _chunkRenderer.Render(new ChunkRenderContext(), chunk, chunkCoords);
        }

        public void Run()
        {
            _window.Run();
        }
    }
}