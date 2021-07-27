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
open System.Threading

open Hopac
open Hopac.Infixes

open MinecraftDotNet.Core.World
open MinecraftDotNet.Fp
open MinecraftDotNet
//open MinecraftDotNet.State


let render (state: State.State) frameTime =
    printfn "[%.2f] %A ; %A" frameTime state.World.Time state.Counter


let [<Literal>] Fps = 60
let [<Literal>] Tps = 20

let updating (initState: State.State) stateRef =
    
    let sw = Stopwatch()
    sw.Start()
    
    let rec loop state =
        let sleepT = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / int64 Tps) - sw.Elapsed |> max TimeSpan.Zero
        Thread.Sleep(sleepT)
        sw.Restart()
        let (u, ()) = UpdateM.run State.update state
        let state = UpdateM.apply state u
        lock stateRef.contents (fun() ->
            stateRef := state
        )
        
        loop state
    
    loop initState

let rendering (initState: State.State) stateRef =
    let sw = Stopwatch()
    sw.Start()
    
    let rec loop () =
        let sleepT = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / int64 Fps) - sw.Elapsed |> max TimeSpan.Zero
        Thread.Sleep(sleepT)
        let frameTime = sw.Elapsed.TotalMilliseconds
        lock stateRef.contents (fun() ->
            render !stateRef frameTime
        )
        sw.Restart()
        loop ()
    
    loop ()


let runMinecraft updating rendering initState =
    let stateRef = ref initState
    
    let logicThread = Thread(fun() ->
        updating initState stateRef
    )
    
    let renderThread = Thread(fun() ->
        rendering initState stateRef
    )
    
    logicThread.Start()
    renderThread.Start()
    
    logicThread.Join()
    renderThread.Join()
    
    ()

[<EntryPoint>]
let main argv =
    
    let dim0 = Dimension.create "DIM0"
    
    let world =
        { Name = "New World"
          Time = 0L
          Dimensions = [| dim0 |] }
    
    let initState =
        { State.World = world
          State.Counter = 0 }
    
    runMinecraft updating rendering initState
    
    0
