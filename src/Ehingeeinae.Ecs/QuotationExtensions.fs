module Ehingeeinae.Ecs.QuotationExtensions

open FSharp.Quotations


[<RequireQualifiedAccess>]
module Expr =

    [<RequiresExplicitTypeArguments>]
    let varTyped<'T> (variable: Var) =
        assert(variable.Type = typeof<'T>)
        Expr.Var(variable) |> Expr.Cast<'T>

    let lambdaMany (arguments: Var list) (body: Expr list -> Expr) : Expr =
        let arguments' = arguments |> Seq.map Expr.Var |> Seq.toList
        let rec loop parameters =
            match parameters with
            | [] -> invalidOp ""
            | [parameter] -> Expr.Lambda(parameter, body arguments')
            | parameter :: tail -> Expr.Lambda(parameter, loop tail)
        loop arguments

    [<RequiresExplicitTypeArguments>]
    let lambda1<'T, 'R> (paramName: string) (body: Expr<'T> -> Expr<'R>) : Expr<'T -> 'R> =
        let paramVar = Var(paramName, typeof<'T>)
        Expr.Lambda(paramVar,
            let paramVarExpr = varTyped<'T> paramVar
            body paramVarExpr
        )
        |> Expr.Cast

    [<RequiresExplicitTypeArguments>]
    let lambda2<'T1, 'T2, 'R> (param1Name: string) (param2Name: string) (body: Expr<'T1> -> Expr<'T2> -> Expr<'R>) : Expr<'T1 -> 'T2 -> 'R> =
        let param1Var = Var(param1Name, typeof<'T1>)
        let param2Var = Var(param2Name, typeof<'T2>)
        Expr.Lambda(param1Var,
            Expr.Lambda(param2Var,
                let param1VarExpr = varTyped<'T1> param1Var
                let param2VarExpr = varTyped<'T2> param2Var
                body param1VarExpr param2VarExpr
            )
        )
        |> Expr.Cast

    let letMany (vars: (Var * Expr) list) (body: Expr list -> Expr) : Expr =
        let vars' = vars |> Seq.map (fst >> Expr.Var) |> Seq.toList
        let rec foo vars =
            match vars with
            | [] -> invalidOp ""
            | [(var, value)] -> Expr.Let(var, value, body vars')
            | (var, value) :: tail ->
                Expr.Let(var, value, foo tail)
        foo vars

    let sequentialMany (exprs: Expr list) =
        let rec loop exprs =
            match exprs with
            | [] -> invalidOp ""
            | [expr] -> expr
            | expr :: tail -> Expr.Sequential(expr, loop tail)
        loop exprs
