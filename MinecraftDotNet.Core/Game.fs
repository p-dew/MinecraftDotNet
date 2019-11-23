namespace MinecraftDotNet.Core

open OpenTK
open OpenTK.Graphics.OpenGL4
open OpenTK.Input

type Game(window: GameWindow) =
    
    let vertices = [| -0.5f; -0.5f; 0.0f;
                       0.5f; -0.5f; 0.0f;
                       0.0f;  0.5f; 0.0f; |]
    
    let vbo = GL.GenBuffer()
    
    let shaderProgram = new ShaderProgram("./Shaders/std_shader.vert.glsl", "./Shaders/std_shader.frag.glsl")
    
    let vao = GL.GenVertexArray()
    do GL.BindVertexArray vao
    do GL.BindBuffer(BufferTarget.ArrayBuffer, vbo)
    do GL.BufferData(BufferTarget.ArrayBuffer, (Array.length vertices) * sizeof<float32>, vertices, BufferUsageHint.StaticDraw)
    do GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof<float32>, 0)
    do GL.EnableVertexAttribArray(0)
    do GL.BindBuffer(BufferTarget.ArrayBuffer, 0) 
    
    // Events
    
    let renderFrame args =
        
        match GL.GetError() with
        | ErrorCode.NoError -> ()
        | err -> printfn "%s" (err.ToString())
        
        let input = Keyboard.GetState()
        if input.IsKeyDown Key.Escape then
            window.Exit()
        GL.Clear(ClearBufferMask.ColorBufferBit)
    
        shaderProgram.Use()
        GL.BindVertexArray vao
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3)
        GL.BindVertexArray 0
        
        window.Context.SwapBuffers()
    
    let load args =
        GL.ClearColor(Color.SkyBlue)
    
    do window.RenderFrame.Add renderFrame
    do window.Load.Add load
    
    member this.Start() =
        window.Run()