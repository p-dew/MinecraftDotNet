namespace Ehingeeinae.Examples.SpaceInvaders.Systems

open System.Collections.Generic

open OpenTK.Windowing.Desktop
open OpenTK.Windowing.GraphicsLibraryFramework

open Ehingeeinae.Ecs.Resources
open Ehingeeinae.Ecs.Systems


type InputState =
    { KeyboardState: KeyboardState
      MouseState: MouseState
      JoystickStates: IReadOnlyList<JoystickState> }

type InputSystem(windowResource: IEcsSharedResource<NativeWindow>, inputStateResource: IEcsUniqueResource<InputState>) =
    interface IEcsSystem with
        member this.Update(ctx) =
            let window = windowResource.Value
            inputStateResource.Value <-
                { KeyboardState = window.KeyboardState
                  MouseState = window.MouseState
                  JoystickStates = window.JoystickStates }
