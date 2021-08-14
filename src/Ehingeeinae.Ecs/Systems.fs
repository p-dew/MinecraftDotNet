namespace Ehingeeinae.Ecs.Systems

//type IEcsSystem =
//    abstract Update: world: EcsWorld -> unit
//
//
////type EcsSystemBuilder() =
//
//
//type EcsSystemUpdateEffect =
//    | UpdateComponent of (obj -> obj)
////    | Task of 'arg * ('result -> 'a)
//
//type EcsSystemUpdate = EcsWorld -> EcsSystemUpdateEffect seq
//
//module EcsSystemUpdate =
//    let iterateQuery (q: EcsQuery<'a>) : EcsWorld -> EcsEntity<'a> seq =
//        fun world -> q world
//
//    let mergeSeq (us: EcsSystemUpdate seq) : EcsSystemUpdate =
//        fun world -> us |> Seq.map (fun x -> x world) |> Seq.collect id
//
//    let bindQuery (binding: EcsEntity<'a> -> EcsSystemUpdate) (q: EcsQuery<'a>) : EcsSystemUpdate =
//        fun world ->
//            let es = q world
//            let upds = es |> Seq.map binding
//            mergeSeq upds world
//
//
//type EcsSystemUpdateBuilder() =
//    member _.Bind(q: EcsQuery<'a>, f: EcsEntity<'a> -> EcsSystemUpdate): EcsSystemUpdate = EcsSystemUpdate.bindQuery f q
//    member _.Yield(x): EcsSystemUpdate =
//        fun world -> seq [EcsSystemUpdateEffect.UpdateComponent x]
//
//[<AutoOpen>]
//module EcsSystemUpdateBuilderImpl =
//    let ecsSystemUpdate = EcsSystemUpdateBuilder()
//
