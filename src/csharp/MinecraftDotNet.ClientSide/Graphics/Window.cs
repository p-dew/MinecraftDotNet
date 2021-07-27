using ObjectTK.Tools;
using OpenTK.Graphics;

namespace MinecraftDotNet.ClientSide.Graphics
{
    public class Window : DerpWindow
    {
        public Window()
            : base(1024, 720, GraphicsMode.Default, "Minecraft .NET Edition")
        {
            
        }
        
    }
}