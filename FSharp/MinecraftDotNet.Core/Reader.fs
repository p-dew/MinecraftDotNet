module MinecraftDotNet.Core.Reader

type Reader<'env, 'a> = Reader of ('env -> 'a)

module Reader =
    let run (env: 'env) (Reader f) = f env
    let value x : Reader<'env, 'a> = Reader (fun _ -> x)
    
    let bind (f: 'a -> Reader<'env, 'b>) r =
        Reader (fun env ->
            let x = run env r
            run env (f x)
        )
    
    let map f : Reader<'env, 'a> -> Reader<'env, 'b> =
        f >> value |> bind
    
    let apply f x : Reader<'env, 'b> = 
        let newAction env =
            let f' = run env f 
            let x' = run env x 
            f' x'
        Reader newAction


type ReaderBuilder() =
    member _.Bind(x, f) = Reader.bind f x
    member _.Return(x) = Reader.value x
    member _.ReturnFrom(r) : Reader<'env, 'a> = r

let reader = ReaderBuilder()