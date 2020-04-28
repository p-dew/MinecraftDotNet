namespace MinecraftDotNet.Core.Resources

open System
open System.Drawing

type ResourceId = ResourceId of string

type TextureId = TextureId of ResourceId

type Texture =
    { Id: TextureId
      Bitmap: Bitmap }
    
    interface IDisposable with
        member this.Dispose() =
            this.Bitmap.Dispose()

type SoundId = SoundId of ResourceId
type Sound =
    { Id: SoundId }
