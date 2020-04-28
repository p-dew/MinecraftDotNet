module MinecraftDotNet.ClientSide.Graphics.OpenGl.Graphics

open System
open MinecraftDotNet.ClientSide.Graphics.Core
open MinecraftDotNet.ClientSide.Graphics.Core.Graphics
open MinecraftDotNet.Core.Math
open OpenTK.Graphics.OpenGL


type ResultBuilder() =
    let ofOption error = function Some s -> Ok s | None -> Error error
    member _.Return(x) = Ok x
    member _.ReturnFrom(m: Result<_, _>) = m
    member _.Bind(m, f) = Result.bind f m
    member _.Bind((m, error): (Option<'T> * 'E), f) = m |> ofOption error |> Result.bind f
    member _.Zero() = None
    member _.Combine(m, f) = Result.bind f m
    member _.Delay(f: unit -> _) = f
    member _.Run(f) = f()
    member this.TryWith(m, h) = try this.ReturnFrom(m) with e -> h e
    member this.TryFinally(m, compensation) = try this.ReturnFrom(m) finally compensation()
    member this.Using(res:#IDisposable, body) = this.TryFinally(body res, fun () -> match res with null -> () | disp -> disp.Dispose())
    member this.While(guard, f) =
        if not (guard()) then Ok () else
        do f() |> ignore
        this.While(guard, f)
    member this.For(sequence:seq<_>, body) = this.Using(sequence.GetEnumerator(), fun enum -> this.While(enum.MoveNext, this.Delay(fun () -> body enum.Current)))
let result = ResultBuilder()


let loadShader (Glsl (src, shaderType)) =
    let shader = GL.CreateShader(match shaderType with Vertex -> ShaderType.VertexShader | Fragment -> ShaderType.FragmentShader)
    GL.ShaderSource(shader, src)
    GL.CompileShader(shader)
    
    let status = GL.GetShader(shader, ShaderParameter.CompileStatus)
    if status = 0 then
        let infoLog = GL.GetShaderInfoLog(shader)
        infoLog |> Error
    else
        shader |> Ok

let loadProgram material =
    let program = GL.CreateProgram()
    result {
        let! vertShader = loadShader material.VertexShader
        let! fragShader = loadShader material.FragmentShader
        
        GL.AttachShader(program, vertShader)
        GL.AttachShader(program, fragShader)
        
        GL.LinkProgram(program)
        
        GL.DeleteShader(vertShader)
        GL.DeleteShader(fragShader)
        
        let status = GL.GetProgram(program, GetProgramParameterName.LinkStatus)
        if status = 0 then
            return! GL.GetProgramInfoLog(program) |> Error
        else
            return! program |> Ok
    }

let vertexLocation = 0
let uvLocation = 2

let instrs =
    {
        Clear = fun color ->
            GL.ClearColor(color.R, color.G, color.B, 1.0f)
            GL.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
        
        LoadTexture = fun tex ->
            let texH = GL.GenTexture()
            GL.BindTexture(TextureTarget.Texture2D, texH)
            GL.TexImage2D(
                TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                tex.Bitmap.Width, tex.Bitmap.Height, 0, PixelFormat.Rgba, PixelType.Byte,
                tex.Bitmap.GetHbitmap() )
            GL.BindTexture(TextureTarget.Texture2D, 0)
            texH |> TextureHandler
        
        UnloadTexture = fun texH ->
            let (TextureHandler texH') = texH
            GL.DeleteTexture(texH')
        
        LoadMesh = fun mesh ->
            let vao = GL.GenVertexArray()
            GL.BindVertexArray(vao)

            let vbo = GL.GenBuffer()
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo)
            GL.BufferData(BufferTarget.ArrayBuffer, mesh.Vertices.Length * 3 * sizeof<float32>, mesh.Vertices, BufferUsageHint.DynamicRead)

            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 0, 0)
            GL.EnableVertexAttribArray(vertexLocation)
            
            let uvs = GL.GenBuffer()
            GL.BindBuffer(BufferTarget.ArrayBuffer, uvs)
            GL.BufferData(BufferTarget.ArrayBuffer, mesh.Uvs.Length * 2 * sizeof<float32>, mesh.Uvs, BufferUsageHint.DynamicRead)
            
            GL.VertexAttribPointer(uvLocation, 2, VertexAttribPointerType.Float, false, 0, 0)
            GL.EnableVertexAttribArray(uvLocation)
            
            let ebo = GL.GenBuffer()
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo)
            GL.BufferData(BufferTarget.ElementArrayBuffer, mesh.Elements.Length * sizeof<int>, mesh.Elements, BufferUsageHint.DynamicRead)
            
            GL.VertexArrayElementBuffer(vao, ebo)
            
            GL.BindVertexArray(0)
            vao |> MeshHandler
        
        UnloadMesh = fun meshH ->
            NotImplementedException() |> raise
        
        LoadMaterial = fun mat ->
            let program = loadProgram mat
            match program with
            | Ok x -> x |> MaterialHandler
            | Error err -> InvalidOperationException(err) |> raise
        
        UnloadMaterial = fun matH ->
            let (MaterialHandler matH') = matH
            GL.DeleteProgram(matH')
        
        DrawMesh = fun (MeshHandler meshH, TextureHandler texH, MaterialHandler matH) ->
            GL.BindVertexArray(meshH)
            GL.BindTexture(TextureTarget.Texture2D, texH)
            GL.UseProgram(matH)
            
            let ebo = GL.GetVertexArray(meshH, VertexArrayParameter.ElementArrayBufferBinding)
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo)
            let mutable eboSize = 0
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, &eboSize)
            
            GL.DrawElements(PrimitiveType.Triangles, eboSize, DrawElementsType.UnsignedInt, 0)
            
            GL.BindVertexArray(0)
            GL.BindTexture(TextureTarget.Texture2D, 0)
            GL.UseProgram(0)
    }

let interpret prog = Graphics.simpleInterpret instrs prog
