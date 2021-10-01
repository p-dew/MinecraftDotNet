module Ehingeeinae.Examples.SpaceInvaders.Components

[<Struct>]
type Velocity =
    { dx: float32; dy: float32 }

[<Struct>]
type Position =
    { X: float32; Y: float32 }

[<Struct>]
type Health =
    { Points: int }

type Enemy = struct end

type Player = struct end

[<Struct; RequireQualifiedAccess>]
type Owner = Enemy | Player

[<Struct>]
type Bullet =
    { Damage: int
      Owner: Owner }

[<Struct>]
type CollisionBox =
    { TODO_All: unit }

[<Struct>]
type Sprite =
    { Texture_TODO: unit }

[<Struct>]
type Text =
    { Text: string }
