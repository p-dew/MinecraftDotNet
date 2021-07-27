module MinecraftDotNet.State

open MinecraftDotNet.Core.World
open Fp

module World =
    
    type State = World
    
    type Update' =
        | AddTime of Tick
        | SetTime of Tick
        static member Unit = AddTime 0L
        static member Apply(s: State, u: Update') =
            match u with
            | AddTime t -> { s with Time = s.Time + t }
            | SetTime t -> { s with Time = t }
        static member Combine(u1, u2) =
            match u1, u2 with
            | _, SetTime t -> SetTime t
            | AddTime t1, AddTime t2 -> AddTime (t1 + t2)
            | SetTime t1, AddTime t2 -> SetTime (t1 + t2)
    
    type Update = Nopable<Update'>
    
    let update: UpdateM<State, Update, unit> = updateM {
        do! UpdateM.write (AddTime Tick.unit |> Nopable.Update)
    }


type State =
    { World: World
      Counter: int }

type Update' =
    | SetCount of int | AddCount of int
    | UpdateWorld of World.Update
    | Updates of Update' * Update'
    static member Apply(state, update) =
        match update with
        | SetCount i -> { state with Counter = i }
        | AddCount i -> { state with Counter = state.Counter + i }
        | UpdateWorld wu -> { state with World = UpdateM.apply state.World wu }
        | Updates (u1, u2) ->
            state |> (flip UpdateM.apply u1 >> flip UpdateM.apply u2)
    
    static member Combine(u1, u2) =
        match u1, u2 with
        | AddCount x1, AddCount x2 -> AddCount (x1 + x2)
        | SetCount x1, AddCount x2 -> SetCount (x1 + x2)
        | (SetCount _ | AddCount _), SetCount x -> SetCount x
        
        | UpdateWorld uw1, UpdateWorld uw2 -> UpdateM.combine uw1 uw2 |> UpdateWorld
        
        | u1, u2 -> Updates (u1, u2)

type Update = Nopable<Update'>

let update = updateM {
    do! World.update |> UpdateM.map' (fun s -> s.World) (UpdateWorld >> Nopable.Update)
    
    let! state = UpdateM.read ()
    
    if state.World.Time % 20L = 0L
    then do!  AddCount 1 |> Nopable.Update |> UpdateM.write
}

module pg =
    
    let update () = update {
        let! state = Mc.getState ()
        let newState = { state with Count = state.Count + 1 }
        do! Mc.setState newState
    }
