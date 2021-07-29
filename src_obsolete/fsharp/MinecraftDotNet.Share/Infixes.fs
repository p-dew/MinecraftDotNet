[<AutoOpen>]
module Infixes

let inline (^) f x = f x

let inline flip f x y = f y x
