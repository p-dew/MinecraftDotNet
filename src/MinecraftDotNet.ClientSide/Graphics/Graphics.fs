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



type SingleBlockChunkRenderer(camera: Camera) =

    static let cubeVerticesRaw =
        seq {
            // X = 0, two triangles
            0, 0, 0;  0, 0, 1;  0, 1, 0
            0, 0, 1;  0, 1, 0;  0, 1, 1
            // X = 1, two triangles
            1, 0, 0;  1, 0, 1;  1, 1, 0
            1, 0, 1;  1, 1, 0;  1, 1, 1
            // Y = 0, two triangles
            0, 0, 0;  0, 0, 1;  1, 0, 0
            0, 0, 1;  1, 0, 0;  1, 0, 1
            // Y = 1, two triangles
            0, 1, 0;  0, 1, 1;  1, 1, 0
            0, 1, 1;  1, 1, 0;  1, 1, 1
            // Z = 0, two triangles
            0, 0, 0;  0, 1, 0;  1, 0, 0
            0, 1, 0;  1, 0, 0;  1, 1, 0
            // Z = 1, two triangles
            0, 0, 1;  0, 1, 1;  1, 0, 1
            0, 1, 1;  1, 0, 1;  1, 1, 1
        }
    static let cubeVertices =
        cubeVerticesRaw
        |> Seq.map (fun (x, y, z) -> Vector3d(float x, float y, float z))
        |> Seq.toArray

    static let cubeUv =
        let inline h k = 1. - (float k / 3.) // mirror on Y
        let inline w k = float k / 4.
        seq {
            0, 1;  1, 1;  0, 2
            1, 1;  0, 2;  1, 2

            3, 1;  2, 1;  3, 2
            2, 1;  3, 2;  2, 2

            1, 0;  1, 1;  2, 0
            1, 1;  2, 0;  2, 1

            1, 3;  1, 2;  2, 3
            1, 2;  2, 3;  2, 2

            4, 1;  4, 2;  3, 1
            4, 2;  3, 1;  3, 2

            1, 1;  1, 2;  2, 1
            1, 2;  2, 1;  2, 2
        }
        |> Seq.map (fun (x, y) -> Vector2d(w x, h y))
        |> Seq.toArray

//    static let cubeEbo =
//        [|
//            // 4; 5; 6; 7
//            // 0; 1; 2; 3
//            // 2; 3; 6; 7
//            // 0; 1; 4; 5
//            // 1; 3; 5; 7
//            // 0; 2; 4; 6
//
//            0; 2; 3
//            3; 1; 0
//
//            1; 3; 7
//            7; 5; 1
//
//            5; 7; 6
//            6; 4; 5
//
//            4; 6; 2
//            2; 0; 4
//
//            0; 1; 5
//            5; 4; 0
//
//            7; 3; 2
//            2; 6; 7
//        |]

    // ----

    let mutable vao: VertexArray = Unchecked.defaultof<_>
    let mutable cubeVertexBuffer: Buffer<Vector3d> = Unchecked.defaultof<_>
//    let mutable cubeElementBuffer: Buffer<int> = Unchecked.defaultof<_>
    let mutable cubeUvBuffer: Buffer<Vector2d> = Unchecked.defaultof<_>

//    let mutable program: BlockProgram = Unchecked.defaultof<_>
    let mutable programHandle: int = Unchecked.defaultof<_>
    let mutable mvpMatrixUniformLocation: int = Unchecked.defaultof<_>
    let mutable sideUniformLocation: int = Unchecked.defaultof<_>


    let renderCube (context: ChunkRenderContext) (blockInfo: BlockInfo) x y z =
        let mutable mvpMatrix =
            Matrix4.CreateTranslation(Vector3(x, y, z))
            * context.ViewMatrix
            * context.ProjectionMatrix
        GL.UniformMatrix4(mvpMatrixUniformLocation, false, &mvpMatrix)

        let tex = blockInfo.TextureSheet
        let texUnit = TextureUnit.Texture0
        GL.Uniform1(sideUniformLocation, int texUnit)
        tex.Bind(texUnit)

        vao.DrawArrays(PrimitiveType.Triangles, 0, 3 * 2 * 6)

//        for i in 0 .. 2 do
//            let tex = blockInfo.Sides.Textures.[i]
//            // program.Side.BindTexture(TextureUnit.Texture0, tex)
//            let texUnit = TextureUnit.Texture0
//            GL.Uniform1(sideUniformLocation, int texUnit)
//            tex.Bind(texUnit)
//
//            vao.DrawArrays(PrimitiveType.TriangleStrip, i * 4, 4)
        ()

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

//            cubeElementBuffer <- new Buffer<_>()
//            cubeElementBuffer.Init(BufferTarget.ArrayBuffer, cubeEbo)

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
//            cubeElementBuffer.Dispose()
            cubeUvBuffer.Dispose()
//            program.Dispose()
            GL.DeleteProgram(programHandle)
            vao.Dispose()
