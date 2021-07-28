module MinecraftDotNet.Program

open MinecraftDotNet.ClientSide
open log4net

[<EntryPoint>]
let main args =
    printfn "Minecraft .NET Edition | 0.0.0-indev"
    
    try
        use client = new StandaloneClient()
        client.Run()
    with
    | e ->
        eprintfn $"%A{e}"
        reraise ()
    0