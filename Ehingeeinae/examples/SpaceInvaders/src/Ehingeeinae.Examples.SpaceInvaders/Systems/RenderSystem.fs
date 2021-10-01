namespace Ehingeeinae.Examples.SpaceInvaders.Systems

open System.Text
open Ehingeeinae.Ecs.Querying
open Ehingeeinae.Ecs.Systems
open Ehingeeinae.Examples.SpaceInvaders.Components

type RenderSystem
    (
        q: IEcsQuery<(Position cread * Text cread)>,
        queryExecutor: EcsWorldQueryExecutor
    ) =
    let screenWidth = 16
    let screenHeight = 8
    let screen = Array2D.create screenWidth screenHeight ' '

    let clearScreen (screen: char[,]) : unit =
        screen |> Array2D.iteri (fun x y _ -> screen.[x, y] <- ' ')

    let screenToString (screen: char[,]) : string =
        let sb = StringBuilder()
        for y in 0 .. Array2D.length2 screen - 1 do
            for x in 0 .. Array2D.length1 screen - 1 do
                let c = screen.[x, y]
                sb.Append(c) |> ignore
            sb.AppendLine() |> ignore
        sb.ToString()

    interface IEcsSystem with
        member this.Update(ctx) =
            clearScreen screen

            let r = queryExecutor.ExecuteQuery(q)
            for positionComp, textComp in r do
                let position = positionComp.Value
                let text = textComp.Value
                let x = int (round position.X)
                let y = int (round position.Y)
                if x >= 0 && x < screenWidth && y >= 0 && y < screenHeight then
                    screen.[x, y] <- text.Text.[0]

            let screenStr = screenToString screen
            printfn $"<<<<\n%s{screenStr}\n>>>>"
