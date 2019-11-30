namespace MinecraftDotNet.Core.Graphics

open System
open System.IO
open System.Text
open OpenTK.Graphics.OpenGL4
open MinecraftDotNet.Core.Graphics.OpenGl

type ShaderProgram =
    {
        GlHandle: GlHandler
    }

module ShaderProgram =
    
    let private loadShader path t errorHandler =
        let source = File.ReadAllText(path, Encoding.UTF8)
        let shader = GL.CreateShader(t)
        GL.ShaderSource(shader, source)
        GL.CompileShader(shader)
        let infoLog = GL.GetShaderInfoLog(shader)
        if infoLog <> String.Empty then
            errorHandler infoLog
        shader
    
    let load vertPath fragPath =
        let vert = loadShader vertPath ShaderType.VertexShader Console.WriteLine
        let frag = loadShader fragPath ShaderType.FragmentShader Console.WriteLine
        
        let prog = GL.CreateProgram()
        GL.AttachShader(prog, vert)
        GL.AttachShader(prog, frag)
        
        GL.LinkProgram(prog)
        
        GL.DetachShader(prog, vert)
        GL.DetachShader(prog, frag)
        GL.DeleteShader(frag)
        GL.DeleteShader(vert)
        
        {
            GlHandle = GlHandler prog
        }
    
    let use1 ({ GlHandle = GlHandler prog }: ShaderProgram) =
        GL.UseProgram(prog)

    let delete ({ GlHandle = GlHandler prog }: ShaderProgram) =
        GL.DeleteProgram(prog)
