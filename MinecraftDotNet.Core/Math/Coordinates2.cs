namespace MinecraftDotNet.Core.Math
{
    public class Coordinates2
    {
        public Coordinates2(int x, int z)
        {
            X = x;
            Z = z;
        }

        public int X { get; }
        public int Z { get; }

        public ChunkCoords ToChunkCoords()
        {
            return new ChunkCoords(X / Chunk.Width, Z / Chunk.Depth);
        }
    }
}