using MinecraftDotNet.Core.Blocks.Chunks;
using MinecraftDotNet.Core.Math;

namespace MinecraftDotNet.Core.Blocks
{
    public class ChunkBlockRepository : IBlockRepository
    {
        private readonly IChunkRepository _chunkRepository;

        public ChunkBlockRepository(IChunkRepository chunkRepository)
        {
            _chunkRepository = chunkRepository;
        }

        private Chunk GetChunkByBlockCoords(Coords3 blockCoords)
        {
            var chunk = _chunkRepository.GetChunk(new ChunkCoords(blockCoords.X / Chunk.Width, blockCoords.Z / Chunk.Depth));
            return chunk;
        }

        public BlockInfo GetBlock(Coords3 coords)
        {
            var chunk = GetChunkByBlockCoords(coords);
            var localBlockCoords = chunk.GetLocalBlockCoords(coords);
            return chunk.Blocks[localBlockCoords.X, localBlockCoords.Y, localBlockCoords.Z];
        }

        public Meta GetBlockMeta(Coords3 coords)
        {
            var chunk = GetChunkByBlockCoords(coords);
            var localBlockCoords = chunk.GetLocalBlockCoords(coords);
            
            // Return empty meta if there is no meta
            if (!chunk.BlockMetas.ContainsKey(localBlockCoords))
                return Meta.Empty;

            return chunk.BlockMetas[localBlockCoords];
        }

        public void SetBlock(BlockInfo blockInfo, Coords3 coords)
        {
            var chunk = GetChunkByBlockCoords(coords);
            var localBlockCoords = chunk.GetLocalBlockCoords(coords);
            chunk.Blocks[localBlockCoords.X, localBlockCoords.Y, localBlockCoords.Z] = blockInfo;
        }

        public void SetBlockMeta(Meta meta, Coords3 coords)
        {
            var chunk = GetChunkByBlockCoords(coords);
            var localBlockCoords = chunk.GetLocalBlockCoords(coords);
            chunk.BlockMetas[localBlockCoords] = meta;
        }
    }
}