module MinecraftDotNet.Core.Items

open MinecraftDotNet.Core.Blocks

type ItemId = ItemId of string

//type StackSize =
//    | One
//    | Many of int

type ItemInfo =
    { Id: ItemId
      MaxStackSize: int }

type InventoryItem =
    { ItemInfo: ItemInfo
      Count: int
      Meta: Meta }