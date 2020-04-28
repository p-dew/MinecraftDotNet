using System.Drawing;
using MinecraftDotNet.Core.Items;
using MinecraftDotNet.Core.Resources;

namespace MinecraftDotNet.Core.Blocks
{
    // TODO: Get rid it
    public static class HcBlocks
    {
        public static BlockInfo Air { get; }
        
        public static BlockInfo Dirt { get; }
        
        public static BlockInfo Test0 { get; }

        private static BlockInfo LoadBlockInfo(string texPath, ResourceId id)
        {
            var tex = new Texture(id, new Bitmap(texPath));
            
            return new BlockInfo(new ItemInfo(id.Name, 64), new BlockSides(new[]{tex,tex,tex,tex,tex,tex}));
        }
        
        static HcBlocks()
        {
            Dirt = LoadBlockInfo("./assets/textures/dirt.png", new ResourceId("dirt"));
            Test0 = LoadBlockInfo("./assets/textures/test0.png", new ResourceId("test0"));
            
            Air = new BlockInfo(new ItemInfo("air", 0), new BlockSides(new Texture[]{null!,null!,null!,null!,null!,null!,}));
        }
    }
}