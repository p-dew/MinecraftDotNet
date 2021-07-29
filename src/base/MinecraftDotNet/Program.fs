module MinecraftDotNet.Program

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting

open MinecraftDotNet.ClientSide
open MinecraftDotNet.ClientSide.Hosting


let configureServices (services: IServiceCollection) : unit =
    services.AddLogging(fun b ->
        b.AddConsole() |> ignore
    ) |> ignore
    ()

let configureMc (mc: unit) : unit =
    ()


let createHostBuilder args =
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(configureServices)
        .ConfigureMc(configureMc)

[<EntryPoint>]
let main args =
//    printfn "Minecraft .NET Edition | 0.0.0-indev"
    
    (createHostBuilder args).Build().Run()
    
//    try
//        let client = new StandaloneClient()
//        client.Run()
//    with
//    | e ->
//        eprintfn $"%A{e}"
//        reraise ()
    0