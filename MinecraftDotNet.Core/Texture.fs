namespace MinecraftDotNet.Core

open MinecraftDotNet.Core.Math
open OpenTK.Graphics.OpenGL4
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Advanced
open SixLabors.ImageSharp.PixelFormats

type GlId = GlId of int

type Texture(glId, size, data) =
    
    new(glId, image: Image<Rgba32>) =
        let size = { X = image.Width; Y = image.Height }
        let data = image.GetPixelSpan().ToArray() |> Seq.collect (fun x -> [| x.R; x.G; x.B |]) |> Seq.toArray
        Texture(glId, size, data)
    
    member this.Size: Vector2i = size
    member this.Data: byte array = data
    
    member this.Id: GlId = glId

type Texture1 =
    {
        Size: Vector2i
        Data: byte array
    }

module Texture =
    
    let bind (texture: Texture) =
        GL.BindTexture(TextureTarget.Texture2D, texture.Id.Item)
    
    