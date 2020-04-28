module MinecraftDotNet.ClientSide.Graphics.Core.Camera

open MinecraftDotNet.Core.Math

type Camera =
    { ViewMatrix: Matrix4x4f
      ProjectionMatrix: Matrix4x4f }

module Camera =
    
    let create view projection =
        { ViewMatrix = view
          ProjectionMatrix = projection }
    
    let pos camera =
        Matrix4x4.translation camera.ViewMatrix
    
    let move translation camera =
        { camera with
            ViewMatrix = camera.ViewMatrix * Matrix4x4.createTranslation translation }
    
    let setProjection projection camera =
        { camera with
            ProjectionMatrix = projection }
    
    let lookAt target camera =
        { camera with
            ViewMatrix = Matrix4x4.createLookAt (pos camera) target (Vector3.create 0G 1G 0G) }
    
//    let rotateX radians camera =
//        { camera with ViewMatrix = camera.ViewMatrix * Matrix4x4.CreateRotationX radians }
//    let rotateY radians camera =
//        { camera with ViewMatrix = camera.ViewMatrix * Matrix4x4.CreateRotationY radians }
//    let rotateZ radians camera =
//        { camera with ViewMatrix = camera.ViewMatrix * Matrix4x4.CreateRotationZ radians }
//    let rotate (rotation: Vector3d) camera =
//        camera |> (rotateX rotation.X >> rotateY rotation.Y >> rotateZ rotation.Z)
    