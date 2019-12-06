using ObjectTK.Tools;
using OpenTK;
using OpenTK.Graphics;

namespace MinecraftDotNet.Core.Graphics
{
    public class Window : DerpWindow
    {
        public Window()
            : base(1024, 720, GraphicsMode.Default, "Minecraft .NET Edition")
        {
            
        }
        
    }
}