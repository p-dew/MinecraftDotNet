namespace MinecraftDotNet.Core.Math

type Vector2<'T> =
    {
        X: 'T
        Y: 'T
    }
    static member inline (~-) (v) =
        let { X = x; Y = y } = v
        { X = -x; Y = -y }
    static member inline (~+) (v) =
        { X = +v.X; Y = +v.Y }
    static member inline (+) (v1, v2) =
        let { X = x1; Y = y1 } = v1
        let { X = x2; Y = y2 } = v2
        { X = x1 + x2; Y = y1 + y2 }
    static member inline (-) (v1, v2) =
        v1 + -v2

type Vector2i = Vector2<int>
type Vector2f = Vector2<single>
type Vector2d = Vector2<double>


type Vector3<'T> =
    {
        X: 'T
        Y: 'T
        Z: 'T
    }
    static member inline (~-) (v) =
        { X = -v.X; Y = -v.Y; Z = -v.Z }
    static member inline (~+) (v) =
        { X = +v.X; Y = +v.Y; Z = v.Z }
    static member inline (+) (v1, v2) =
        {
            X = v1.X + v2.X
            Y = v1.Y + v2.Y
            Z = v1.Z + v2.Z
        }
    static member inline (-) (v1, v2) =
        v1 + -v2

type Vector3i = Vector3<int>
type Vector3f = Vector3<single>
type Vector3d = Vector3<double>