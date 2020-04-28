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
open MinecraftDotNet.ClientSide.Graphics.OpenGl

type Dimension =
    { Name: string
       }

type World =
    { Name: string
       }

let hardAsyncLogic () =
    let n = Random().Next(10, 60)
    Async.Sleep(n)

let D = 50L

type Msg =
    | Msg

[<EntryPoint>]
let main argv =
    async {
        
        let logicAgent =
            MailboxProcessor<Msg>.Start(fun inbox ->
                let rec loop t =
                    async {
                        
                        let sw = Stopwatch.StartNew()
                        printfn "Hard work..."
                        do! hardAsyncLogic()
                        printfn "Finished hard work (%i)" sw.ElapsedMilliseconds

                        let processMsg msg = async { printfn "Got and proceed message %A" msg }

                        let rec receiveLoop () = async {
                            let d = sw.ElapsedMilliseconds
                            let leftT = max 0L (D - d)
                            let! msgOpt = inbox.TryReceive(int leftT)
                            match msgOpt with
                            | None -> return ()
                            | Some msg ->
                                do! processMsg msg
                                return! receiveLoop ()
                        }

                        do! receiveLoop ()

                        printfn "Finished! (%i)\n" sw.ElapsedMilliseconds
                        return! loop 0
                    }
                loop 0
            )
        
        return ()
    }
    |> Async.RunSynchronously
    0
