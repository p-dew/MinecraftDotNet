module ExprToCode

open System
open System.Text

open FSharp.Quotations


module rec ExprDisplay =


    let display (expr: Expr) : string =

        // let rec codeExpr expr =
        //     match expr with
        //     | DerivedPatterns.Applications (func, arguments) ->
        //         SourceCode.ofLines [
        //             SourceCodeLine.ofTokens [
        //                 "fun"
        //                 for args in arguments do
        //                     " "
        //
        //             ]
        //         ]

        let sb = StringBuilder()

        let rec appendExpr tabN expr =
            let tabStr = String.replicate tabN "    "
            let append (s: string) = sb.Append(s) |> ignore
            let appendLine (s: string) = sb.AppendLine(s).Append(tabStr) |> ignore
            let appendExprs sep exprs =
                match exprs with
                | arg :: _ -> appendExpr (tabN+1) arg | _ -> ()
                match exprs with
                | _ :: args ->
                    for arg in args do
                        append sep
                        appendExpr (tabN+1) arg
                | _ -> ()
            let appendExprT1 expr = appendExpr (tabN+1) expr

            match expr with
            | DerivedPatterns.Applications (func, arguments) ->
                appendExprT1 func
                for args in arguments do
                    append " "
                    match args with
                    | [arg] -> appendExprT1 arg
                    | args ->
                        append "("
                        appendExprs ", " args
                        append ")"

            | DerivedPatterns.Lambdas (arguments, body) ->
                append "fun"
                for args in arguments do
                    append " "
                    match args with
                    | [arg] -> append $"{arg.Name}"
                    | args ->
                        append "("
                        match args with
                        | arg :: _ -> append $"{arg.Name}" | _ -> ()
                        match args with
                        | _ :: args ->
                            for arg in args do
                                append ", "
                                append $"{arg.Name}"
                        | _ -> ()
                        append ")"
                appendLine " ->"
                append "    "
                appendExpr (tabN + 1) body

            | Patterns.TupleGet (tuple, idx) ->
                appendExpr (tabN+1) tuple
                append $".Item{idx}"

            | Patterns.Let (var, assigmentExpr, bodyExpr) ->
                append "let "
                if var.IsMutable then append "mutable "
                appendLine $"{var.Name} = "
                append "    "
                appendExpr (tabN + 1) assigmentExpr
                appendLine ""
                appendExpr tabN bodyExpr

            | Patterns.Value (value, typ) ->
                append $"%A{value}"

            | Patterns.Lambda (parameter, body) ->
                appendLine $"fun ({parameter.Name}: {parameter.Type}) ->"
                appendExpr (tabN + 1) body

            | Patterns.Var var ->
                append $"{var.Name}"

            | Patterns.Call (this, method, args) ->
                this |> Option.iter (fun this ->
                    appendExpr (tabN+1) this
                    append "."
                )
                append $"{method.Name}"
                append "("
                appendExprs ", " args
                append ")"

            | Patterns.NewObject (ctor, args) ->
                append $"{ctor.DeclaringType.Name}{ctor.Name}"
                append "("
                appendExprs ", " args
                append ")"

            | Patterns.PropertyGet (this, propertyInfo, args) ->
                this |> Option.iter (fun this ->
                    appendExpr (tabN+1) this
                    append "."
                )
                append $"{propertyInfo.Name}"
                if args.Length > 0 then
                    append ".["
                    appendExprs ", " args
                    append "]"

            | Patterns.Sequential (first, second) ->
                appendExpr tabN first
                appendLine ""
                appendExpr tabN second

            | Patterns.IfThenElse (guard, thenExpr, elseExpr) ->
                append "if "
                appendExpr (tabN+1) guard
                appendLine " then"
                append "    "
                appendExpr (tabN+1) thenExpr
                appendLine ""
                appendLine "else"
                append "    "
                appendExpr (tabN+1) elseExpr

            | Patterns.Coerce (source, target) ->
                appendExpr (tabN+1) source
                append $" :> {target.Name}"

            | Patterns.TypeTest (source, target) ->
                appendExpr (tabN+1) source
                append $" :? {target.Name}"

            | Patterns.WhileLoop (guard, body) ->
                append "while "
                appendExprT1 guard
                appendLine " do"
                append "    "
                appendExprT1 body

            | Patterns.TryFinally (body, compensation) ->
                appendLine "try"
                append "    "
                appendExprT1 body
                appendLine ""
                appendLine "finally"
                append "    "
                appendExprT1 compensation

            | Patterns.NewTuple elements ->
                append "("
                appendExprs ", " elements
                append ")"

            | Patterns.NewStructTuple elements ->
                append "struct("
                appendExprs ", " elements
                append ")"

            | Patterns.DefaultValue expressionType ->
                append $"defaultof<{expressionType.Name}>"

            | expr ->
                raise <| NotSupportedException($"This expression is not supported: %A{expr}")

        appendExpr 0 expr
        sb.ToString()
