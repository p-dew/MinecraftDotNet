using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;

namespace MinecraftDotNet.ClientSide
{
    public class MinecraftGameWindow : GameWindow
    {
        public MinecraftGameWindow(string title, int width, int height)
            : base(width, height, GraphicsMode.Default, title)
        {
            
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            
            SwapBuffers();
        }
    }
}