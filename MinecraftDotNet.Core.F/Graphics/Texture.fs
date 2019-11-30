namespace MinecraftDotNet.Core.Graphics

open OpenTK.Graphics.OpenGL4
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Advanced
open SixLabors.ImageSharp.PixelFormats
open MinecraftDotNet.Core.Math
open MinecraftDotNet.Core.Graphics.OpenGl


type Texture =
    {
        Id: GlHandler
        Size: Vector2i
        Data: byte array
    }

module Texture =
    
    let bind ({ Id = GlHandler id }: Texture) =
        GL.BindTexture(TextureTarget.Texture2D, id)
    
    let fromImage id (image: Image<Rgba32>) : Texture =
        let size = { X = image.Width; Y = image.Height }
        let data = image.GetPixelSpan().ToArray() |> Seq.collect (fun x -> [| x.R; x.G; x.B |]) |> Seq.toArray
        {
            Id = id
            Size = size
            Data = data
        }
