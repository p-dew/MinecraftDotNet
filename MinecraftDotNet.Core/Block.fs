namespace MinecraftDotNet.Core


//type BlockInfo() =
//    do ()

type BlockInfo(texture) =
    
    member _.Texture: Texture = texture


type Block(info) =
    
    interface IRenderable
    
    member _.Info: BlockInfo ref = info


type Chunk(blocks) =
    member this.Blocks: Block[,,] = blocks


type Terrain(chunks) =
    member this.Chunks: Chunk[,] = chunks


type Player = unit


type World(terrain, player) =
    member this.Terrain: Terrain = terrain




type BlockRenderer() =
    interface IRenderer<Block> with
        member this.Render(block) =
            ()