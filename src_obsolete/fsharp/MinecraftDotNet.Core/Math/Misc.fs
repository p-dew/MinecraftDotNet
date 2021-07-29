module MinecraftDotNet.Core.Math.Misc

open System.Runtime.InteropServices


[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type Size<'T> =
    { Width: 'T
      Height: 'T }

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type ColorRgbaF =
    { R: single
      G: single
      B: single
      A: single }

type Color = ColorRgbaF

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type Area<'T> =
    { Width: 'T
      Height: 'T
      Depth: 'T }


module Size =
    let inline create width height =
        { Width = width; Height = height }
    
    let inline ofTuple (w, h) = create w h
    let inline toTuple (size: Size<_>) = (size.Width, size.Height)

type Size<'T> with
    member inline this.ToTuple() = Size.toTuple this
    static member inline OfTuple(w, h) = Size.ofTuple (w, h)


type Sizei = Size<int32>
type Sizeui = Size<uint32>
type Sizef = Size<single>
type Sized = Size<double>


// ----------------
// Color
// ----------------

module ColorRgbaF =
    let inline create r g b a = { R = r; G = g; B = b; A = a }
    let inline createOfRgb r g b = create r g b 1.f
    
    let inline ofTuple (r, g, b, a) = create r g b a
    let inline toTuple col = (col.R, col.G, col.B, col.A)

type ColorRgbaF with
    member this.ToTuple() = ColorRgbaF.toTuple this
    static member OfTuple(r, g, b, a) = ColorRgbaF.ofTuple (r, g, b, a)


module Area =
    let inline create width height depth =
        { Width = width; Height = height; Depth = depth }
    
    let inline ofTuple (w, h, d) = create w h d
    let inline toTuple area = (area.Width, area.Height, area.Depth)

type Area<'T> with
    member this.ToTuple() = Area.toTuple this
    static member OfTuple(w, h, d) = Area.ofTuple (w, h, d)

// Aliases

type Areai = Area<int32>
type Areaui = Area<uint32>
type Areaf = Area<single>
type Aread = Area<double>