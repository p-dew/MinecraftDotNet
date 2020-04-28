namespace MinecraftDotNet.ClientSide.Graphics.Core

open System.Collections
open System.Collections.Generic
open System.Collections.Generic
open MinecraftDotNet.Core.Math

type MeshVertex =
    { Vertex: Vector3f
      Uv: Vector2f }

type Mesh =
    { Vertices: Vector3f array
      Uvs: Vector2f array
      Elements: int array }
    interface IEnumerable<MeshVertex>

module Mesh =
    let create meshVertices elements =
        { Vertices = meshVertices |> Array.map (fun v -> v.Vertex)
          Uvs = meshVertices |> Array.map (fun v -> v.Uv)
          Elements = elements }

    let createUnsafe vertices uvs elements =
        if Array.length vertices <> Array.length elements
        then None
        else { Vertices = vertices
               Uvs = uvs
               Elements = elements } |> Some
    
    let toSeq mesh =
        mesh.Elements
        |> Seq.map (fun e ->
            { Vertex = mesh.Vertices.[e]
              Uv = mesh.Uvs.[e] })
    
//    let map mapping mesh =
//        mesh
//        |> toSeq
//        |> Seq.map mapping

#nowarn "69"

type Mesh with
    interface IEnumerable<MeshVertex> with
        member this.GetEnumerator() =
            (Mesh.toSeq this).GetEnumerator()
        member this.GetEnumerator() : IEnumerator = upcast (this :> IEnumerable<_>).GetEnumerator()