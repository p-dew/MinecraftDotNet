namespace Ehingeeinae.Ecs.Scheduling

open Ehingeeinae.Ecs

//type EcsScheduler(world: EcsWorld, systems: IEcsSystem seq) =
//    let sequentialUpdate (systems: IEcsSystem seq) (world: EcsWorld) =
//        for system in systems do
//            system.Update(world)
//
//    member this.AsyncRun() = async {
//        while true do
//            sequentialUpdate systems world
//    }
