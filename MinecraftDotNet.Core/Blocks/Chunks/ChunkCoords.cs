using System;

namespace MinecraftDotNet.Core.Blocks.Chunks
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

        public override string ToString() =>
            $"{nameof(ChunkCoords)}({X}, {Z})";

        #region Equality

        protected bool Equals(ChunkCoords other)
        {
            return X == other.X && Z == other.Z;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ChunkCoords) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Z);
        }
        
        #endregion
    }
}