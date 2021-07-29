[<AutoOpen>]
module MinecraftDotNet.Core.Math.Linear.Aliases

//
// Vector3
//

type Vector3i = Vector3<int>

type Vector3f = Vector3<float32>
module Vector3f =
    let toSystem (v: Vector3f) =
        Vector3.toTuple v |> System.Numerics.Vector3
    let ofSystem (sysv: System.Numerics.Vector3) =
        Vector3.create sysv.X sysv.Y sysv.Z

type Vector3d = Vector3<double>

type vec3<'a> = Vector3<'a>
let vec3 = Vector3.create

//
// Vector2
//

type Vector2i = Vector2<int>

type Vector2f = Vector2<single>
module Vector2f =
    let inline toSystem v =
        System.Numerics.Vector2(v.X, v.Y)
    let inline ofSystem (sysv: System.Numerics.Vector2) =
        Vector2.create sysv.X sysv.Y

type Vector2d = Vector2<double>

type vec2<'a> = Vector2<'a>
let vec2 = Vector2.create

//
// Matrix4x4
//

type Matrix4x4f = Matrix4x4<single>
module Matrix4x4f =
    let ofSystem (sysm: System.Numerics.Matrix4x4) =
        Matrix4x4.create
            sysm.M11 sysm.M12 sysm.M13 sysm.M14
            sysm.M21 sysm.M22 sysm.M23 sysm.M24
            sysm.M31 sysm.M32 sysm.M33 sysm.M34
            sysm.M41 sysm.M42 sysm.M43 sysm.M44

    let toSystem m =
        m |> Matrix4x4.toTuple |> System.Numerics.Matrix4x4

type Matrix4x4d = Matrix4x4<double>
