module MinecraftDotNet.Core.HcBlocks

open System.Drawing
open MinecraftDotNet.Core.Blocks
open MinecraftDotNet.Core.Items
open ObjectTK.Textures
open OpenTK.Graphics.OpenGL

let private loadBlockInfo (texPath: string) id =
    let bitmap = new Bitmap(texPath)
    let tex = new Texture2D(SizedInternalFormat.Rgba8, bitmap.Width, bitmap.Height)
    tex.SetParameter(TextureParameterName.TextureMinFilter, int TextureMinFilter.Nearest)
    tex.SetParameter(TextureParameterName.TextureMagFilter, int TextureMagFilter.Nearest)
    tex.LoadBitmap(bitmap)
    let itemInfo =
        { Id = ItemId id
          MaxStack = 64 }
    let blockInfo =
        { ItemInfo = itemInfo
          Sides = BlockSides(Array.replicate 6 tex) }
    blockInfo

let mutable private _air = Unchecked.defaultof<_>
let mutable private _dirt = Unchecked.defaultof<_>
let mutable private _test0 = Unchecked.defaultof<_>

let init () =
    _dirt <- loadBlockInfo "./assets/textures/dirt.png" "dirt"
    _test0 <- loadBlockInfo "./assets/textures/test0.png" "test0"
    
    let airBitmap = new Bitmap(16, 16)
    let mutable airTex = new Texture2D(SizedInternalFormat.Rgba8, airBitmap.Width, airBitmap.Height)
    airTex.LoadBitmap(airBitmap)
    airTex <- null
    _air <-
        { ItemInfo = { Id = ItemId "air"; MaxStack = 0 }
          Sides = BlockSides(Array.replicate 6 airTex) }

let air = _air
let dirt = _dirt
let test0 = _test0
