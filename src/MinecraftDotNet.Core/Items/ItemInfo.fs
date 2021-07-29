namespace MinecraftDotNet.Core.Items

[<Struct>]
type ItemId = ItemId of string

type ItemInfo =
    { Id: ItemId
      MaxStack: int }
