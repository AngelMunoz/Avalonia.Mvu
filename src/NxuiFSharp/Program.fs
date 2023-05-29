open Avalonia
open Avalonia.Data
open Avalonia.Controls

open NXUI.Extensions
open type NXUI.Builders

open NxUIFSharp


let panelContent (window: Window) =
    let mutable count = 0
    let button = Button().content ("NXUI + F#!")
    let textbox = TextBox().text (window.BindTitle())

    let label =
        Label()
            .content (
                button.ObserveOnClick()
                |> Observable.tap (fun _ -> count <- count + 1)
                |> Observable.map (fun _ -> $"You clicked %i{count} times"),
                mode = BindingMode.OneWay
            )

    StackPanel().children (button, textbox, label)

let view () : Window =
    let mutable window = null

    Window(&window)
        .title("NXUI From F#")
        .width(300)
        .height(300)
        .content (panelContent window)

[<EntryPoint>]
let main argv =
    AppBuilder
        .Configure<Application>()
        .UsePlatformDetect()
        .UseFluentTheme(Styling.ThemeVariant.Dark)
        .WithApplicationName("NXUI and F#")
        .StartWithClassicDesktopLifetime(view, argv)