namespace MinecraftDotNet.ClientSide.Graphics.Core

type ShaderType =
    | Vertex
    | Fragment

type Shader =
    | Glsl of string * ShaderType

type Material =
    { VertexShader: Shader
      FragmentShader: Shader
       }

module Material =
    let create vertShader fragShader =
        match vertShader, fragShader with
        | Glsl (_, Vertex), Glsl(_, Fragment) ->
            { VertexShader = vertShader
              FragmentShader = fragShader }
            |> Some
        | _ -> None