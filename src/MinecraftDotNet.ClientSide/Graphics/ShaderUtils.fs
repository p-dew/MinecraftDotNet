namespace MinecraftDotNet.ClientSide.Graphics

open System
open OpenTK.Graphics.OpenGL

module Shader =

    open System.Diagnostics

    exception ShaderCompileException of infoLog: string

    let private checkCompileStatus (shaderHandle: int) =
        let params' = GL.GetShader(shaderHandle, ShaderParameter.CompileStatus)
        Debug.Print("Compile status: ", params')
        let str = GL.GetShaderInfoLog(shaderHandle)
        if not (String.IsNullOrEmpty(str)) then
            Debug.Print("Compile log:\n", str)
        if params' <> 1 then
            Debug.Print("Error compiling shader.")
//            raise (ShaderCompileException str)
            failwithf $"Shader compile error:\n{str}"

    let compileSource source shaderType =
        let shaderHandle = GL.CreateShader(shaderType)
        GL.ShaderSource(shaderHandle, source)
        GL.CompileShader(shaderHandle)
        checkCompileStatus shaderHandle
        shaderHandle

module ShaderProgram =

    exception ProgramLinkException of programInfoLog: string

    let private checkLinkStatus (programHandle: int) =
        let params' = GL.GetProgram(programHandle, GetProgramParameterName.LinkStatus)
        let programInfoLog = GL.GetProgramInfoLog(programHandle)
        if not (String.IsNullOrEmpty(programInfoLog)) then
            ()
        if params' <> 1 then
            raise (ProgramLinkException programInfoLog)

    let create vertexShaderSource fragmentShaderSource =
        let programHandle = GL.CreateProgram()

        let vertexShaderHandle = Shader.compileSource vertexShaderSource ShaderType.VertexShader
        GL.AttachShader(programHandle, vertexShaderHandle)

        let fragmentShaderHandle = Shader.compileSource fragmentShaderSource ShaderType.FragmentShader
        GL.AttachShader(programHandle, fragmentShaderHandle)

        GL.LinkProgram(programHandle)
        checkLinkStatus programHandle

        programHandle
