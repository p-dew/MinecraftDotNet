namespace MinecraftDotNet.Core.Items
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

        #region Equation

        protected bool Equals(ItemInfo other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ItemInfo) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        
        #endregion
    }
}