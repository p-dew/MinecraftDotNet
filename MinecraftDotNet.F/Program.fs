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
    use game = new Game()
    game.Start()
    0