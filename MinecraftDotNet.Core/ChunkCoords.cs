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

        public override string ToString()
        {
            return $"ChunkCoords({X}, {Z})";
        }

        public override bool Equals(object obj)
        {
            if (obj is ChunkCoords coords)
            {
                return this.Equals(coords);
            }

            return false;
        }

        protected bool Equals(ChunkCoords other)
        {
            return X == other.X && Z == other.Z;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Z;
            }
        }
    }
}