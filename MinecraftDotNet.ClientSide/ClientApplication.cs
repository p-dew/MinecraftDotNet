using System.Numerics;
using Microsoft.Extensions.Logging;
using MinecraftDotNet.ClientSide.Graphics.Core;
using MinecraftDotNet.ClientSide.Resources;
using MinecraftDotNet.Core;
using MinecraftDotNet.Core.Blocks.Chunks;

namespace MinecraftDotNet.ClientSide
{
    public class ClientApplication : IClientApplication
    {
        public delegate IServerApplication LocalServerFactory();

        private readonly ILogger<ClientApplication> _logger;
        private readonly McWindow _window;
        private readonly IChunkRenderer _chunkRenderer;
        private readonly Camera _camera;
        private readonly IResourceManager _resourceManager;
        private readonly LocalServerFactory _localServerFactory;

        public ClientApplication(IResourceManager resourceManager, ILogger<ClientApplication> logger, 
            IRenderersProvider renderersProvider, LocalServerFactory localServerFactory)
        {
            _logger = logger;
            _resourceManager = resourceManager;
            _localServerFactory = localServerFactory;
            _chunkRenderer = renderersProvider.ChunkRenderer;
            
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
            
            _window = new McWindow(_camera);
            _window.KeyPress += Window_KeyPress;
            
            _camera.SetBehavior(new FreeLookBehavior());
            _camera.Enable(_window);
        }

        private void Window_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 'j') 
                return;
                   
            var localServer = _localServerFactory();
            Join(localServer);
        }

        private void Join(IServerApplication serverApplication)
        {
            _logger.LogInformation($"Joining {serverApplication}...");
            
            _server = serverApplication;
            
            _window.AddRenderAction((projectionMatrix, viewMatrix) =>
            {
                var chunkCoords = new ChunkCoords(0, 0);
                var chunk = _server.World.ChunkRepository.GetChunk(chunkCoords);
                _chunkRenderer.Render(new ChunkRenderContext(projectionMatrix, viewMatrix), chunk, chunkCoords);
            });
        }

        public void Run()
        {
            _window.Run();
        }
    }
}