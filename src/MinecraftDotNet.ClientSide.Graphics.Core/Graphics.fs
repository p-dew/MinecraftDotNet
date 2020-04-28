module MinecraftDotNet.ClientSide.Graphics.Core.Graphics

open MinecraftDotNet.Core.Math
open MinecraftDotNet.Core.Resources

type private GraphicsHandler = int
type TextureHandler = TextureHandler of GraphicsHandler
type MeshHandler = MeshHandler of GraphicsHandler
type MaterialHandler = MaterialHandler of GraphicsHandler

type private ResourceLoader<'Resource, 'Handler> = 'Resource -> 'Handler
type private ResourceUnloader<'Handler> = 'Handler -> unit
type TextureLoader    = ResourceLoader<Texture, TextureHandler>
type TextureUnloader  = ResourceUnloader<TextureHandler>
type MeshLoader       = ResourceLoader<Mesh, MeshHandler>
type MeshUnloader     = ResourceUnloader<MeshHandler>
type MaterialLoader   = ResourceLoader<Material, MaterialHandler>
type MaterialUnloader = ResourceUnloader<MaterialHandler>

type Cleaner = Color -> unit
type MeshDrawer = MeshHandler * TextureHandler * MaterialHandler -> unit

type RenderMiddleware<'Args, 'Retn, 'a> = 'Args * ('Retn -> 'a)

type RenderInstruction<'a> =
    | Clear of RenderMiddleware<Color, unit, 'a>
    | LoadTexture of RenderMiddleware<Texture, TextureHandler, 'a>
    | UnloadTexture of RenderMiddleware<TextureHandler, unit, 'a>
    | LoadMesh of RenderMiddleware<Mesh, MeshHandler, 'a>
    | UnloadMesh of RenderMiddleware<MeshHandler, unit, 'a>
    | LoadMaterial of RenderMiddleware<Material, MaterialHandler, 'a>
    | UnloadMaterial of RenderMiddleware<MaterialHandler, unit, 'a>
    | DrawMesh of RenderMiddleware<MeshHandler * TextureHandler * MaterialHandler, unit, 'a>

module RenderInstruction =
    let map instr mapping =
        match instr with
        | Clear          (x, next) -> Clear          (x, next >> mapping)
        | LoadTexture    (x, next) -> LoadTexture    (x, next >> mapping)
        | UnloadTexture  (x, next) -> UnloadTexture  (x, next >> mapping)
        | LoadMesh       (x, next) -> LoadMesh       (x, next >> mapping)
        | UnloadMesh     (x, next) -> UnloadMesh     (x, next >> mapping)
        | LoadMaterial   (x, next) -> LoadMaterial   (x, next >> mapping)
        | UnloadMaterial (x, next) -> UnloadMaterial (x, next >> mapping)
        | DrawMesh       (x, next) -> DrawMesh       (x, next >> mapping)

type RenderProgram<'a> =
    | Free of RenderInstruction<RenderProgram<'a>>
    | Pure of 'a

module RenderProgram =
    
    let rec map x mapping =
        let inline flip f x y = f y x
        match x with
        | Pure x' -> mapping x' |> Pure
        | Free x' -> RenderInstruction.map x' (flip map mapping) |> Free
    
    let rec join x =
        match x with
        | Free x' -> RenderInstruction.map x' join |> Free
        | Pure x' -> x'
    
    let rec bind x binding =
        join (map x binding)
    
    let clear color = Clear (color, Pure) |> Free
    
    let loadTexture texture = LoadTexture (texture, Pure) |> Free
    let unloadTexture textureH = UnloadTexture (textureH, Pure) |> Free
    
    let loadMesh mesh = LoadMesh (mesh, Pure) |> Free
    let unloadMesh meshH = UnloadMesh (meshH, Pure) |> Free
    
    let loadMaterial material = LoadMaterial (material, Pure) |> Free
    let unloadMaterial materialH = UnloadMaterial (materialH, Pure) |> Free
    
    let drawMesh meshH textureH materialH = DrawMesh ((meshH, textureH, materialH), Pure) |> Free


type RendererBuilder() =
    member _.Bind(x, binding) = RenderProgram.bind x binding
    member _.Return(x) = Pure x
    member _.ReturnFrom(x) = x
    member _.Zero() = Pure ()
//    member _.Quote(expr): Quotations.Expr -> Quotations.Expr =
//        System.NotImplementedException() |> raise

let render = RendererBuilder()

type InstructionsSet =
    { Clear: Cleaner
      LoadTexture: TextureLoader
      UnloadTexture: TextureUnloader
      LoadMesh: MeshLoader
      UnloadMesh: MeshUnloader
      LoadMaterial: MaterialLoader
      UnloadMaterial: MaterialUnloader
      DrawMesh: MeshDrawer }

type IProgramInstructions =
    abstract member Clear: Cleaner
    abstract member LoadTexture: TextureLoader
    abstract member UnloadTexture: TextureUnloader
    abstract member LoadMesh: MeshLoader
    abstract member UnloadMesh: MeshUnloader
    abstract member LoadMaterial: MaterialLoader
    abstract member UnloadMaterial: MaterialUnloader
    abstract member DrawMesh: MeshDrawer

type Interpreter<'a> = RenderProgram<'a> -> 'a

let rec simpleInterpret instrs prog =
    match prog with
    | Pure x -> x
    | Free instr ->
        match instr with
        | Clear (col, next) -> instrs.Clear (col) |> next
        | LoadTexture (texture, next) -> instrs.LoadTexture (texture) |> next
        | UnloadTexture (texH, next) -> instrs.UnloadTexture (texH) |> next
        | LoadMesh (mesh, next) -> instrs.LoadMesh (mesh) |> next
        | UnloadMesh (meshH, next) -> instrs.UnloadMesh (meshH) |> next
        | LoadMaterial (material, next) -> instrs.LoadMaterial (material) |> next
        | UnloadMaterial (materialH, next) -> instrs.UnloadMaterial (materialH) |> next
        | DrawMesh ((meshH, texH, matH), next) -> instrs.DrawMesh (meshH, texH, matH) |> next
        |> simpleInterpret instrs
