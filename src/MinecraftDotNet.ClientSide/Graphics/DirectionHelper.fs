namespace MinecraftDotNet.ClientSide.Graphics

open System
open MinecraftDotNet.Core
open ObjectTK.Buffers
open OpenTK
open OpenTK.Graphics.OpenGL

type DirectionHelper() =

    static let vertices =
        seq {
            0, 0, 0;  0, 0, 1
            0, 0, 0;  0, 1, 0
            0, 0, 0;  1, 0, 0
        }
        |> Seq.map ^fun (x, y, z) -> Vector3d(float x, float y, float z)
        |> Seq.toArray

    static let vertexShaderSource = """#version 330
        in vec3 InVertex;
        out vec3 Vertex;
        uniform mat4 MvpMatrix;
        void main() {
            Vertex = InVertex;
            gl_Position = MvpMatrix * vec4(InVertex, 1);
        }
    """
    static let fragmentShaderSource = """#version 330
        in vec3 Vertex;
        out vec4 FragColor;
        void main() {
            FragColor = vec4(sign(Vertex), 1);
        }
    """

    let mutable vao: VertexArray = Unchecked.defaultof<_>
    let mutable vertexBuffer: Buffer<Vector3d> = Unchecked.defaultof<_>

    let mutable programHandle: int = Unchecked.defaultof<_>
    let mutable mvpMatrixUniformLocation: int = Unchecked.defaultof<_>

    interface IGlInitializable with
        member this.InitGl() =
            programHandle <- ShaderProgram.create vertexShaderSource fragmentShaderSource
            mvpMatrixUniformLocation <- GL.GetUniformLocation(programHandle, "MvpMatrix")

            vertexBuffer <- new Buffer<_>()
            vertexBuffer.Init(BufferTarget.ArrayBuffer, vertices)

            vao <- new VertexArray()
            vao.Bind()
            vao.BindAttribute(0, vertexBuffer, 3, VertexAttribPointerType.Double, 0, 0, false)

    interface IDisposable with
        member this.Dispose() =
            vao.Dispose()
            vertexBuffer.Dispose()
            GL.DeleteProgram(programHandle)

    member this.Render(projectionMatrix: Matrix4, rotation: Matrix4) =
        vao.Bind()
        GL.UseProgram(programHandle)

        let mutable mvpMatrix =
            rotation
            * Matrix4.CreateTranslation(0f, 0f, -40f)
            * projectionMatrix
        GL.UniformMatrix4(mvpMatrixUniformLocation, false, &mvpMatrix)

        GL.Clear(ClearBufferMask.DepthBufferBit)
        vao.DrawArrays(PrimitiveType.Lines, 0, 2 * 3)

        GL.UseProgram(0)
        GL.BindVertexArray(0)
