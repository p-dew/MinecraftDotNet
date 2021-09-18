namespace Ehingeeinae

// open System
// open System.Threading
// open System.Threading.Tasks
// open Ehingeeinae.Ecs.Hosting
// open Microsoft.Extensions.Configuration
// open Microsoft.Extensions.Hosting
// open Microsoft.Extensions.DependencyInjection
// open Microsoft.Extensions.Logging
// open FSharp.Control.Tasks
// open Microsoft.Extensions.Options


// type GameHost(services: IServiceProvider) as this =
//     let logger = services.GetRequiredService<ILoggerFactory>().CreateLogger((this.Environment: IHostEnvironment).ApplicationName)
//
//     member this.Configuration = services.GetRequiredService<IConfiguration>()
//     member this.Environment = services.GetRequiredService<IHostEnvironment>()
//     member this.Logger = logger
//
//     interface IHostedService with
//         member this.StartAsync(cancellationToken) =
//             // host.StartAsync(cancellationToken)
//             Task.CompletedTask
//         member this.StopAsync(cancellationToken) =
//             // host.StopAsync(cancellationToken)
//             Task.CompletedTask
//
//
// type GameBuilder(services: IServiceCollection) =
//     let ecsBuilder = EcsBuilder(services)
//     member this.Services = services
//
//     member this.Ecs = ecsBuilder
//
//     // member this.Build() =
//     //     services.AddHostedService<GameHost>(fun sp -> GameHost(sp))
//
// [<AutoOpen>]
// module Extensions =
//     type IHostBuilder with
//         member this.ConfigureGame(configureGame: GameBuilder -> unit) =
//             this.ConfigureServices(fun services ->
//                 services.AddOptions<string>().Configure(fun x -> ())
//                 services.AddHostedService()
//                 let gameBuilder = GameBuilder(services)
//                 configureGame gameBuilder
//             ) |> ignore
//             this
//
// module pg =
//
//     let main () =
//         ServiceCollection().BuildServiceProvider()
//         let hostBuilder =
//             Host.CreateDefaultBuilder()
//                 .ConfigureGame(fun gameBuilder ->
//                     // gameBuilder.Ecs.Systems.AddSystem()
//                     ()
//                 )
//         ()
//         hostBuilder.Build().Services
//
//     ()
//
// type AnonymousHostedService(hostedService: IHostedService) =
//     interface IHostedService with
//         member this.StartAsync(cancellationToken) = hostedService.StartAsync(cancellationToken)
//         member this.StopAsync(cancellationToken) = hostedService.StopAsync(cancellationToken)
//
// module AnonymousHostedServiceServiceCollectionExtensions =
//
//     type IServiceCollection with
//         member this.AddServiceCollection() =
//             this.AddTransient<IServiceCollection>(fun _ -> this)
//
//     type IServiceCollection with
//         member this.AddAnonymousHostedService(factory: IServiceProvider -> IHostedService) =
//             this.Add(ServiceDescriptor.Singleton<IHostedService, AnonymousHostedService>(fun services -> AnonymousHostedService(factory services)))
