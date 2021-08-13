namespace MinecraftDotNet.Core.Blocks

open System.Drawing
open System.Drawing
open Ehingeeinae.ResourceManagement
open Microsoft.Extensions.Logging
open MinecraftDotNet.Core
open MinecraftDotNet.Core.Items
open ObjectTK.Textures
open OpenTK.Graphics.OpenGL

type IBlockInfoRepository =
    abstract GetByItemId: itemId: ItemId -> BlockInfo

[<AutoOpen>]
module BlockInfoRepositoryExtensions =
    type IBlockInfoRepository with
        member this.Air = this.GetByItemId(ItemId "air")
        member this.Dirt = this.GetByItemId(ItemId "dirt")
        member this.Test0 = this.GetByItemId(ItemId "test0")


type DefaultBlockInfoRepository(logger: ILogger<DefaultBlockInfoRepository>, glResourceManager: IGlResourceManager) =

    let loadBlockInfo (texPath: string) id =
        logger.LogDebug($"Load new BlockInfo(ItemId = {id}; TexPath = {texPath})")
        let textureHandle = using (new Bitmap(texPath)) (fun bitmap ->
            let textureHandle =
                glResourceManager.LoadTexture(gl, bitmap, fun tex ->
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, int TextureMinFilter.Nearest)
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, int TextureMinFilter.Nearest)
                    ()
                )
            textureHandle
        )
        let itemInfo =
            { Id = ItemId id
              MaxStack = 64 }
        let blockInfo =
            { ItemInfo = itemInfo
              TextureSheet = textureHandle }
        blockInfo

    let mutable _air = Unchecked.defaultof<_>
    let mutable _dirt = Unchecked.defaultof<_>
    let mutable _test0 = Unchecked.defaultof<_>

    interface IGlInitializable with
        member this.InitGl() =
            _dirt <- loadBlockInfo "./assets/textures/dirt.png" "dirt"
            _test0 <- loadBlockInfo "./assets/textures/test0.png" "test0"

//            let airBitmap = new Bitmap(16, 16)
//            let mutable airTex = new Texture2D(SizedInternalFormat.Rgba8, airBitmap.Width, airBitmap.Height)
//            airTex.LoadBitmap(airBitmap)
//            airTex <- null
            let airTex = null
            _air <-
                { ItemInfo = { Id = ItemId "air"; MaxStack = 0 }
                  TextureSheet = airTex }

    interface IBlockInfoRepository with
        member this.GetByItemId(itemId) =
            match itemId with
            | ItemId "air" -> _air
            | ItemId "dirt" -> _dirt
            | ItemId "test0" -> _test0
            | _ -> failwithf $"Failed to get BlockInfo(ItemId = {itemId})"
