using MinecraftDotNet.ClientSide.Graphics;
using MinecraftDotNet.Core;
using MinecraftDotNet.Core.Blocks;
using MinecraftDotNet.Core.Blocks.Chunks;
using MinecraftDotNet.Core.Worlds;
using ObjectTK.Tools.Cameras;
using OpenTK;
using System;
using System.Data;
using ObjectTK;
using ObjectTK.Tools;
using OpenTK.Graphics.OpenGL;

namespace MinecraftDotNet.ClientSide
{
    public class StandaloneClient : IClient
    {
        private readonly DerpWindow _window;
        private readonly IWorld _currentWorld;
        private SingleBlockChunkRenderer _chunkRenderer;
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
            _camera.State.Up.Y = 1000;
            
            _window.Load += OnLoad;
            _window.Unload += OnUnload;
            _window.RenderFrame += OnWindowRenderFrame;
            _window.Closed += OnWindowClosed;
        }

        private void OnLoad(object sende, EventArgs e) {
            _chunkRenderer = new SingleBlockChunkRenderer(_camera);
        }

        private void OnUnload(object sender, EventArgs e) {

        }

        private void OnWindowClosed(object sender, EventArgs e) {

        }

        private void OnWindowRenderFrame(object sender, FrameEventArgs e)
        {
            _window.Title = string.Format("MC.NET - FPS {0}", _window.RenderFrequency);

            GL.ClearColor(Color.MidnightBlue);
            GL.Viewport(0, 0, _window.Width, _window.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //_window.SetupPerspective();

            _camera.Update();
            
            

            var chunkCoords = new ChunkCoords(0, 0);
            var chunk = _currentWorld.ChunkRepository.GetChunk(chunkCoords);
            _chunkRenderer.Render(new ChunkRenderContext(), chunk, chunkCoords);

            _window.SwapBuffers();
        }

        public void Run()
        {
            _window.Run();
        }
    }
}