namespace Ehingeeinae.Graphics.Hosting

open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open OpenTK.Graphics.OpenGL
open OpenTK.Windowing.Desktop

[<AutoOpen>]
module Utils =

    let inline ( ^ ) f x = f x

type GraphicsCommand = unit -> unit

type IGraphicsCommandSender =
    abstract Send: command: GraphicsCommand -> unit

type CommandQueueGameWindow(settings: GameWindowSettings, nativeSettings: NativeWindowSettings) as this =
    inherit GameWindow(settings, nativeSettings)

    let titleBase = this.Title

    let commandQueue = ResizeArray<GraphicsCommand>()

    let setFpsTitle () =
        this.Title <- $"{titleBase} (%.2f{1. / this.RenderTime}; (%.2f{1. / this.UpdateTime}))"

    member this.AddCommand(command: GraphicsCommand) =
        lock commandQueue ^fun () ->
            commandQueue.Add(command) |> ignore

    // override this.OnLoad() =
    //     GL.ClearColor(0.f, 0.f, 1.f, 1.f)
    //     base.OnLoad()

    override this.OnRenderFrame(args) =
        setFpsTitle ()
        base.OnRenderFrame(args)

    override this.OnUpdateFrame(args) =
        // GL.Clear(ClearBufferMask.ColorBufferBit)

        if commandQueue.Count > 0 then
            let commands =
                // Materialize
                lock commandQueue ^fun () ->
                    let commands = commandQueue.ToArray()
                    commandQueue.Clear()
                    commands
            for command in commands do
                command ()

        // this.Context.SwapBuffers()
        base.OnUpdateFrame(args)

    interface IGraphicsCommandSender with
        member this.Send(command) = this.AddCommand(command)


type GraphicsWindowHostedService(logger: ILogger<GraphicsWindowHostedService>, window: CommandQueueGameWindow) =
    let mutable renderThread = null

    let start () =
        renderThread <- Thread(fun () ->
            try
                logger.LogInformation("Running window")
                // window.MakeCurrent()
                window.Run()
            with e ->
                logger.LogError(e, "")
        )
        renderThread.Name <- "Window Render Thread"
        renderThread.Start()

    let stop () =
        (window :> IGraphicsCommandSender).Send(fun () ->
            logger.LogInformation("Closing window")
            window.Close()
        )
        renderThread.Join()

    interface IHostedService with
        member this.StartAsync(cancellationToken) =
            start ()
            Task.CompletedTask
        member this.StopAsync(cancellationToken) =
            stop ()
            Task.CompletedTask


[<AutoOpen>]
module ServiceCollectionExtensions =

    open Microsoft.Extensions.DependencyInjection

    type IServiceCollection with
        member this.AddGraphics(gameWindowSettings: GameWindowSettings, nativeWindowSettings: NativeWindowSettings) =
            this.AddSingleton(gameWindowSettings).AddSingleton(nativeWindowSettings) |> ignore

            this
                .AddSingleton<CommandQueueGameWindow>()
                .AddSingleton<NativeWindow, CommandQueueGameWindow>(fun sp -> sp.GetRequiredService<CommandQueueGameWindow>())
                .AddSingleton<IGraphicsCommandSender, CommandQueueGameWindow>(fun sp -> sp.GetRequiredService<CommandQueueGameWindow>())
            |> ignore

            this.AddHostedService<GraphicsWindowHostedService>() |> ignore
            this
