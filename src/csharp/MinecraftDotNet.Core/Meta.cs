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
    }
}