#nowarn "9"

open System
open System.Numerics
open System.Collections.Generic
open System.Runtime.CompilerServices
open FSharp.NativeInterop
open Ehingeeinae.Ecs

[<Struct>]
type Position =
    { Position: Vector2 }

[<Struct>]
type Velocity =
    { Velocity: Vector2 }

let world = EcsWorld.createEmpty ()
let worldManager = EcsWorldManager(world)

[<EntryPoint>]
let main args =
    printfn $"World init: %A{world}"

    worldManager.AddEntity1(EcsEntityId 1UL) |> ignore

    let eid1 = worldManager.AddEntity2({ Position = Vector2(2f, 2f) }, { Velocity = Vector2(1f, 1f) })
    let eid2 = worldManager.AddEntity2({ Position = Vector2(2f, 2f) }, { Velocity = Vector2(-1f, -1f) })

    printfn $"World seed: %A{world}"

    worldManager.QueryComponent2<Position, Velocity>()
    |> Seq.map (fun (a1, a2) -> (a1.AsMemory(), a2.AsMemory()))
    |> ArraySeq.iter2 (ByRefAction<_, _> (fun position velocity ->
        let pPosition = NativePtr.ofVoidPtr<Position> (Unsafe.AsPointer(&position))
        NativePtr.set pPosition 0 { Position = Vector2.Add(position.Position, velocity.Velocity) }

//        position <- { Position = Vector2.Add(position.Position, velocity.Velocity) }
        ()
    ))

    printfn $"World result: %A{world}"
    0
