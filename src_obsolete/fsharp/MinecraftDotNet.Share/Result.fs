module Result

let tryWithExn f =
    try f () |> Ok
    with e -> Error e