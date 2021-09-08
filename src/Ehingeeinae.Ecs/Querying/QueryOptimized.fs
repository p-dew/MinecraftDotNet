namespace Ehingeeinae.Ecs.Querying

open System
open System.Runtime.CompilerServices

open Ehingeeinae.Ecs
open Ehingeeinae.Ecs.Worlds


//module Optimized =
//
//    [<RequiresExplicitTypeArguments>]
//    let private getStorageArray<'c> (storage: ArchetypeStorage) =
//        storage.GetColumn<'c>() |> ResizeArray.getItems
//
//    let private castComp (arr: ArraySegment<_>) (i: int) =
//        let c = &arr.Array.[i]
//        let p = Unsafe.AsPointer(&c)
//        { Pointer = p }
//
//    [<RequireQualifiedAccess>]
//    module EcsQuery =
//
//        [<RequiresExplicitTypeArguments>]
//        let query1<'c1> =
//            { new IEcsQuery<EcsComponent<'c1>> with
//                member _.Filter(archetype) = archetype.ComponentTypes.IsSupersetOf([typeof<'c1>])
//                member _.Fetch(storage) = seq {
//                    let arr1 = getStorageArray<'c1> storage
//                    for i in 0 .. arr1.Count - 1 do
//                        yield castComp arr1 i
//                }
//            }
//
//        [<RequiresExplicitTypeArguments>]
//        let query2<'c1, 'c2> =
//            { new IEcsQuery<struct (EcsComponent<'c1> * EcsComponent<'c2>)> with
//                member _.Filter(archetype) = archetype.ComponentTypes.IsSupersetOf([typeof<'c1>; typeof<'c2>])
//                member _.Fetch(storage) = seq {
//                    let arr1 = getStorageArray<'c1> storage
//                    let arr2 = getStorageArray<'c2> storage
//                    assert (arr1.Count = arr2.Count)
//                    for i in 0 .. arr1.Count - 1 do
//                        yield castComp arr1 i, castComp arr2 i
//                }
//            }
//
//        [<RequiresExplicitTypeArguments>]
//        let query3<'c1, 'c2, 'c3> =
//            { new IEcsQuery<struct (EcsComponent<'c1> * EcsComponent<'c2> * EcsComponent<'c3>)> with
//                member _.Filter(archetype) = archetype.ComponentTypes.IsSupersetOf([typeof<'c1>; typeof<'c2>; typeof<'c3>])
//                member _.Fetch(storage) = seq {
//                    let arr1 = getStorageArray<'c1> storage
//                    let arr2 = getStorageArray<'c2> storage
//                    let arr3 = getStorageArray<'c3> storage
//                    assert (arr1.Count = arr2.Count && arr2.Count = arr3.Count)
//                    for i in 0 .. arr1.Count - 1 do
//                        yield castComp arr1 i, castComp arr2 i, castComp arr3 i
//                }
//            }
