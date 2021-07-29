module MinecraftDotNet.Program

open System
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration

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
        .ConfigureAppConfiguration(fun context builder ->
            printfn $"EnvironmentName: {context.HostingEnvironment.EnvironmentName}"
            printfn $"ContentRootPath: {context.HostingEnvironment.ContentRootPath}"
        )
        .UseContentRoot("../../../")
        .ConfigureServices(configureServices)
        .ConfigureMc(configureMc)

[<EntryPoint>]
let main args =
    Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development")
    (createHostBuilder args).Build().Run()
    0
