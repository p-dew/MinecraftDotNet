namespace Ehingeeinae.ResourceManagement

open System
open System.Collections.Generic
open System.Drawing
open System.IO
open System.Runtime.CompilerServices
open Microsoft.Extensions.FileProviders

(*

- Перезагрузка ассетов
- Менеджмент ассетов, используемых ресурсов, информация для дебага
- Отложенная загрузка и всё в таком духе

*)

type IAssetHandle<'TAsset> =
    inherit IDisposable
    abstract Value: 'TAsset

type IAssetMonitor<'TAsset> =
    inherit IDisposable
    abstract CurrentValue: 'TAsset
    abstract OnChange: ('TAsset -> unit) -> IDisposable

[<Struct>]
type BitmapAssetId = BitmapAssetId of Guid

type BitmapAsset =
    { Id: BitmapAssetId
      Bitmap: Bitmap }

type IBitmapRepository =
    abstract BeginUseAsset: BitmapAssetId -> Async<IAssetHandle<BitmapAsset>>

    // ----

    abstract LoadBitmap: path: string -> Async<IAssetMonitor<BitmapAsset>>
    abstract UnloadBitmap: id: BitmapAssetId -> Async<unit>
    abstract Get: id: BitmapAssetId -> Async<IAssetMonitor<BitmapAsset>>
    abstract Reload: id: BitmapAssetId * path: string -> Async<unit>

type RepositoryAssetMonitor<'TAsset> =
    new(initAsset) = { Asset = initAsset }

    val mutable Asset: 'TAsset

    interface IAssetMonitor<'TAsset> with
        member this.CurrentValue = this.Asset
        member this.Dispose() = failwith "todo"
        member this.OnChange(var0) = failwith "todo"

type BitmapRepository(fileProvider: IFileProvider) =
//    let loadedAssets = Dictionary<BitmapAssetId, BitmapAsset * int>()
    let monitors = Dictionary<BitmapAssetId, ResizeArray<RepositoryAssetMonitor<BitmapAsset>>>()

    interface IBitmapRepository with
        member this.LoadBitmap(path) = async {
            let fileInfo = fileProvider.GetFileInfo(path)
            if not fileInfo.Exists then raise <| FileNotFoundException()

            use readStream = fileInfo.CreateReadStream()
            let bitmap = new Bitmap(readStream)
            let assetId = BitmapAssetId (Guid.NewGuid())
            let asset = { Id = assetId; Bitmap = bitmap }

            let monitor = new RepositoryAssetMonitor<_>(asset)
            monitors.[assetId] <-
            return monitor
        }
        member this.Reload(id, path) = failwith "todo"
        member this.UnloadBitmap(id) = failwith "todo"




[<Struct; IsByRefLike; NoComparison; NoEquality>]
type GlContext =
    struct
        member _.Foo() = ()
    end

type GlFunc<'a> = delegate of GlContext inref -> 'a

type IGlExecutor =
    abstract Execute: GlFunc<'a> -> Async<'a>


type GlTextureHandle = int
type GlShaderProgramHandle = int

type IGlResourceManager =
    abstract LoadTexture: bitmap: Bitmap -> Async<GlTextureHandle>
    abstract LoadShaderProgram: vertexShaderSource: string * fragmentShaderSource: string -> Async<GlShaderProgramHandle>

module Impl =

    open System.Drawing.Imaging
    open Ehingeeinae.Shaders
    open OpenTK.Graphics.OpenGL

    type GlResourceManager(glExecutor: IGlExecutor) =

        let loadTextureFromBitmap (gl: GlContext inref) (bitmap: Bitmap) =
            let bitmapData = bitmap.LockBits(Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat)
            let level = 0
            let pixelFormat = PixelFormat.Bgra
            let pixelType = PixelType.UnsignedByte
            GL.TexSubImage2D(TextureTarget.Texture2D, level, 0, 0, bitmapData.Width, bitmapData.Height, pixelFormat, pixelType, bitmapData.Scan0)
            bitmap.UnlockBits(bitmapData)

        interface IGlResourceManager with
            member this.LoadTexture(bitmap) = async {
                let textureTarget = TextureTarget.Texture2D
                let! textureHandle = glExecutor.Execute(fun gl ->
                    let textureHandle = GL.GenTexture()
                    GL.BindTexture(textureTarget, textureHandle)
                    loadTextureFromBitmap &gl bitmap
                    textureHandle
                )

                return textureHandle
            }

            member this.LoadShaderProgram(vertexShaderSource, fragmentShaderSource) = async {
                let! shaderProgramHandle = glExecutor.Execute(fun gl ->
                    ShaderProgram.create vertexShaderSource fragmentShaderSource
                )
                return shaderProgramHandle
            }
