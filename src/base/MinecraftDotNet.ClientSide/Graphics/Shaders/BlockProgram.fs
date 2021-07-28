namespace MinecraftDotNet.ClientSide.Graphics.Shaders

open ObjectTK.Shaders
open ObjectTK.Shaders.Sources
open ObjectTK.Shaders.Variables
open ObjectTK.Textures
open OpenTK
open OpenTK.Graphics.OpenGL

[<VertexShaderSource("BlockShader.Vertex")>]
[<FragmentShaderSource("BlockShader.Fragment")>]
type BlockProgram() =
    inherit Program()
    
    // Vertex
    
    [<VertexAttrib(3, VertexAttribPointerType.Double)>]
    member _.InVertex
        with get(): VertexAttrib = Unchecked.defaultof<_>
        and set (_: VertexAttrib) = ()
    
    [<VertexAttrib(2, VertexAttribPointerType.Double)>]
    member _.InUv
        with get(): VertexAttrib = Unchecked.defaultof<_>
        and set (_: VertexAttrib) = ()
    
    member _.MvpMatrix
        with get(): Uniform<Matrix4> = Unchecked.defaultof<_>
        and set(_: Uniform<Matrix4>) = ()
    
    // Fragment
    
    member val Side: TextureUniform<Texture2D> = Unchecked.defaultof<_> with get, set
