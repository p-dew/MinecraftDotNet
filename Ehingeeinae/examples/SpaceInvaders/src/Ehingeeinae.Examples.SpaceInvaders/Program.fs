open System
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

open OpenTK.Windowing.Desktop

open Ehingeeinae.Ecs.Hosting
open Ehingeeinae.Ecs.Querying
open Ehingeeinae.Ecs.Scheduling
open Ehingeeinae.Ecs.Scheduling.SystemChainBuilding

open Ehingeeinae.Graphics.Hosting

open Ehingeeinae.Examples.SpaceInvaders.Systems
open Ehingeeinae.Examples.SpaceInvaders.Components


let configureServices (services: IServiceCollection) : unit =
    services.AddEcs(fun sp ctx ->

        ctx.Resources.Register<NativeWindow>(sp.GetRequiredService<_>())

        let systemChainBuilder = ctx.SystemChain

        let logicLoop = systemChainBuilder.CreateLoop(20.f / 1000f, SystemExecutor.threadPool ())
        let renderLoop = systemChainBuilder.CreateLoop(60.f / 1000f, SystemExecutor.threadPool ())

        // < Systems

        systemChainBuilder
            .AddSystem(renderLoop, fun ctx ->
                RenderTimingSystem(ctx.Resources.GetUnique<RenderTimingState>())
            )
            .AddSystem(logicLoop, fun ctx ->
                LogicTimingSystem(ctx.Resources.GetUnique<LogicTimingState>())
            )
        |> ignore

        systemChainBuilder.AddSystem(logicLoop, fun ctx ->
            InputSystem(
                ctx.Resources.GetShared<NativeWindow>(),
                ctx.Resources.GetUnique<InputState>()
            )
        ) |> ignore

        systemChainBuilder.AddSystem(logicLoop, fun ctx ->
            PlayerControlSystem(
                ctx.Resources.GetShared<InputState>(),
                ctx.Queries.CreateQuery<_>(),
                sp.GetRequiredService<EcsWorldQueryExecutor>()
            )
        ) |> ignore

        systemChainBuilder.AddSystem(logicLoop, fun ctx ->
            PhysicsSystem(
                ctx.Queries.CreateQuery<_>(),
                sp.GetRequiredService<EcsWorldQueryExecutor>(),
                ctx.Resources.GetShared<LogicTimingState>()
            )
        ) |> ignore

        systemChainBuilder.AddSystem(renderLoop, fun ctx ->
            RenderSystem(
                ctx.Queries.CreateQuery<_>(),
                sp.GetRequiredService<EcsWorldQueryExecutor>()
            )
        ) |> ignore

        // Systems >

    ) |> ignore
    services.AddGraphics(
        GameWindowSettings(RenderFrequency=60.),
        NativeWindowSettings()
    ) |> ignore
    services.AddSingleton<IEcsWorldSeeder>(fun _ ->
        { new IEcsWorldSeeder with
            member _.Seed(wm) =
                wm.AddEntities([
                    { Position.X = 2f; Y = 2f }, { Velocity.dx = 0.f; dy = 0.f }, { Text = "@" }, Player()
                ]) |> ignore
        }
    ) |> ignore


[<EntryPoint>]
let main args =
    Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development")
    let hostBuilder =
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging(fun logging ->
                logging.AddConsole() |> ignore
            )
            .ConfigureServices(configureServices)

    hostBuilder.Build().Run()
    0
