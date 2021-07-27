namespace MinecraftDotNet.Core.Worlds

open MinecraftDotNet.Core.Blocks
open MinecraftDotNet.Core.Blocks.Chunks

// IWorld

type BlockPlacedHandler = delegate of blockInfo: BlockInfo * blockCoords: BlockCoords * player: Player -> unit
type PlayerJoinedHandler = delegate of player: Player -> unit

type IWorld =
    [<CLIEvent>]
    abstract BlockPlaced: IEvent<BlockPlacedHandler>
    [<CLIEvent>]
    abstract PlayerJoined: IEvent<PlayerJoinedHandler>
    
    abstract BlockRepository: IBlockRepository
    abstract ChunkRepository: IChunkRepository

// World

type World(chunkRepository, blockRepository) =
    
    let blockPlacedEvent = Event<BlockPlacedHandler>()
    let playerJoinedEvent = Event<PlayerJoinedHandler>()
    
    interface IWorld with
        
        member _.BlockRepository = blockRepository
        member _.ChunkRepository = chunkRepository
        
        [<CLIEvent>]
        member _.BlockPlaced = blockPlacedEvent.Publish
        member _.PlayerJoined = playerJoinedEvent.Publish
