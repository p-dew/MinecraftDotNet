namespace MinecraftDotNet.Core
{
    public class ChunkCoords
    {
        public ChunkCoords(int x, int z)
        {
            X = x;
            Z = z;
        }

        public int X { get; }
        public int Z { get; }
    }
}