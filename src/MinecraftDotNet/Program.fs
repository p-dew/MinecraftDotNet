// ---------------------
// | В память о всей ебле с кодом
// ---------
//                     _,.---'""'"--.._
//                   ,"                `-.
//                 ,'                     `.
//                /     _       ,.          `.
//               /     ||      |"|            \
//              /      ||      | |             \
//             /       .'      `_'              L
//            j                                 |
//            |        __,...._                 |
//            |      ."        `.               |
//            |      '           )              |
//            |       `-...__,.-'               |
//            |                                 |
//            |                                 |
//         ...|                                 |
//      _,'   |                                 |
//  _,-'  ___ |                                 |.-----_
//-' ,.--`.  \|                                 |     . \
//,-'     ,  |--,                               |  _,'   `- -----._
//      ,' ,'    - ----.            _,..       _|.',               \
// ,-""' .-             \  ____   `'  _-'`  ,-'     `.              `-
// .--'"`   ,--:`.       --    ,"'. ,'  ,'`,_
//        _'__,' |  _,..'_    ,:______,-     --.         _.
//        -__..--' '      ` ..`L________,___ _,     _,.-'
//                                              '" ' mh

open System
open System.Diagnostics
open System.Text.Json
open MinecraftDotNet.Core.Math

type Tick = int64

type Dimension =
    { Name: string }

type World =
    { Name: string
      Time: Tick }

module World =
    let incrTime world = { world with Time = world.Time + 1L }

type LogicAgentState =
    { World: World }

type LogicMsg =
    | Tick of World

let holdMsg (inbox: MailboxProcessor<'msg>) updater (init: 'state) =
    let Tick = 50L * TimeSpan.TicksPerMillisecond
    let rec loop (t: int64) state = async {
        let sw = Stopwatch()
        sw.Start()
        match! inbox.TryReceive(t / TimeSpan.TicksPerMillisecond |> int) with
        | None -> return state
        | Some msg ->
            let! state' = updater msg state
            sw.Stop()
            return! loop (max 0L <| t - sw.ElapsedTicks) state'
    }
    loop Tick init


//module Mailbox =
//    /// Remove all exclude last messages from agent's queue
//    let last predicate (inbox: MailboxProcessor<_>) =
//        let length = inbox.CurrentQueueLength
//        async {
//            
//            inbox.Scan(fun msg -> async {
//                if predicate msg
//                then Some msg
//                else None
//            }
//            )
//        }

[<EntryPoint>]
let main argv =
    
    async {
        
        let world: World = { Name = "MyWorld"; Time = 0L }
        
        let dependedAgent =
            MailboxProcessor.Start(fun inbox ->
                let rec loop () = async {
//                    do! seekMessages inbox
                    match! inbox.Receive() with
                    | Tick state ->
                        do! Async.Sleep 1000
                        printfn "(%i)\n%A" inbox.CurrentQueueLength state
                        return! loop ()
                }
                loop ()
            )
        
        let logicAgent =
            let update state = async {
                do! Async.Sleep 50
                return state
            }
            MailboxProcessor<LogicMsg>.Start(fun inbox ->
                let rec loop state = async {
                    let! state' = update state
                    dependedAgent.Post (Tick state')
                    return! loop (World.incrTime state')
                }
                loop world
            )
        
        do! (System.Threading.Tasks.Task.Delay(-1) |> Async.AwaitTask)
    }
    |> Async.RunSynchronously
    0
