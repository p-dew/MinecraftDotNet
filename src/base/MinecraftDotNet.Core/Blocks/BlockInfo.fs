namespace MinecraftDotNet.Core.Blocks

open System.Collections.Generic
open MinecraftDotNet.Core.Items
open ObjectTK.Textures

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

type BlockInfo =
    { ItemInfo: ItemInfo
      Sides: BlockSides }
