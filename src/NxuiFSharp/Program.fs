open Avalonia
open Avalonia.Data
open Avalonia.Controls

open NXUI.Extensions
open NXUI.FSharp.Extensions

open NxUIFSharp


let panelContent (window: Window) =
    let mutable count = 0
    let button = Button().content ("NXUI + F#!")
    let textbox = TextBox().text (window.BindTitle())
    let counterText =
        button.ObserveOnClick()
        |> Observable.tap (fun _ -> count <- count + 1)
        |> Observable.map (fun _ -> $"You clicked %i{count} times")

    let label =
        TextBox()
            .text(counterText, BindingMode.OneWay)

    StackPanel().children (button, textbox, label)

let view () : Window =
    let window = 
        Window()
            .title("NXUI and F#")
            .width(300)
            .height(300)

    window
        .content (panelContent window)

[<EntryPoint>]
let main argv =
    AppBuilder
        .Configure<Application>()
        .UsePlatformDetect()
        .UseFluentTheme(Styling.ThemeVariant.Dark)
        .WithApplicationName("NXUI and F#")
        .StartWithClassicDesktopLifetime(view, argv)