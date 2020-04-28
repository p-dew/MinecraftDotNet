using System.Drawing;

namespace MinecraftDotNet.Core.Resources
{
    public class TextureId
    {
        public TextureId(ResourceId resourceId)
        {
            ResourceId = resourceId;
        }
    
        public TextureId(string name)
        {
            ResourceId = new ResourceId(name);
        }
        
        public ResourceId ResourceId { get; }
        public string Name => ResourceId.Name;
        
        #region Equality
    
        protected bool Equals(TextureId other)
        {
            return ResourceId.Equals(other.ResourceId);
        }
    
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextureId) obj);
        }
    
        public override int GetHashCode()
        {
            return ResourceId.GetHashCode();
        }
    
        #endregion
    }
    
    public class Texture : IResource
    {
        public Texture(ResourceId resourceId, Bitmap bitmap)
        {
            Id = resourceId;
            Bitmap = bitmap;
        }

        public ResourceId Id { get; }

        public Bitmap Bitmap { get; }

        public void Dispose()
        {
            Bitmap.Dispose();
        }
    }
}