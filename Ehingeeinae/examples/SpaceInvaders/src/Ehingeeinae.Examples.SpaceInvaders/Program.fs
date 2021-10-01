open System
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

open Ehingeeinae.Ecs.Hosting
open Ehingeeinae.Ecs.Scheduling
open Ehingeeinae.Ecs.Scheduling.SystemChainBuilding

open Ehingeeinae.Examples.SpaceInvaders.Systems


let configureServices (services: IServiceCollection) : unit =
    services.AddEcs(fun sp ctx ->
        let systemChainBuilder = ctx.SystemChain

        let logicLoop = systemChainBuilder.CreateLoop(20.f, SystemExecutor.threadPool ())
        let renderLoop = systemChainBuilder.CreateLoop(60.f, SystemExecutor.threadPool ())

        systemChainBuilder
            .AddSystem(logicLoop, fun ctx ->
                RenderTimingSystem(ctx.Resources.GetUnique<RenderTimingState>())
            )
            .AddSystem(renderLoop, fun ctx ->
                LogicTimingSystem(ctx.Resources.GetUnique<LogicTimingState>())
            )
        |> ignore

        systemChainBuilder.AddSystem(renderLoop, fun ctx ->
            RenderSystem(ctx.Queries.CreateQuery<_>(), sp.GetRequiredService<_>())
        ) |> ignore

        // systemChainBuilder.AddSystem(logicLoop, fun ctx ->
        //     let options = { CriticalY = 20f }
        //     LoseSystem(ctx.Queries.CreateQuery<_>(), sp.GetRequiredService<_>(), options)
        // ) |> ignore
        ()
    ) |> ignore
    services.AddSingleton<IEcsWorldSeeder>(fun _ -> { new IEcsWorldSeeder with member _.Seed(wm) = () }) |> ignore


[<EntryPoint>]
let main args =
    let hostBuilder =
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging(fun logging ->
                logging.AddConsole() |> ignore
            )
            .ConfigureServices(configureServices)

    hostBuilder.Build().Run()
    0
