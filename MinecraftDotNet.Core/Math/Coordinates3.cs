namespace MinecraftDotNet.Core.Math
{
    public class Coordinates3
    {
        public Coordinates3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        
        public ChunkCoords ToChunkCoords()
        {
            return new ChunkCoords(X / Chunk.Width, Z / Chunk.Depth);
        }
    }
}