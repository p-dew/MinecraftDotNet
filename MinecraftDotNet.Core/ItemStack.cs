namespace MinecraftDotNet.Core
{
    public class ItemStack
    {
        public ItemStack(ItemInfo info, int count)
        {
            Count = count;
            Info = info;
        }
        
        public ItemInfo Info { get; }
        public int Count { get; }
    }
}