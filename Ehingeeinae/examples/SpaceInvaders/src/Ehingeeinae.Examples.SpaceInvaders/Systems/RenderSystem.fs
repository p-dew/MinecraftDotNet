namespace Ehingeeinae.Examples.SpaceInvaders.Systems

open System.Text
open Ehingeeinae.Ecs.Querying
open Ehingeeinae.Ecs.Systems
open Ehingeeinae.Examples.SpaceInvaders.Components

type RenderSystem
    (
        q: IEcsQuery<struct(Position cread * Text cread)>,
        queryExecutor: EcsWorldQueryExecutor
    ) =
    let screenWidth = 16
    let screenHeight = 8
    let screen = Array2D.create screenWidth screenHeight ' '

    let screenToString (screen: char[,]) : string =
        let sb = StringBuilder()
        screen |> Array2D.iteri (fun x y c ->
            sb.Append(c) |> ignore
            if x = screenWidth - 1 then
                sb.AppendLine() |> ignore
        )
        sb.ToString()

    interface IEcsSystem with
        member this.Update(ctx) =
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
