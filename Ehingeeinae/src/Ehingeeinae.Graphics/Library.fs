namespace Ehingeeinae.Graphics

open System
open System.Collections.Concurrent
open System.Threading

type RenderAction = delegate of unit -> unit

type Foo() =

    let queue = ConcurrentQueue<RenderAction>()

    member this.SendAction(action: RenderAction): unit =
        queue.Enqueue(action)

    member this.Run(?ct: CancellationToken) =
        let ct = CancellationToken.None |> defaultArg ct

        while not ct.IsCancellationRequested do
            let actions = queue.ToArray()
            queue.Clear()

            for action in actions do
                action.Invoke()
