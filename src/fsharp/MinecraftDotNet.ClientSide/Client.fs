namespace MinecraftDotNet.ClientSide

open MinecraftDotNet.ClientSide.Graphics.Core

type ClientApp =
    { Window: Window
       }

module Client =
    let run client =
        Window.run client.Window