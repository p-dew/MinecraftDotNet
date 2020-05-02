module MinecraftDotNet.Core.Entities

open System
open MinecraftDotNet.Core.Math.Linear

type EntityId = EntityId of Guid

type Position = Vector3<float>

type Entity =
    { Id: EntityId
      Health: int
      Position: Position }


// ------------------------------------------



