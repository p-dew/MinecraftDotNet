module MinecraftDotNet.Core.Items

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