namespace Ehingeeinae.ResourceManagement

open System.Drawing
open System.Runtime.CompilerServices

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
