namespace MinecraftDotNet.Core

type ItemId = ItemId of string

type ItemInfo =
    {
        Id: ItemId
        StackSize: int
        
        OnLeftClicked: unit -> unit
    }


type ItemInfo with
    member this.IsStackable = this.StackSize > 1


module HardcodeItemDb =
    let defaultItem: ItemInfo =
        
        {
            Id = ItemId ""
            StackSize = 64
            OnLeftClicked = fun() -> ()
        }
    
    let appleItem: ItemInfo =
        { defaultItem with
            Id = ItemId ""
        }

type ItemStack =
    {
        Info: ItemInfo
        Count: int
        Meta: Meta
    }

type ItemStack with
    member this.Take(count: int) =
        let stack1 = { this with Count = count }
        let stack2 = { this with Count = this.Count - stack1.Count }
        stack1, stack2
    
    member this.Split() = this.Take(this.Count / 2)


type Inventory =
    {
        Items: ItemStack
    }