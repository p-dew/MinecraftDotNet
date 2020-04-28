using System.Collections.Generic;
using System.Numerics;
using MinecraftDotNet.ClientSide.Graphics.Core;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace MinecraftDotNet.ClientSide.Graphics.OpenGl
{
    public class GlMaterial : IMaterial
    {
        private readonly int _programHandle;
        private readonly IDictionary<string, int> _locations;
        
        public GlMaterial(int programHandle)
        {
            _programHandle = programHandle;
            _locations = new Dictionary<string, int>();
        }

        private int GetLocation(string name)
        {
            if (_locations.TryGetValue(name, out var location))
            {
                return location;
            }
            else
            {
                var newLocation = GL.GetUniformLocation(_programHandle, name);
                if (newLocation == -1)
                    throw new GraphicsException($"Cannot find a {name} uniform");
                
                _locations[name] = newLocation;
                return newLocation;
            }
        }

        private void UseProgram()
        {
            GL.UseProgram(_programHandle);
        }
        
        public void SetFloat(string name, float value)
        {
            UseProgram();
            
            var location = GetLocation(name);
            GL.Uniform1(location, value);
        }

        public void SetMatrix4x4(string name, Matrix4x4 value)
        {
            UseProgram();
            
            var glMatrix = new Matrix4(
                value.M11, value.M12, value.M13, value.M14,
                value.M21, value.M22, value.M23, value.M24,
                value.M31, value.M32, value.M33, value.M34,
                value.M41, value.M42, value.M43, value.M44);

            var location = GetLocation(name);
            
            GL.UniformMatrix4(location, false, ref glMatrix);
        }
    }
}