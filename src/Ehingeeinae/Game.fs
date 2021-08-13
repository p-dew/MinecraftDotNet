namespace Ehingeeinae

type Game = unit

type IGameHost =
    abstract Run: game: Game -> unit
