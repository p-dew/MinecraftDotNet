using System;
using OpenTK;
using OpenTK.Graphics;

namespace MinecraftDotNet.Core
{
    public class MinecraftGameWindow : GameWindow
    {
        public MinecraftGameWindow(string title, int width, int height)
            : base(width, height, GraphicsMode.Default, title)
        {
            
        }
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            
        }
        
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            
            
        }
    }
}