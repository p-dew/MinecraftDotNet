module MinecraftDotNet.ClientSide.Graphics.OpenGl.Window

open System
open MinecraftDotNet.Core.Math
open MinecraftDotNet.ClientSide.Graphics.Core
open MinecraftDotNet.ClientSide.Graphics.Core.Graphics
open MinecraftDotNet.ClientSide.Graphics.Core.Graphics
open MinecraftDotNet.ClientSide.Graphics.OpenGl
open MinecraftDotNet.ClientSide.Graphics.OpenGl
open MinecraftDotNet.ClientSide.Graphics.OpenGl
open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL

[<Sealed>]
type private McGameWindow(title, size: Sizei) =
    inherit GameWindow(size.Width, size.Height, GraphicsMode.Default, title)
    
    let mutable _mesh = Unchecked.defaultof<MeshHandler>
    let mutable _texture = Unchecked.defaultof<TextureHandler>
    let mutable _material = Unchecked.defaultof<MaterialHandler>
    
    member private this._onExit() = this.OnClosed EventArgs.Empty
    member private this._onRender delay = this.OnRenderFrame(FrameEventArgs(delay))
    
    member this.ToWindow() =
        { Title = title
          Size = size
          IsClosing = false
          Runner = this.Run
          OnExit = this._onExit
          OnRender = this._onRender }
    
    override this.OnLoad(args) =
        let mesh, texture, material =
            render {
                let! mesh =
                    RenderProgram.loadMesh
                        (Mesh.create
                             ([| -1.f, -1.f; 0.f, 1.f; 1.f, -1.f |]
                              |> Array.map (fun (x, y) ->
                                  { Vertex = Vector3.create x y -1.f
                                    Uv = Vector2.create x y |> Vector2.map (fun e -> (e + 1.f) / 2.f) }
                                  )
                              )
                              [| 0; 1; 2 |])
                let texture = TextureHandler 0
                let! material =
                    Material.create
                        (Glsl ("""
                            #version 330
                            layout (location = 0) in vec3 vertex;
                            layout (location = 2) in vec2 uv;
                            void main()
                            {
                                gl_Position = vec4(vertex, 1.0);
                            }
                            """, Vertex))
                        (Glsl ("""
                            #version 330
                            out vec4 FragColor;
                            void main()
                            {
                                FragColor = vec4(1.0, 0.0, 1.0, 1.0);
                            }
                            """, Fragment))
                    |> Option.get
                    |> RenderProgram.loadMaterial
                
                return mesh, texture, material
            } |> Graphics.interpret
        _texture <- texture
        _mesh <- mesh
        _material <- material
    
    override this.OnRenderFrame(args) =
        render {
            do! RenderProgram.clear (ColorRgbaF.create 0.f 0.f 1.f 1.f)
            do! RenderProgram.drawMesh _mesh _texture _material
        } |> Graphics.interpret
        
        this.SwapBuffers()

let create title size =
    let gameWindow = new McGameWindow(title, size)
    gameWindow.ToWindow()