using System;
using System.Collections.Generic;
using ObjectTK.Tools;
using ObjectTK.Tools.Cameras;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace MinecraftDotNet.ClientSide
{
    public class McGameWindow : DerpWindow
    {
        public delegate void RenderAction(Matrix4 projection, Matrix4 modelView);
        
        private readonly Camera _camera;
        
        private Matrix4 _projectionMatrix;
        private Matrix4 _viewMatrix;
        
        private readonly Queue<RenderAction> _renderActions;

        public McGameWindow(Camera camera)
            : base(1024, 720, GraphicsMode.Default, "Minecraft .NET Edition")
        {
            _camera = camera;
            _renderActions = new Queue<RenderAction>();
        }

        public void AddRenderAction(RenderAction action)
        {
            _renderActions.Enqueue(action);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            GL.ClearColor(Color.MidnightBlue);
            
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            
            Title = $"MC.NET - FPS {Math.Round(RenderFrequency)}";
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            SetupPerspective();
            
            _camera.Update();
            
            foreach (var renderAction in _renderActions)
            {
                renderAction(_projectionMatrix, _viewMatrix);
            }
            
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            GL.Viewport(0, 0, Width, Height);
        }
        
        /// <summary>
        /// Sets a perspective projection matrix and applies the camera transformation on the modelview matrix.
        /// </summary>
        protected void SetupPerspective()
        {
            // setup perspective projection
            var aspectRatio = Width / (float) Height;
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 0.1f, 1000);
            _viewMatrix = Matrix4.Identity;
            // apply camera transform
            _viewMatrix = _camera.GetCameraTransform();
        }
    }
}