namespace MinecraftDotNet.ClientSide.Graphics

open System
open System.IO
open MinecraftDotNet.ClientSide.Graphics.Shaders
open MinecraftDotNet.Core
open MinecraftDotNet.Core.Blocks
open MinecraftDotNet.Core.Blocks.Chunks
open MinecraftDotNet.Core.Items
open ObjectTK.Buffers
open ObjectTK.Shaders
open ObjectTK.Tools.Cameras
open OpenTK
open OpenTK.Graphics.OpenGL

// ChunkRenderContext

type ChunkRenderContext =
    { ProjectionMatrix: Matrix4
      ViewMatrix: Matrix4 }

// IChunkRenderer

type IChunkRenderer =
    abstract Render: context: ChunkRenderContext * chunk: Chunk * chunkCoords: ChunkCoords -> unit

// SingleBlockChunkRenderer

module Absorb =
    let mutable _value: obj = null
    let inline absorb x =
        _value <- x


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
            raise (ShaderCompileException str)

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

type SingleBlockChunkRenderer(camera: Camera) =

    static let cubeVertices =
        seq {
            0, 0, 0;  0, 0, 1;  0, 1, 0;  0, 1, 1  // X = 0
            1, 0, 0;  1, 0, 1;  1, 1, 0;  1, 1, 1  // X = 1
        }
        |> Seq.map (fun (x, y, z) -> Vector3d(float x, float y, float z))
        |> Seq.toArray

    static let cubeUv =
        seq {
            0, 0;  0, 1;  1, 0;  1, 1;  0, 0;  0, 1;  1, 0;  1, 1
            0, 0;  0, 1;  1, 0;  1, 1;  0, 0;  0, 1;  1, 0;  1, 1
            0, 0;  0, 1;  1, 0;  1, 1;  0, 0;  0, 1;  1, 0;  1, 1
        }
        |> Seq.map (fun (x, y) -> Vector2d(float x, float y))
        |> Seq.toArray

    static let cubeEbo =
        [|
            // 4; 5; 6; 7
            // 0; 1; 2; 3
            // 2; 3; 6; 7
            // 0; 1; 4; 5
            // 1; 3; 5; 7
            // 0; 2; 4; 6

            0; 2; 3
            3; 1; 0

            1; 3; 7
            7; 5; 1

            5; 7; 6
            6; 4; 5

            4; 6; 2
            2; 0; 4

            0; 1; 5
            5; 4; 0

            7; 3; 2
            2; 6; 7
        |]

    static let uvs =
        [|
            Vector2d(0., 0.)
            Vector2d(0., 1.)
            Vector2d(1., 0.)
            Vector2d(1., 1.)
        |]

    // ----

    let mutable vao: VertexArray = Unchecked.defaultof<_>
    let mutable cubeVertexBuffer: Buffer<Vector3d> = Unchecked.defaultof<_>
    let mutable cubeElementBuffer: Buffer<int> = Unchecked.defaultof<_>
    let mutable cubeUvBuffer: Buffer<Vector2d> = Unchecked.defaultof<_>

//    let mutable program: BlockProgram = Unchecked.defaultof<_>
    let mutable programHandle: int = Unchecked.defaultof<_>
    let mutable mvpMatrixUniformLocation: int = Unchecked.defaultof<_>
    let mutable sideUniformLocation: int = Unchecked.defaultof<_>

    let renderCube (context: ChunkRenderContext) (blockInfo: BlockInfo) x y z =
//        program.MvpMatrix.Set(
//            Matrix4.CreateTranslation(Vector3(x, y, z))
//            * context.ViewMatrix
//            * context.ProjectionMatrix
//        )
        let mvpMatrix =
            Matrix4.CreateTranslation(Vector3(x, y, z))
            * context.ViewMatrix
            * context.ProjectionMatrix
        GL.UniformMatrix4(mvpMatrixUniformLocation, false, ref mvpMatrix)

        for i in 0 .. 2 do
            let tex = blockInfo.Sides.Textures.[i]
//            program.Side.BindTexture(TextureUnit.Texture0, tex)
            let texUnit = TextureUnit.Texture0
            GL.Uniform1(sideUniformLocation, int texUnit)
            tex.Bind(texUnit)

            vao.DrawArrays(PrimitiveType.TriangleStrip, i * 4, 4)

    interface IGlInitializable with
        member this.InitGl() =
    //        program <- ProgramFactory.Create<BlockProgram>()
            programHandle <-
                ShaderProgram.create
                    (File.ReadAllText "./Data/Shaders/BlockShader.Vertex.glsl")
                    (File.ReadAllText "./Data/Shaders/BlockShader.Fragment.glsl")
            mvpMatrixUniformLocation <- GL.GetUniformLocation(programHandle, "MvpMatrix")
            sideUniformLocation <- GL.GetUniformLocation(programHandle, "Side")

            cubeVertexBuffer <- new Buffer<_>()
            cubeVertexBuffer.Init(BufferTarget.ElementArrayBuffer, cubeVertices)

            cubeElementBuffer <- new Buffer<_>()
            cubeElementBuffer.Init(BufferTarget.ArrayBuffer, cubeEbo)

            cubeUvBuffer <- new Buffer<_>()
            cubeUvBuffer.Init(BufferTarget.ArrayBuffer, cubeUv)

            vao <- new VertexArray()
            vao.Bind()


    //        vao.BindAttribute(program.InVertex, cubeVertexBuffer)
            vao.BindAttribute(0, cubeVertexBuffer, 3, VertexAttribPointerType.Double, 0, 0, false)

    //        vao.BindAttribute(program.InUv, cubeUvBuffer)
            vao.BindAttribute(1, cubeUvBuffer, 2, VertexAttribPointerType.Double, 0, 0, false)

            // vao.BindElementBuffer(cubeElementBuffer)

    interface IChunkRenderer with
        member this.Render(context, chunk, chunkCoords) =
            vao.Bind()
//            program.Use()
            GL.UseProgram(programHandle)

//            program.MvpMatrix.Value <- camera.GetCameraTransform()
            let mvpMatrix = camera.GetCameraTransform()
            GL.UniformMatrix4(mvpMatrixUniformLocation, false, ref mvpMatrix)

            chunk.Blocks |> Array3D.iteri (fun x y z blockInfo ->
                match blockInfo.ItemInfo.Id with
                | ItemId "air" -> () // skip air
                | _ ->
                    let blockX = chunkCoords.X * Chunk.Size.Width + x
                    let blockY = y
                    let blockZ = chunkCoords.Z * Chunk.Size.Depth + z
                    renderCube context blockInfo (float32 blockX) (float32 blockY) (float32 blockZ)
            )

            GL.UseProgram(0)
            GL.BindVertexArray(0)

    interface IDisposable with
        member this.Dispose() =
            cubeVertexBuffer.Dispose()
            cubeElementBuffer.Dispose()
            cubeUvBuffer.Dispose()
//            program.Dispose()
            GL.DeleteProgram(programHandle)
            vao.Dispose()
