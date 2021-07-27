namespace MinecraftDotNet.Core.Blocks

open System.Collections.Generic
open ObjectTK.Textures
open MinecraftDotNet.Core
open MinecraftDotNet.Core.Items

// BlockSides

type BlockSides(textures: IReadOnlyList<Texture2D>) =
    do if textures.Count <> 6
        then invalidArg (nameof textures) "Block must have only 6 sides."
    member this.Textures = textures
    member this.RightTexture = this.Textures.[0]
    member this.LeftTexture  = this.Textures.[1]
    member this.UpTexture    = this.Textures.[2]
    member this.DownTexture  = this.Textures.[3]
    member this.FrontTexture = this.Textures.[4]
    member this.BackTexture  = this.Textures.[5]

// BlockInfo

type BlockInfo =
    { ItemInfo: ItemInfo
      Sides: BlockSides }

//

type BlockCoords =
    { X: int; Y: int; Z: int }

// IBlockQueryHandler

type IBlockQueryHandler =
    abstract GetBlock: coords: BlockCoords -> BlockInfo
    abstract GetBlockMeta: coords: BlockCoords -> Meta

// IBlockCommandHandler
type IBlockCommandHandler =
    abstract SetBlock: blockInfo: BlockInfo * coords: BlockCoords -> unit
    abstract SetBlockMeta: meta: Meta * coords: BlockCoords -> unit

// IBlockRepository
type IBlockRepository =
    inherit IBlockQueryHandler
    inherit IBlockCommandHandler
