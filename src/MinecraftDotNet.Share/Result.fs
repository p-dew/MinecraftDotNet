module Result

type X = int

let tryWithExn f =
    try
        let x = f ()
        Ok x
    with e -> Error e