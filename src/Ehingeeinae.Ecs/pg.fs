module private Ehingeeinae.Ecs.pg

[<Struct>]
type EcsComponent<'comp when 'comp : unmanaged> =
    internal
        { Pointer: nativeptr<'comp> }


module rec a =

    open System.Numerics

    type IComponentGetter = abstract Get<'c when 'c : unmanaged> : unit -> EcsComponent<'c>

    type QAnd<'q1, 'q2, 'a> = abstract Map: 'q1 * 'q2 -> 'a
    type QOr<'q1, 'q2, 'a> = abstract Map: Choice<'q1, 'q2> -> 'a
    type QNot<'c> = struct end
    type QComp<'c> = struct end

    module EcsQuery =
        let qComp<'c> = QComp<'c>()
        let qAnd f = { new QAnd<'q1, 'q2, 'a> with member _.Map(q1, q2) = f (q1, q2) }
        let qOr f = { new QOr<'q1, 'q2, 'a> with member _.Map(e) = f e }


//        let notComp<'c when 'c : unmanaged> () : EcsQuery<unit> =
//            failwith ""


    [<Struct>]
    type Position = { Position: Vector2 }

    [<Struct>]
    type Velocity = { Velocity: Vector2 }

    [<Struct>]
    type Static = struct end

    let foo () =
        let q =
//            EcsQuery.qAnd ()
            ()
        ()





//module pg =
//
//    open System.Numerics
//
//    type Position = { X: int; Y: int }
//    type Color = { Color: string }
//    type Velocity = { Velocity: Vector3 }
//
//    [<Struct>]
//    type Transform =
//        { Position: Vector3
//          Rotation: Vector3
//          Scale: Vector3 }
//
//    let foo () =
//        let q = ecsQuery {
//            let! position = EcsQuery.queryComponent<Position>
//            and! color = EcsQuery.queryComponent<Color>
//            and! velocity = EcsQuery.queryComponent<Velocity>
//            and! transform = EcsQuery.queryComponent<Transform>
//            return transform, position, color, velocity
//        }
////        ecsSystemUpdate {
////            let! transform, velocity = q
////            return [
////                { transform with Position = transform.Position + velocity.Velocity * dt }
////            ]
////        }
//        ()
//
////    module UnloadMarkerSystem =
////        let update =
////            let distance = 10f
////            ecsSystemUpdate {
////                let! playerPositions = ecsQuery {
////                    let! position = EcsQuery.queryComponent<Position>
////                    and! _ = EcsQuery.queryComponent<Player>
////                    return position
////                }
////                let! nonPlayerPositions = ecsQuery {
////                    let! position = EcsQuery.queryComponent<Position>
////                    and! _ = EcsQuery.queryComponent<Player> |> EcsQuery.not
////                    return position
////                }
////                for nonPlayerPosition in nonPlayerPositions do
////                    let isEnoughFar = playerPositions |> Seq.forall (fun pPos -> pPos.DistanceTo(nonPlayerPosition) > distance)
////
////                    ()
////            }
//
////    type MySystem() =
////        interface IEcsSystem with
////            member this.Update(world) =
////                let q = ecsQuery {
////                    let! position = EcsQuery.queryComponent<Position>
////                    and! color = EcsQuery.queryComponent<Color>
////                    and! velocity = EcsQuery.queryComponent<Velocity>
////                    and! transform = EcsQuery.queryComponent<Transform>
////                    return transform, position, color, velocity
////                }
////                let entities = q world
////                for entity in entities do
////                    let eid = entity.Id
////                    let transform, position, color, velocity = entity.Component
////                    transform.Position <- Vector3(0f, 0f, 0f)
////                    ()
//
////    let update =
////        ecsUpdate {
////            let q = ecsQuery { ... }
////            for e in q do
////
////        }
