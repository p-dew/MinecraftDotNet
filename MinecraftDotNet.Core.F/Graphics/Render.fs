namespace MinecraftDotNet.Core.Graphics.Render

type IRenderable = interface end

type Renderer<'TRenderable when 'TRenderable :> IRenderable> = 'TRenderable -> unit
