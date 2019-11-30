namespace MinecraftDotNet.Core
{
    public class ItemInfo
    {
        public ItemInfo(string id, int maxStack)
        {
            Id = id;
            MaxStack = maxStack;
        }
        
        public string Id { get; }
        
        public int MaxStack { get; }
    }
}