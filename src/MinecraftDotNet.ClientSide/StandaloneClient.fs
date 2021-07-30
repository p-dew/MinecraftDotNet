namespace MinecraftDotNet.ClientSide

open System

open System.Threading
open Microsoft.Extensions.Logging

open ObjectTK.Tools.Cameras
open OpenTK

open MinecraftDotNet.Core
open MinecraftDotNet.Core.Blocks
open MinecraftDotNet.Core.Blocks.Chunks
open MinecraftDotNet.Core.Blocks.Chunks.ChunkGenerators
open MinecraftDotNet.Core.Blocks.Chunks.ChunkRepositories
open MinecraftDotNet.Core.Worlds

open MinecraftDotNet.ClientSide.Graphics


type StandaloneClient(world: IWorld, glDeps: IGlInitializable seq, onClose) =

    let camera =
        Camera(
            State =
                CameraState(
                    Position = Vector3(2f, 2f, 2f),
                    LookAt = Vector3.One
                )
        )

    let chunkRenderer = new SingleBlockChunkRenderer(camera)

    let directionHelper = new DirectionHelper()

    do camera.MoveSpeed <- camera.MoveSpeed / 2f
    do camera.MouseMoveSpeed <- camera.MouseMoveSpeed / 2f
    do camera.SetBehavior(FreeLookBehavior())

    let mutable window: McGameWindow = Unchecked.defaultof<_>

    let windowThread =
        Thread(fun () ->
            window <-
                new McGameWindow(
                    camera, fun () ->
                        [
                            yield! glDeps
                            chunkRenderer
                            directionHelper
                        ] |> Seq.iter (fun g -> g.InitGl())
                )
            camera.Enable(window)
            window.AddRenderAction(fun viewProjection ->
                let chunkCoords: ChunkCoords = { X = 0; Z = 0 }
                let chunk = world.ChunkRepository.GetChunk(chunkCoords)
                let context = { ProjectionMatrix = viewProjection.Projection; ViewMatrix = viewProjection.View }
                (chunkRenderer :> IChunkRenderer).Render(context, chunk, chunkCoords)
            )
            window.AddRenderAction(fun viewProjection ->
                let rotation = viewProjection.View.ClearTranslation()
                directionHelper.Render(viewProjection.Projection, rotation)
            )

            window.Closed.Add(ignore >> onClose)

            window.Run()
        )

    member _.Start() =
        windowThread.Start()

    member this.Stop(): unit =
        window.Close()
        windowThread.Join()

    interface IDisposable with
        member this.Dispose() =
            window.Dispose()
            (chunkRenderer :> IDisposable).Dispose()
