module private Ehingeeinae.Ecs.pg

open Ehingeeinae.Ecs.Worlds
open Ehingeeinae.Ecs.Querying


type Query<'a, 'q> =
    { Run: 'q -> 'a }

type QueryFilter = EcsArchetype -> bool

type QueryBuilder() =
    member _.Bind(x: Query<'a, 'view>, f: 'a -> Query<'b, 'view>): Query<'b, 'view> =
        { Run = fun view ->
            let a = x.Run view
            let qb = f a
            qb.Run view
        }
    member _.Return(x) = { Run = fun _ -> x }
    member _.BindReturn(x: Query<'a, 'view>, f: 'a -> 'b): Query<'b, 'view> =
        { Run = fun view -> f (x.Run view) }
    member _.MergeSources(q1: Query<'a, 'aview>, q2: Query<'b, 'bview>): Query<'a * 'b, 'aview * 'bview> =
        let map (aview, bview) : ('a * 'b) =
            q1.Run aview, q2.Run bview
        { Run = map }


module rec a =

    open System.Numerics

    let ecsQuery = QueryBuilder()


    (*
    query<Position, Velocity>
    let x: Query< Component<Position> * Component<Velocity> , (Position * (Velocity))> =
        query {
            let! pos: Component<Position> = q<Position>
            and! vel: Component<Velocity> = q<Velocity>

            return pos, vel
        }

    *)

    [<RequiresExplicitTypeArguments>]
    let comp<'c> : Query<EcsComponent<'c>, EcsComponent<'c>> =
        { Run = id }

    let retn x = { Run = fun _ -> x }
    let getQ () = { Run = fun q -> q }

//    let notComp<'c> : Query<unit, NotComp<'c>>

    let foo () =
        let q =
            query {
                for i in 1..5 do
                where (i < 3)
                select (i + 1)
            }
        let q =
            ecsQuery {
                let! pos = comp<Position>
                and! vel = comp<Velocity>
                and! hp = comp<int>
                return pos, vel
            }

        let qt = q.GetType()
        let viewType = qt.GenericTypeArguments.[1]

        ()



    [<Struct>]
    type Position = { Position: Vector2 }

    [<Struct>]
    type Velocity = { Velocity: Vector2 }

    [<Struct>]
    type Static = struct end


module c =



    type EcsQueryBuilder() =

        member _.Run<'c1 when 'c1 : struct>(q: 'c1) = q
        member _.Run<'c1, 'c2 when 'c2 : struct>(q: 'c1 * 'c2) = q
        member _.Run<'c1, 'c2, 'c3 when 'c3 : struct>((c1, (c2, c3))): 'c1 * 'c2 * 'c3 = (c1, c2, c3)
        member _.Run<'c1, 'c2, 'c3, 'c4 when 'c4 : struct>((c1, (c2, (c3, c4)))): 'c1 * 'c2 * 'c3 * 'c4 = (c1, c2, c3, c4)

    let foo () =
        EcsQueryBuilder().Run((1, (2, 3))) |> ignore
        EcsQueryBuilder().Run((1, 2)) |> ignore

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
