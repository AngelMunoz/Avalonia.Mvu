open System

open Avalonia
open Avalonia.Data
open Avalonia.Controls

open NXUI.Extensions
open NXUI.FSharp.Extensions

open Elmish
open type Mvu.AvaloniaElmish

open FSharp.Control.Reactive

type State = {
  count: int
  name: string
  nameFound: bool
}

type Message =
  | Increment
  | Decrement
  | Reset
  | NameFound of bool
  | SetName of string

let panelContent (window: Window) : StackPanel =

  let counter, dispatch =
    useElmish(
      {
        count = 0
        name = ""
        nameFound = false
      },
      fun msg state ->
        match msg with
        | Increment ->
          { state with count = state.count + 1 }, Cmd.ofMsg(SetName "Increment")
        | Decrement ->
          { state with count = state.count - 1 }, Cmd.ofMsg(SetName "Decrement")
        | Reset -> { state with count = 0 }, Cmd.ofMsg(SetName "Reset")
        | NameFound found -> { state with nameFound = found }, Cmd.none
        | SetName name ->
          let wasNameFound (name: string) =
            name.Equals("peter", StringComparison.InvariantCultureIgnoreCase)

          let ofSuccess (found: bool) = NameFound found

          { state with name = name },
          Cmd.OfFunc.perform wasNameFound name ofSuccess
    )

  let counterText =
    counter
    |> Observable.map(fun ({ count = count }) -> count)
    |> Observable.distinctUntilChanged
    |> Observable.map(fun count -> $"You clicked %i{count} times")

  let nameFound =
    counter
    |> Observable.map(fun ({ nameFound = nameFound }) -> nameFound)
    |> Observable.distinctUntilChanged

  let actionPerformed =
    counter
    |> Observable.map(fun ({ name = name }) -> name)
    |> Observable.distinctUntilChanged
    |> Observable.map(fun name -> $"Action Performed: {name}")

  let incrementOnClick _ observable =
    observable |> Observable.add(fun _ -> dispatch Increment)

  let checkText _ observable =
    observable
    |> Observable.throttle(TimeSpan.FromMilliseconds(250))
    |> Observable.add(fun name -> dispatch(SetName name))

  StackPanel()
    .children(
      Button().content("Click me!!").OnClick(incrementOnClick),
      TextBox().text(window.BindTitle()),
      TextBlock().text(counterText, BindingMode.OneWay),
      TextBlock().text(actionPerformed, BindingMode.OneWay),
      StackPanel()
        .spacing(4.0)
        .children(
          Label().content("Find the name!"),
          TextBox().watermark("Starts with P ends with R").OnText(checkText)
        ),
      CheckBox()
        .isChecked(nameFound.ToBinding(), BindingMode.OneWay)
        .isEnabled(false)
        .content("Name Found")
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
