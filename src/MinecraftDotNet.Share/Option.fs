module Option

// Erase Error data
let ofResult = function
    | Ok x -> Some x
    | Error _ -> None

let ofOut = function
    | true, x -> Some x
    | false, _ -> None