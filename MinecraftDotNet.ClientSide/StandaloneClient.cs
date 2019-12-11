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
        private readonly SingleBlockChunkRenderer _chunkRenderer;
        private readonly Camera _camera;
        private readonly IServer _server;

        public StandaloneClient(IServer server)
        {
            _server = server;
            
            _camera = new Camera
            {
                State =
                {
                    Position = new Vector3(2, 2, 2), 
                    LookAt = Vector3.One
                },
                MoveSpeed = 60f / 2f,
                MouseMoveSpeed = 0.005f / 2f,
            };
            
            _window = new McGameWindow(_camera);

            _camera.SetBehavior(new FreeLookBehavior());
            
            _camera.Enable(_window);

            
            
            _chunkRenderer = new SingleBlockChunkRenderer(_camera);
            
            _window.AddRenderAction((projectionMatrix, viewMatrix) =>
            {
                var chunkCoords = new ChunkCoords(0, 0);
                var chunk = _server.World.ChunkRepository.GetChunk(chunkCoords);
                _chunkRenderer.Render(new ChunkRenderContext(projectionMatrix, viewMatrix), chunk, chunkCoords);
            });
        }

        public Player Player { get; }

        public void Run()
        {
            _window.Run();
        }
    }
}