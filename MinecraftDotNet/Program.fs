// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Reflection
open MinecraftDotNet.Core
open OpenTK
open OpenTK.Graphics

[<EntryPoint>]
let main argv =
    Directory.SetCurrentDirectory("../../../")
    use window = new GameWindow(1024, 720, GraphicsMode.Default, "MinecraftDotNet")
    let game = Game(window)
    game.Start()
    0