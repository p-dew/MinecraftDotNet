namespace MinecraftDotNet.Core

open System
open System.Threading
open System.Threading.Tasks
open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL4
open OpenTK.Input
open MinecraftDotNet.Core.Graphics


type MinecraftGameWindow(width, height, title) =
    inherit GameWindow(width, height, GraphicsMode.Default, title)
    
    let vertices = [| -0.5f; -0.5f; 0.0f;
                       0.5f; -0.5f; 0.0f;
                       0.0f;  0.5f; 0.0f; |]
    let vao = GL.GenVertexArray()
    
    let shaderProgram = ShaderProgram.load "./Shaders/std_shader.vert.glsl" "./Shaders/std_shader.frag.glsl"
    
    override this.OnLoad(args) =
        let vbo = GL.GenBuffer()
        
        GL.BindVertexArray vao
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo)
        GL.BufferData(BufferTarget.ArrayBuffer, (Array.length vertices) * sizeof<float32>, vertices, BufferUsageHint.StaticDraw)
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof<float32>, 0)
        GL.EnableVertexAttribArray(0)
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0)
    
    override this.OnRenderFrame(args) =
        match GL.GetError() with
        | ErrorCode.NoError -> ()
        | err -> printfn "GLError: %s" (err.ToString())
        
        let input = Keyboard.GetState()
        if input.IsKeyDown Key.Escape then
            this.Exit()
        
        GL.Clear(ClearBufferMask.ColorBufferBit)
        
        ShaderProgram.use1 shaderProgram
        GL.BindVertexArray vao
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3)
        GL.BindVertexArray 0
        
        this.Context.SwapBuffers()

type Game() =
    
    let createWindow() =
        use window = new MinecraftGameWindow(1024, 720, "MinecraftDotNet") :> GameWindow
        window.Run()
    
//    let world = World()
    
    member this.Start() =
        let renderTask = Task.Run(createWindow)
        
        renderTask.Wait()
    
    interface IDisposable with
        member this.Dispose() =
            ()
