[<AutoOpen>]
module MinecraftDotNet.Core.World.Types

open System.Collections.Generic
open MinecraftDotNet.Core
open MinecraftDotNet.Core.Math.Linear

// Block

type BlockId = BlockId of string

type BlockInfo =
    { Id: BlockId
      Name: string }

// Chunk

type BlockCoords = Vector3<int>

type Block =
    { BlockInfo: BlockInfo
      Meta: Meta }

type Position = Vector3f

type Entity =
    { Position: Position }

type Chunk =
    { BlockInfos: BlockInfo[,,]
      BlockMetas: IDictionary<BlockCoords, Meta>
      Entities: IList<Entity> }
    static member Width  = 16  // X
    static member Height = 256 // Y
    static member Depth  = 16  // Z


type Player =
    { Name: string
       }

type ChunkCoords = Vector2i

type Dimension =
    { Name: string
      Chunks: IDictionary<ChunkCoords, Chunk> }

type Tick = int64
module Tick =
    let unit: Tick = 1L

type World =
    { Name: string
      Time: Tick
      Dimensions: IList<Dimension> }