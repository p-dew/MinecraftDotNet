namespace Ehingeeinae.Ecs.ServiceProviding

open System
open System.Collections.Generic
open Ehingeeinae.Ecs.Querying
open Ehingeeinae.Ecs.Systems
open Microsoft.Extensions.DependencyInjection

// module ServiceDescriptor =
//
//
//     let (|Factory|Type|Instance|) (descriptor: ServiceDescriptor) =
//         if descriptor.ImplementationFactory <> null then
//             Factory descriptor.ImplementationFactory
//         elif descriptor.ImplementationType <> null then
//             Type descriptor.ImplementationType
//         else
//             Instance descriptor.ImplementationInstance
//
// [<RequireQualifiedAccess>]
// type EcsDependency =
//     | Query of QueryArgument
//     | Resource of unit
//
// type IEcsDependencyProvider =
//     abstract GetEcsDependencies: IEcsSystem -> IReadOnlyCollection<EcsDependency>
//
// // ----
//
// type EcsServiceProvider(defaultServiceProvider: IServiceProvider, ecsSystemDescriptors: ServiceDescriptor seq) =
//
//     let dependencies = Dictionary<IEcsSystem, ResizeArray<EcsDependency>>()
//
//     let ecsDependencyProvider =
//         { new IEcsDependencyProvider with
//             member _.GetEcsDependencies(system) = upcast dependencies.[system]
//         }
//
//     let resolveEcsService (descriptor: ServiceDescriptor) =
//         match descriptor with
//         | ServiceDescriptor.Instance instance -> instance
//         | ServiceDescriptor.Factory factory ->
//             let serviceProvider =
//                 { new IServiceProvider with
//                     member _.GetService(serviceType) =
//                         ()
//                 }
//             factory.Invoke(serviceProvider)
//
//             ()
//         | ServiceDescriptor.Type typ ->
//             ()
//
//     let ecsServiceCache = Dictionary<Type, obj>()
//
//     let getEcsService (ecsServiceType: Type) =
//         match ecsServiceCache.TryGetValue(ecsServiceType) with
//         | true, ecsService -> ecsService
//         | false, _ ->
//             let ecsService = resolveEcsService descriptor
//             ecsServiceCache.[ecsServiceType] <- ecsService
//             ecsService
//
//     let isEcsSystemType (typ: Type) =
//         typ.IsAssignableTo(typeof<IEcsSystem>)
//         || false
//
//     let isEcsDependencyType (typ: Type) =
//         typ.GetGenericTypeDefinition() = typedefof<IEcsQuery<_>>
//         || false
//
//     interface IServiceProvider with
//         member this.GetService(serviceType) =
//             if isEcsSystemType serviceType then
//                 getEcsService serviceType
//             elif serviceType = typeof<IEcsDependencyProvider> then
//                 box ecsDependencyProvider
//             else
//                 defaultServiceProvider.GetService(serviceType)
//
// // ----
//
// type EcsServiceProviderFactory() =
//     let getEcsSystems (services: IServiceCollection) =
//         services
//         |> Seq.choose (fun descriptor ->
//             let isEcsSystem = descriptor.ServiceType.IsAssignableTo(typeof<IEcsSystem>)
//             if not isEcsSystem then
//                 None
//             else
//                 Some descriptor
//         )
//
//     interface IServiceProviderFactory<IServiceCollection> with
//         member this.CreateBuilder(services) = services
//         member this.CreateServiceProvider(containerBuilder) =
//             let defaultServiceProvider = containerBuilder.BuildServiceProvider()
//             let ecsSystemDescriptors = getEcsSystems containerBuilder
//             upcast EcsServiceProvider(defaultServiceProvider, ecsSystemDescriptors)
//
// // ----
//
// [<AutoOpen>]
// module Extensions =
//
//     open Microsoft.Extensions.Hosting
//
//     type IHostBuilder with
//         member this.UseEcsServiceProvider() =
//             this.UseServiceProviderFactory(fun ctx ->
//                 EcsServiceProviderFactory() :> IServiceProviderFactory<_>
//             )
