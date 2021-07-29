namespace MinecraftDotNet.ClientSide.Hosting

open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

open FSharp.Control.Tasks

open MinecraftDotNet.ClientSide
open MinecraftDotNet.Core.Blocks
open MinecraftDotNet.Core.Blocks.Chunks.ChunkGenerators
open MinecraftDotNet.Core.Blocks.Chunks.ChunkRepositories

type McClientHostedService(loggerFactory: ILoggerFactory) =
    let logger = loggerFactory.CreateLogger<McClientHostedService>()
    let blockInfoRepository = DefaultBlockInfoRepository(loggerFactory.CreateLogger())
    let chunkGenerator = ChessChunkGenerator((fun () -> blockInfoRepository.Air), (fun () -> blockInfoRepository.Test0))
    let chunkRepository = MemoryChunkRepository(chunkGenerator, loggerFactory.CreateLogger())
    let blockRepository = ChunkBlockRepository(chunkRepository)
    let client = new StandaloneClient(chunkRepository, blockRepository, blockInfoRepository, [blockInfoRepository])
    interface IHostedService with
        member this.StartAsync(cancellationToken) = unitTask {
            client.Run()
        }
        member this.StopAsync(cancellationToken) = unitTask {
            logger.LogInformation($"Stop {nameof McClientHostedService}")
        }


[<AutoOpen>]
module ServiceCollectionExtensions =

    open Microsoft.Extensions.DependencyInjection
    
//    type IServiceCollection with
//        do ()
    type IHostBuilder with
        member this.ConfigureMc(configureMc: unit -> unit) =
            this.ConfigureServices(fun services ->
                services.AddHostedService<McClientHostedService>() |> ignore
            )