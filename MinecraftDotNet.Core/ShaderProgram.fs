namespace MinecraftDotNet.Core

open System
open System.IO
open System.Text
open OpenTK.Graphics.OpenGL4

type ShaderProgram(vertexPath: string, fragmentPath: string) =
        
    let mutable _handle: int = 0
    
    let mutable _disposedValue = false
    
    let loadShaders() =
        
        use reader = new StreamReader(vertexPath, Encoding.UTF8)
        let vertexShaderSource = reader.ReadToEnd()

        use reader = new StreamReader(fragmentPath, Encoding.UTF8)
        let fragmentShaderSource = reader.ReadToEnd()
        
        //
        
        let vertexShader = GL.CreateShader(ShaderType.VertexShader)
        GL.ShaderSource(vertexShader, vertexShaderSource)

        let fragmentShader = GL.CreateShader(ShaderType.FragmentShader)
        GL.ShaderSource(fragmentShader, fragmentShaderSource)
        
        //
        
        GL.CompileShader(vertexShader)

        let infoLogVert = GL.GetShaderInfoLog(vertexShader)
        if infoLogVert <> System.String.Empty then
            System.Console.WriteLine(infoLogVert)

        GL.CompileShader(fragmentShader)

        let infoLogFrag = GL.GetShaderInfoLog(fragmentShader)

        if infoLogFrag <> System.String.Empty then
            System.Console.WriteLine(infoLogFrag)
        
        //
        
        _handle <- GL.CreateProgram()

        GL.AttachShader(_handle, vertexShader)
        GL.AttachShader(_handle, fragmentShader)

        GL.LinkProgram(_handle)
        
        //
        
        GL.DetachShader(_handle, vertexShader)
        GL.DetachShader(_handle, fragmentShader)
        GL.DeleteShader(fragmentShader)
        GL.DeleteShader(vertexShader)
    
    do loadShaders()

    member this.Use() =
        GL.UseProgram(_handle)

    member private this.Dispose(disposing: bool) =
        if _disposedValue then ()
        
        GL.DeleteProgram(_handle)

        _disposedValue <- true
    
    interface IDisposable with
        member this.Dispose() =
            this.Dispose(true)
            GC.SuppressFinalize(this)

    override this.Finalize() =
        GL.DeleteProgram(_handle)
