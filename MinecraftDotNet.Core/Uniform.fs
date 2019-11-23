namespace MinecraftDotNet.Core
open OpenTK.Graphics.OpenGL4

type UniformType =
    | Vec1i of int
    | Vec1f of float32
//    | Vec2
//    | Vec3
//    | Vec4

type Uniform =
    {
        ShaderId: GlId
        Location: int
        Name: string
    }
    
module Uniform =
    let set v uniform =
        let loc = uniform.Location
        match v with
        | Vec1i x -> GL.Uniform1(loc, x)
        | Vec1f x -> GL.Uniform1(loc, x)