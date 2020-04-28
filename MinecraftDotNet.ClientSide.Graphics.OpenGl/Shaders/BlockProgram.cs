using ObjectTK.Shaders;
using ObjectTK.Shaders.Sources;
using ObjectTK.Shaders.Variables;
using ObjectTK.Textures;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace MinecraftDotNet.ClientSide.Graphics.OpenGl.Shaders
{
    [VertexShaderSource("BlockShader.Vertex")]
    [FragmentShaderSource("BlockShader.Fragment")]
    public class BlockProgram : Program
    {
        #nullable disable
        // Vertex
        
        [VertexAttrib(3, VertexAttribPointerType.Double)]
        public VertexAttrib InVertex { get; protected set; }
        
        [VertexAttrib(2, VertexAttribPointerType.Double)]
        public VertexAttrib InUv { get; protected set; }
        
        public Uniform<Matrix4> MvpMatrix { get; protected set; }
        
        // Fragment

        public TextureUniform<Texture2D> Side { get; protected set; }
        
        #nullable restore
    }
}