namespace MinecraftDotNet.ClientSide.Hosting

open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

open FSharp.Control.Tasks

open MinecraftDotNet.ClientSide
open MinecraftDotNet.ClientSide.Graphics
open MinecraftDotNet.Core.Blocks
open MinecraftDotNet.Core.Blocks.Chunks.ChunkGenerators
open MinecraftDotNet.Core.Blocks.Chunks.ChunkRepositories

type McClientHostedService(loggerFactory: ILoggerFactory, lifetime: IHostApplicationLifetime) =
    let logger = loggerFactory.CreateLogger<McClientHostedService>()
    let blockInfoRepository = DefaultBlockInfoRepository(loggerFactory.CreateLogger())
    let chunkGenerator = SingleBlockChunkGenerator((fun () -> blockInfoRepository.Air), (fun () -> blockInfoRepository.Test0)) // ChessChunkGenerator((fun () -> blockInfoRepository.Air), (fun () -> blockInfoRepository.Test0))
    let chunkRepository = MemoryChunkRepository(chunkGenerator, loggerFactory.CreateLogger())
    let blockRepository = ChunkBlockRepository(chunkRepository)

    let onClosed () =
        lifetime.StopApplication()

    let client = new StandaloneClient(chunkRepository, blockRepository, blockInfoRepository, [blockInfoRepository], onClosed)

    interface IHostedService with
        member this.StartAsync(cancellationToken) = unitTask {
            logger.LogInformation($"Run {nameof McClientHostedService}")
            client.Start()
        }
        member this.StopAsync(cancellationToken) = unitTask {
            logger.LogInformation($"Stop {nameof McClientHostedService}")
            client.Stop()
        }


[<AutoOpen>]
module Extensions =

    open Microsoft.Extensions.DependencyInjection

    type IHostBuilder with
        member this.ConfigureMc(configureMc: unit -> unit) =
            this.ConfigureServices(fun services ->
                services.AddHostedService<McClientHostedService>() |> ignore
            )
