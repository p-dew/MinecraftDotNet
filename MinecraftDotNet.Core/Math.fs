namespace MinecraftDotNet.Core.Math

type Vector2<'T> =
    {
        X: 'T
        Y: 'T
    }
    static member inline (+) (v1, v2) =
        let { X = x1; Y = y1 } = v1
        let { X = x2; Y = y2 } = v2
        { X = x1 + x2; Y = y1 + y2 }

type Vector2i = Vector2<int>