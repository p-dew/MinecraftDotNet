using System;
using ObjectTK.Tools.Cameras;
using OpenTK;
using OpenTK.Input;

namespace MinecraftDotNet.ClientSide
{
    public class McCameraBehavior : CameraBehavior
    {
        private readonly float _sensitive;
        private readonly INativeWindow _window;
        private Point _resertCursorPoint;
        
        public McCameraBehavior(float sensitive, INativeWindow window)
        {
            _sensitive = sensitive;
            _window = window;

            _window.Resize += OnWindowResize;
            _window.Move += OnWindowMove;
        }

        public override void MouseMove(CameraState state, Vector2 delta)
        {
            base.MouseMove(state, delta);

            HandleFreeLook(state, delta * _sensitive);

            Mouse.SetPosition(_resertCursorPoint.X, _resertCursorPoint.Y);
        }

        public override void UpdateFrame(CameraState state, float step)
        {
            base.UpdateFrame(state, step);
        }

        private void UpdateResetCursorPoint()
        {
            _resertCursorPoint = _window.Location + new Size(_window.Width / 2, _window.Height / 2);
        }
        
        private void OnWindowMove(object sender, EventArgs e)
        {
            UpdateResetCursorPoint();
        }

        private void OnWindowResize(object sender, EventArgs e)
        {
            UpdateResetCursorPoint();
        }
    }
}