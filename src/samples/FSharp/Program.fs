open Avalonia
open Avalonia.Data
open Avalonia.Controls

open NXUI.Extensions
open NXUI.FSharp.Extensions

open Elmish
open type Avalonia.Mvu.AvaloniaElmish

type State = { count: int }

type Message =
  | Increment
  | Decrement
  | Reset

let panelContent (window: Window) : StackPanel =

  let counter, dispatch =
    useElmish(
      { count = 0 },
      fun msg state ->
        match msg with
        | Increment -> { state with count = state.count + 1 }, Cmd.none
        | Decrement -> { state with count = state.count - 1 }, Cmd.none
        | Reset -> { state with count = 0 }, Cmd.none
    )

  let counterText =
    counter
    |> Observable.map(fun ({ count = count }) -> $"You clicked %i{count} times")

  let incrementOnClick _ observable =
    observable |> Observable.add(fun _ -> dispatch Increment)

  StackPanel()
    .children(
      Button().content("Click me!!").OnClick(incrementOnClick),
      TextBox().text(window.BindTitle()),
      TextBlock().text(counterText, BindingMode.OneWay)
    )

let view () : Window =
  let window = Window().title("NXUI and F#").width(300).height(300)

  window.content(panelContent window)

[<EntryPoint>]
let main argv =
  AppBuilder
    .Configure<Application>()
    .UsePlatformDetect()
    .UseFluentTheme(Styling.ThemeVariant.Dark)
    .WithApplicationName("NXUI and F#")
    .StartWithClassicDesktopLifetime(view, argv)
