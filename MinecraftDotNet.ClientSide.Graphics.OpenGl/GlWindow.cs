using System;
using MinecraftDotNet.ClientSide.Graphics.Core;
using MinecraftDotNet.ClientSide.Graphics.OpenGl.Extensions;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Size = System.Drawing.Size;

namespace MinecraftDotNet.ClientSide.Graphics.OpenGl
{
    public class GlWindow : GameWindow, IWindow
    {
        public GlWindow(string title, Size size, IGlGraphics glGraphics)
            : base(size.Width, size.Height, GraphicsMode.Default, title)
        {
            _glGraphics = glGraphics;
        }

        private readonly IGlGraphics _glGraphics;
        public IGraphics Graphics => _glGraphics;

        public new Size Size => base.Size.ToSys();
        
        public event FrameRenderedHandler FrameRendered;
        public event LoadedHandler Loaded;
        public event ResizedHandler Resized;

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            SwapBuffers();
            
            FrameRendered?.Invoke(e.Time);
        }

        protected override void OnLoad(EventArgs e)
        {
            // base.OnLoad(e);
            Loaded?.Invoke();
        }

        protected override void OnResize(EventArgs e)
        {
            var fromSize = this.Size;
            base.OnResize(e);
            var toSize = this.Size;
            Resized?.Invoke(fromSize, toSize);
        }
    }
}