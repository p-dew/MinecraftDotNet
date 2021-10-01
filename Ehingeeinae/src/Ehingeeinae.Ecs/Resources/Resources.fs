namespace Ehingeeinae.Ecs.Resources

open System
open System.Collections.Generic

type IEcsSharedResource<'T> =
    abstract Value: 'T with get

type IEcsUniqueResource<'T> =
    abstract Value: 'T with get, set

type IEcsResourceProvider =
    [<RequiresExplicitTypeArguments>]
    abstract GetShared<'T> : unit -> IEcsSharedResource<'T>
    [<RequiresExplicitTypeArguments>]
    abstract GetUnique<'T> : unit -> IEcsUniqueResource<'T>

type ResourceStorage() =
    let resources = Dictionary<Type, obj (* FSharpRef<_> *) >()

    [<RequiresExplicitTypeArguments>]
    member private this.GetResourceRef<'T>(): 'T ref =
        match resources.TryGetValue(typeof<'T>) with
        | true, r -> unbox r
        | false, _ ->
            let resourceRef = ref Unchecked.defaultof<'T>
            resources.[typeof<'T>] <- resourceRef
            resourceRef

    [<RequiresExplicitTypeArguments>]
    member this.GetShared<'T>() =
        let resourceRef = this.GetResourceRef<'T>()
        { new IEcsSharedResource<'T> with member _.Value = resourceRef.Value }

    [<RequiresExplicitTypeArguments>]
    member this.GetUnique<'T>() =
        let resourceRef = this.GetResourceRef<'T>()
        { new IEcsUniqueResource<'T> with
            member _.Value
                with get() = resourceRef.Value
                and set(x) = resourceRef.Value <- x
        }

    interface IEcsResourceProvider with
        member this.GetShared<'T>() = this.GetShared<'T>()
        member this.GetUnique<'T>() = this.GetUnique<'T>()

[<AutoOpen>]
module ResourceProviderExtensions =
    type IEcsResourceProvider with
        [<RequiresExplicitTypeArguments>]
        member this.RegisterResource<'T>(initialValue: 'T) =
            let resource = this.GetUnique<'T>()
            resource.Value <- initialValue
