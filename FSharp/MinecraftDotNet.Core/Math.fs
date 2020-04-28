module MinecraftDotNet.Core.Math

open System.Runtime.CompilerServices
open System.Runtime.InteropServices


module NumericLiteralG =
    let inline FromZero() = LanguagePrimitives.GenericZero
    let inline FromOne()  = LanguagePrimitives.GenericOne

// Vector math

module Vector =
    let inline add map2 v1 v2 = map2 ( + ) v1 v2
    let inline negate map v = map ( ~- ) v
    let inline subtract map2 v1 v2 = map2 ( - ) v1 v2
    let inline scale map v a = map (( * ) a) v
    let inline sum reduce v = reduce ( + ) v
    let inline length map reduce v = sqrt (map (fun x -> x * x) v |> sum reduce)
    let inline normalize map reduce v = map (( / ) (length map reduce v)) v
    let inline dot map2 reduce v1 v2 = map2 ( * ) v1 v2 |> sum reduce

// --------------------------------
// Vector 3
// --------------------------------

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type Vector3<'T> =
    { X: 'T
      Y: 'T
      Z: 'T }

module Vector3 =
    
    let inline create x y z =
        { X = x; Y = y; Z = z }
    
    let inline toTuple v = (v.X, v.Y, v.Z)
    let inline ofTuple (x, y, z) = create x y z
    
    let inline map mapping v =
        create (mapping v.X)
               (mapping v.Y)
               (mapping v.Z)
    
    let inline map2 mapping v1 v2 =
        create (mapping v1.X v2.X)
               (mapping v1.Y v2.Y)
               (mapping v1.Z v2.Z)
    
    let inline apply applier v =
        create (applier.X v.X)
               (applier.Y v.Y)
               (applier.Z v.Z)
    
    let inline fold folder (state: 'State) (v: Vector3<'T>) : 'State =
        let flip f x y = f y x
        state
        |> flip folder v.X
        |> flip folder v.Y
        |> flip folder v.Z
    
    let inline reduce reducer (v: Vector3<'T>) : 'T =
        reducer (reducer v.X v.Y) v.Z
    
    // Math
    
    let inline zero< ^T when ^T: (static member Zero: ^T)> : Vector3< ^T> =
        create 0G 0G 0G
    let inline one<  ^T when ^T: (static member One:  ^T)> : Vector3< ^T> =
        create 1G 1G 1G
    
    let inline up<      ^T when ^T: (static member Zero: ^T) and ^T: (static member One: ^T)> : Vector3< ^T> =
        { zero with X = 1G }
    let inline down<    ^T when ^T: (static member Zero: ^T) and ^T: (static member One: ^T) and ^T: (static member (~-): ^T -> ^T)> : Vector3< ^T> =
        { zero with X = - 1G }
    let inline right<   ^T when ^T: (static member Zero: ^T) and ^T: (static member One: ^T)> : Vector3< ^T> =
        { zero with Y = 1G }
    let inline left<    ^T when ^T: (static member Zero: ^T) and ^T: (static member One: ^T) and ^T: (static member (~-): ^T -> ^T)> : Vector3< ^T> =
        { zero with Y = - 1G }
    let inline forward< ^T when ^T: (static member Zero: ^T) and ^T: (static member One: ^T)> : Vector3< ^T> =
        { zero with Z = 1G }
    let inline backward<    ^T when ^T: (static member Zero: ^T) and ^T: (static member One: ^T) and ^T: (static member (~-): ^T -> ^T)> : Vector3< ^T> =
        { zero with Z = - 1G }
    
    let inline add (v1: Vector3<'a>) (v2: Vector3<'b>) : Vector3<'c> = Vector.add map2 v1 v2
    let inline length v = Vector.length map reduce v
    let inline negate v = Vector.negate map v
    let inline subtract v1 v2 = Vector.subtract map2 v1 v2
    let inline scale v x = Vector.scale map v x
    let inline cross (v1: Vector3<_>) (v2: Vector3<_>) =
        create (v1.Y * v2.Z - v1.Z * v2.Y)
               (v1.Z * v2.X - v1.X * v2.Z)
               (v1.X * v2.Y - v1.Y * v2.X)
    let inline dot v1 v2 = Vector.dot map2 reduce v1 v2
    let inline normalize v = Vector.normalize map reduce v

type Vector3<'a> with
    static member inline ( + ) (v1: Vector3<'a>, v2: Vector3<'b>) : Vector3<'c> = Vector3.add v1 v2
    static member inline (~- ) (v) = Vector3.negate v
    static member inline ( - ) (v1, v2) = Vector3.subtract v1 v2
    static member inline ( * ) (v1, v2) = Vector3.cross v1 v2
    static member inline ( * ) (v, a) = Vector3.scale v a
    static member inline ( * ) (a, v) = Vector3.scale v a

module T =
    (Vector3.create 1 2 3) + (Vector3.create 4 5 6)
    |> ignore

// Aliases

type Vector3i = Vector3<int>

type Vector3f = Vector3<float32>
module Vector3f =
    let toSystem (v: Vector3f) =
        Vector3.toTuple v |> System.Numerics.Vector3
    let ofSystem (sysv: System.Numerics.Vector3) =
        Vector3.create sysv.X sysv.Y sysv.Z

type Vector3d = Vector3<double>


// --------------------------------
// Vector 2
// --------------------------------

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type Vector2<'T> =
    { X: 'T
      Y: 'T }

module Vector2 =
    
    let inline create x y = { X = x; Y = y }
    
    let inline ofTuple (x, y) = create x y
    let inline toTuple vec2 = (vec2.X, vec2.Y)
    
    let inline iter action (v: Vector2<'T>) =
        action v.X; action v.Y
    
    let inline map mapping (v: Vector2<'T>) : Vector2<'R> =
        create (mapping v.X) (mapping v.Y)
    
    let inline map2 mapping v1 v2 =
        create (mapping v1.X v2.X) (mapping v1.Y v2.Y)
    
    let inline apply applier (v: Vector2<'T>) : Vector2<'R> =
        create (applier.X v.X) (applier.Y v.Y)
    
    let inline fold folder (state: 'State) (v: Vector2<'T>) : 'State =
        folder (folder state v.X) v.Y
    
    let inline reduce reducer (v: Vector2<'T>) : 'T =
        reducer v.X v.Y
    
    // Math
    
    let inline add v1 v2 = Vector.add map2 v1 v2
    let inline negate v = Vector.negate map v
    let inline subtract v1 v2 = Vector.subtract map2 v1 v2
    let inline scale v a = Vector.scale map v a
    let inline sum v = Vector.sum reduce v
    let inline length v = Vector.length map reduce v
    let inline normalize v = Vector.normalize map reduce v
    let inline dot v1 v2 = Vector.dot map2 reduce v1 v2
    
    // Conversions
    
    let inline toVector2 (x: ^T) =
        (^T: (member ToTuple: unit -> 'a * 'a) x) |> ofTuple
    
    let inline ofVector2 (v: Vector2<'a>) : ^T =
        let (x1, x2) = toTuple v
        (^T: (static member OfTuple: 'a * 'a -> ^T) (x1, x2))

type Vector2<'T> with
    static member inline ( + ) (v1, v2) = Vector2.add v1 v2
    static member inline (~- ) (v) = Vector2.negate v
    static member inline ( - ) (v1, v2) = Vector2.subtract v1 v2
    static member inline ( * ) (v, x) = Vector2.scale v x


// Aliases

type Vector2i = Vector2<int>

type Vector2f = Vector2<single>
module Vector2f =
    let inline toSystem v =
        System.Numerics.Vector2(v.X, v.Y)
    let inline ofSystem (sysv: System.Numerics.Vector2) =
        Vector2.create sysv.X sysv.Y

type Vector2d = Vector2<double>


// ----
// Size
// ----

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type Size<'T> =
    { Width: 'T
      Height: 'T }

module Size =
    let inline create width height =
        { Width = width; Height = height }
    
    let inline ofTuple (w, h) = create w h
    let inline toTuple size = (size.Width, size.Height)

type Size<'T> with
    member inline this.ToTuple() = Size.toTuple this
    static member inline OfTuple(w, h) = Size.ofTuple (w, h)


type Sizei = Size<int32>
type Sizeui = Size<uint32>
type Sizef = Size<single>
type Sized = Size<double>


// -----
// Color
// -----

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type ColorRgbaF =
    { R: single
      G: single
      B: single
      A: single }

type Color = ColorRgbaF

module ColorRgbaF =
    let inline create r g b a = { R = r; G = g; B = b; A = a }
    let inline createOfRgb r g b = create r g b 1.f
    
    let inline ofTuple (r, g, b, a) = create r g b a
    let inline toTuple col = (col.R, col.G, col.B, col.A)

type ColorRgbaF with
    member this.ToTuple() = ColorRgbaF.toTuple this
    static member OfTuple(r, g, b, a) = ColorRgbaF.ofTuple (r, g, b, a)

// ----
// Area
// ----

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type Area<'T> =
    { Width: 'T
      Height: 'T
      Depth: 'T }

module Area =
    let inline create width height depth =
        { Width = width; Height = height; Depth = depth }
    
    let inline ofTuple (w, h, d) = create w h d
    let inline toTuple area = (area.Width, area.Height, area.Depth)

type Area<'T> with
    member this.ToTuple() = Area.toTuple this
    static member OfTuple(w, h, d) = Area.ofTuple (w, h, d)

type Areai = Area<int32>
type Areaui = Area<uint32>
type Areaf = Area<single>
type Aread = Area<double>



// ================================
// Matrices
// ================================


module Matrix =
    let inline add map2 m1 m2 = map2 (+) m1 m2
    let inline negate map m = map (~-) m


// --------------------------------
// Matrix2x3
// --------------------------------

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type Matrix3x2<'T> =
    { M11: 'T; M12: 'T
      M21: 'T; M22: 'T
      M31: 'T; M32: 'T }

module Matrix3x2 =
    
    let inline create m11 m12 m21 m22 m31 m32 =
        { M11 = m11; M12 = m12;
          M21 = m21; M22 = m22
          M31 = m31; M32 = m32 }
    
    let inline ofTuple (m11, m12, m21, m22, m31, m32) = create m11 m12 m21 m22 m31 m32
    let inline toTuple m = (m.M11, m.M12, m.M21, m.M22, m.M31, m.M32)
    
    let inline ofRowTuples ((m11, m12), (m21, m22), (m31, m32)) = create m11 m12 m21 m22 m31 m32
    let inline toRowTuples m = ((m.M11, m.M12), (m.M21, m.M22), (m.M31, m.M32))
    
    let inline ofColumnTuples ((m11, m21, m31), (m12, m22, m32)) = create m11 m12 m21 m22 m31 m32
    let inline toColumnTuples m = ((m.M11, m.M21, m.M31), (m.M12, m.M22, m.M32))
    
    let inline identity< ^T when ^T : (static member Zero: ^T) and ^T : (static member One: ^T)> : Matrix3x2< ^T> =
        create 1G 0G
               0G 1G
               0G 0G
    
    let inline translation m = Vector2.create m.M31 m.M32
    
    let inline createTranslation (v: Vector2<_>) =
        { identity with
            M31 = v.X; M32 = v.Y }
    
    // Functors
    
    let inline map mapping m =
        create (mapping m.M11) (mapping m.M12)
               (mapping m.M21) (mapping m.M22)
               (mapping m.M31) (mapping m.M32)
    
    let inline map2 mapping m1 m2 =
        create (mapping m1.M11 m2.M11) (mapping m1.M12 m2.M12)
               (mapping m1.M21 m2.M21) (mapping m1.M22 m2.M22)
               (mapping m1.M31 m2.M31) (mapping m1.M32 m2.M32)
    
    // Math
    
    let inline add m1 m2 = Matrix.add map2 m1 m2
    
    let inline negate m = Matrix.negate map m

// --------------------------------
// Matrix4x4
// --------------------------------

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type Matrix4x4<'T> =
    { M11: 'T; M12: 'T; M13: 'T; M14: 'T
      M21: 'T; M22: 'T; M23: 'T; M24: 'T
      M31: 'T; M32: 'T; M33: 'T; M34: 'T
      M41: 'T; M42: 'T; M43: 'T; M44: 'T }

module Matrix4x4 =
    
    let inline create m11 m12 m13 m14 m21 m22 m23 m24 m31 m32 m33 m34 m41 m42 m43 m44 =
        { M11 = m11; M12 = m12; M13 = m13; M14 = m14
          M21 = m21; M22 = m22; M23 = m23; M24 = m24
          M31 = m31; M32 = m32; M33 = m33; M34 = m34
          M41 = m41; M42 = m42; M43 = m43; M44 = m44 }
    
    let inline ofTuple (m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44) =
        create m11 m12 m13 m14 m21 m22 m23 m24 m31 m32 m33 m34 m41 m42 m43 m44
    let inline toTuple m =
        (m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44)
    
    let inline ofRowTuples ((m11, m12, m13, m14), (m21, m22, m23, m24), (m31, m32, m33, m34), (m41, m42, m43, m44)) =
        create m11 m12 m13 m14 m21 m22 m23 m24 m31 m32 m33 m34 m41 m42 m43 m44
    let inline toRowTuples m =
        ((m.M11, m.M12, m.M13, m.M14), (m.M21, m.M22, m.M23, m.M24), (m.M31, m.M32, m.M33, m.M34), (m.M41, m.M42, m.M43, m.M44))
    
    let inline ofColumnTuples ((m11, m21, m31, m41), (m12, m22, m32, m42), (m13, m23, m33, m43), (m14, m24, m34, m44)) =
        create m11 m12 m13 m14 m21 m22 m23 m24 m31 m32 m33 m34 m41 m42 m43 m44
    let inline toColumnTuples m =
        ((m.M11, m.M21, m.M31, m.M41), (m.M12, m.M22, m.M32, m.M42), (m.M13, m.M23, m.M33, m.M43), (m.M14, m.M24, m.M34, m.M44))
    
    let inline translation m = Vector3.create m.M41 m.M42 m.M43

    let inline identity< ^T when ^T : (static member Zero: ^T) and ^T : (static member One: ^T)> : Matrix4x4< ^T> =
        create 1G 0G 0G 0G
               0G 1G 0G 0G
               0G 0G 1G 0G
               0G 0G 0G 1G
    
    let inline createTranslation (v: Vector3<_>) =
        { identity with
              M41 = v.X
              M42 = v.Y
              M43 = v.Z }
    
    let inline createScale x y z =
        { identity with
              M11 = x
              M22 = y
              M33 = z }
    
    let inline transpose m = m |> toRowTuples |> ofColumnTuples
    
    // Functors
    
    let inline map mapping m =
        create (mapping m.M11) (mapping m.M12) (mapping m.M13) (mapping m.M14)
               (mapping m.M21) (mapping m.M22) (mapping m.M23) (mapping m.M24)
               (mapping m.M31) (mapping m.M32) (mapping m.M33) (mapping m.M34)
               (mapping m.M41) (mapping m.M42) (mapping m.M43) (mapping m.M44)
    
    let inline map2 mapping m1 m2 =
        create (mapping m1.M11 m2.M11) (mapping m1.M12 m2.M12) (mapping m1.M13 m2.M13) (mapping m1.M14 m2.M14)
               (mapping m1.M21 m2.M21) (mapping m1.M22 m2.M22) (mapping m1.M23 m2.M23) (mapping m1.M24 m2.M24)
               (mapping m1.M31 m2.M31) (mapping m1.M32 m2.M32) (mapping m1.M33 m2.M33) (mapping m1.M34 m2.M34)
               (mapping m1.M41 m2.M41) (mapping m1.M42 m2.M42) (mapping m1.M43 m2.M43) (mapping m1.M44 m2.M44)
    
    // Math
    
    let inline add v1 v2 = Matrix.add map2 v1 v2
    
    let inline multiply m1 m2 =
        create
            // First row
            (m1.M11 * m2.M11 + m1.M12 * m2.M21 + m1.M13 * m2.M31 + m1.M14 * m2.M41)
            (m1.M11 * m2.M12 + m1.M12 * m2.M22 + m1.M13 * m2.M32 + m1.M14 * m2.M42)
            (m1.M11 * m2.M13 + m1.M12 * m2.M23 + m1.M13 * m2.M33 + m1.M14 * m2.M43)
            (m1.M11 * m2.M14 + m1.M12 * m2.M24 + m1.M13 * m2.M34 + m1.M14 * m2.M44)
            // Second row
            (m1.M21 * m2.M11 + m1.M22 * m2.M21 + m1.M23 * m2.M31 + m1.M24 * m2.M41)
            (m1.M21 * m2.M12 + m1.M22 * m2.M22 + m1.M23 * m2.M32 + m1.M24 * m2.M42)
            (m1.M21 * m2.M13 + m1.M22 * m2.M23 + m1.M23 * m2.M33 + m1.M24 * m2.M43)
            (m1.M21 * m2.M14 + m1.M22 * m2.M24 + m1.M23 * m2.M34 + m1.M24 * m2.M44)
            // Third row
            (m1.M31 * m2.M11 + m1.M32 * m2.M21 + m1.M33 * m2.M31 + m1.M34 * m2.M41)
            (m1.M31 * m2.M12 + m1.M32 * m2.M22 + m1.M33 * m2.M32 + m1.M34 * m2.M42)
            (m1.M31 * m2.M13 + m1.M32 * m2.M23 + m1.M33 * m2.M33 + m1.M34 * m2.M43)
            (m1.M31 * m2.M14 + m1.M32 * m2.M24 + m1.M33 * m2.M34 + m1.M34 * m2.M44)
            // Fourth row
            (m1.M41 * m2.M11 + m1.M42 * m2.M21 + m1.M43 * m2.M31 + m1.M44 * m2.M41)
            (m1.M41 * m2.M12 + m1.M42 * m2.M22 + m1.M43 * m2.M32 + m1.M44 * m2.M42)
            (m1.M41 * m2.M13 + m1.M42 * m2.M23 + m1.M43 * m2.M33 + m1.M44 * m2.M43)
            (m1.M41 * m2.M14 + m1.M42 * m2.M24 + m1.M43 * m2.M34 + m1.M44 * m2.M44)
    
    let inline negate m = Matrix.negate map m
    
    // Misc
    
    let inline createLookAt cameraPosition cameraTarget cameraUpVector =
        let zaxis = Vector3.normalize (cameraPosition - cameraTarget)
        let xaxis = Vector3.normalize (Vector3.cross cameraUpVector zaxis)
        let yaxis = Vector3.cross zaxis xaxis
        create
            xaxis.X yaxis.X zaxis.X 0G
            xaxis.Y yaxis.Y zaxis.Y 0G
            xaxis.Z yaxis.Z zaxis.Z 0G
            (-Vector3.dot xaxis cameraPosition)
            (-Vector3.dot yaxis cameraPosition)
            (-Vector3.dot zaxis cameraPosition)
            1G
    
    type CreatePerspectiveError<'T> =
        | ``NearPlaneDistance <= 0`` of 'T
        | ``FarPlaneDistance <= 0`` of 'T
        | ``NearPlaneDistance >= FarPlaneDistance``
    
    let inline createPerspective width height (nearPlaneDistance: ^T) farPlaneDistance =
        if nearPlaneDistance <= 0G  then
            ``NearPlaneDistance <= 0`` nearPlaneDistance |> Error
        elif farPlaneDistance <= 0G then
            ``FarPlaneDistance <= 0`` farPlaneDistance |> Error
        elif nearPlaneDistance >= farPlaneDistance then
            ``NearPlaneDistance >= FarPlaneDistance`` |> Error
        else
            let negFarRange =
                if (^T: (static member IsPositiveInfinity: ^T -> bool) nearPlaneDistance) then - 1G
                else farPlaneDistance / (nearPlaneDistance - farPlaneDistance)
            create
                ((1G+1G) * nearPlaneDistance / width) 0G 0G 0G
                0G ((1G+1G) * nearPlaneDistance / height) 0G 0G
                0G 0G negFarRange (- 1G)
                0G 0G 0G (nearPlaneDistance * negFarRange)
            |> Ok


type Matrix4x4<'T> with
    static member inline ( + ) (m1, m2) = Matrix4x4.add m1 m2
    static member inline ( * ) (m1, m2) = Matrix4x4.multiply m1 m2
    static member inline (~- ) (m) = Matrix4x4.negate m

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
