namespace MinecraftDotNet.Core

open System


type IRenderable =
    interface
        
    end

type IRenderer<'TRenderable when 'TRenderable :> IRenderable> =
    abstract member Render: 'TRenderable -> unit



// --------------------------------
// Items
// --------------------------------




type ItemId = ItemId of int * string

type ItemInfo(id) =
    member this.Id: ItemId = id

type ItemMeta = ItemMeta of string

type Item(info, meta) =
    member this.Info: ItemInfo = info
    member this.Meta: ItemMeta = meta

type ItemStack(itemInfo, meta, count) =
    inherit Item(itemInfo, meta)
    member this.Count: int = count
    
type Inventory(items) =
    member this.Items: Item array = items

type Player(name, inventory) =
    member _.Name: string = name
    member this.Inventory: Inventory = inventory
