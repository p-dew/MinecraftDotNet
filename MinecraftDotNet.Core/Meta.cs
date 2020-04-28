namespace MinecraftDotNet.Core
{
    public class Meta
    {
        public static Meta Empty { get; } = new Meta("");

        public Meta(string content)
        {
            Content = content;
        }

        public string Content { get; }

        #region Equality

        protected bool Equals(Meta other)
        {
            return Content == other.Content;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Meta) obj);
        }

        public override int GetHashCode()
        {
            return Content.GetHashCode();
        }

        #endregion
    }
}