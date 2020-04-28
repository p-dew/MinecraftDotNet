using System;

namespace MinecraftDotNet.Core.Resources
{
    public class ResourceId
    {
        public ResourceId(string name)
        {
            Name = name;
        }
        
        public string Name { get; }
        
        #region Equality

        protected bool Equals(ResourceId other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ResourceId) obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        #endregion
    }
}