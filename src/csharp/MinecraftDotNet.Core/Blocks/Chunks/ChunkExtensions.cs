using MinecraftDotNet.Core.Math;

namespace MinecraftDotNet.Core.Blocks.Chunks
{
    public static class ChunkExtensions
    {
        public static Coords3 GetLocalBlockCoords(this Chunk chunk, Coords3 globalBlockCoords)
        {
            var localBlockCoords = new Coords3(
                globalBlockCoords.X % Chunk.Width, 
                globalBlockCoords.Y % Chunk.Height, 
                globalBlockCoords.Z % Chunk.Depth);
            return localBlockCoords;
        }
    }
}