namespace MinecraftDotNet.ClientSide.Graphics.Core

open MinecraftDotNet.Core.Math

type Window =
    { Title: string
      Size: Sizei
      
      IsClosing: bool
      
      Runner: unit -> unit
      
      OnExit: unit -> unit
      OnRender: float -> unit }

module Window =
    let private render window =
        ()
    
    let run window =
        window.Runner()
