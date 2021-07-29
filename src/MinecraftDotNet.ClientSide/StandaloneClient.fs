namespace MinecraftDotNet.ClientSide

open System

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


type StandaloneClient(chunkRepository, blockRepository, blockInfoRepository, glDeps: IGlInitializable seq) =
    
    let camera =
        Camera(
            State =
                CameraState(
                    Position = Vector3(2f, 2f, 2f),
                    LookAt = Vector3.One
                )
        )
    
//    let loggerFactory =
//        LoggerFactory.Create(fun builder ->
//            builder.AddConsole() |> ignore
//            builder.AddSimpleConsole() |> ignore
//        )
//    
//    let blockInfoRepository = DefaultBlockInfoRepository(loggerFactory.CreateLogger())
//    let chunkGenerator = ChessChunkGenerator((fun () -> blockInfoRepository.Air), (fun () -> blockInfoRepository.Test0))
//    let chunkRepository = MemoryChunkRepository(chunkGenerator, loggerFactory.CreateLogger())
//    let blockRepository = ChunkBlockRepository(chunkRepository)
    let currentWorld = World(chunkRepository, blockRepository)
    
    let chunkRenderer = new SingleBlockChunkRenderer(camera)
    
    let window =
        new McGameWindow(
            camera, fun () ->
                glDeps |> Seq.iter (fun g -> g.InitGl())
                (chunkRenderer :> IGlInitializable).InitGl()
        )
    
    do camera.MoveSpeed <- camera.MoveSpeed / 2f
    do camera.MouseMoveSpeed <- camera.MouseMoveSpeed / 2f
    do camera.SetBehavior(FreeLookBehavior())
    
    do camera.Enable(window)
    do window.AddRenderAction(fun projection modelView ->
        let chunkCoords: ChunkCoords = { X = 0; Z = 0 }
        let chunk = (chunkRepository :> IChunkRepository).GetChunk(chunkCoords)
        let context = { ProjectionMatrix = projection; ViewMatrix = modelView }
        (chunkRenderer :> IChunkRenderer).Render(context, chunk, chunkCoords)
    )
    
    member _.Run() =
        window.Run()
    
    interface IDisposable with
        member this.Dispose() =
            window.Dispose()
            (chunkRenderer :> IDisposable).Dispose()
