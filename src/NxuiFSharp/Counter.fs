module NxUIFSharp.Counter

open System
open Avalonia.Controls
open Avalonia.Layout

open NXUI
open type NXUI.Properties
open type NXUI.Events
open type NXUI.Builders
open NXUI.Extensions

open System.Runtime.CompilerServices

module Observable =

    let tap (f: 'Type -> unit) (observable: IObservable<'Type>) : IObservable<'Type> =
        observable
        |> Observable.map (fun args ->
            f args
            args)

/// Temporary Type Extensions with different names than properties
/// to avoid having the F# compiler complain about the ambiguity
/// between the property and the extension method.
/// See https://github.com/fsharp/fslang-suggestions/issues/1039
/// We'll see if there's something we can do on the NXUI side to avoid this.
[<Extension>]
type NxuiFsExtensions =

    [<Extension>]
    static member inline title<'Type when 'Type :> Window>(window: 'Type, title: string) : 'Type =
        WindowExtensions.Title(window, title)

    [<Extension>]
    static member inline text<'Type when 'Type :> TextBox>
        (
            textbox: 'Type,
            text: Avalonia.Data.IBinding,
            ?mode: Avalonia.Data.BindingMode,
            ?priority: Avalonia.Data.BindingPriority
        ) : 'Type =

        TextBoxExtensions.Text(textbox, text, ?mode = mode, ?priority = priority)

    [<Extension>]
    static member inline height<'Type when 'Type :> Layoutable>(layoutable: 'Type, size: float) : 'Type =
        LayoutableExtensions.Height(layoutable, size)

    [<Extension>]
    static member inline width<'Type when 'Type :> Layoutable>(layoutable: 'Type, size: float) : 'Type =
        LayoutableExtensions.Width(layoutable, size)

    [<Extension>]
    static member inline content<'Type when 'Type :> ContentControl>(control: 'Type, content: obj) : 'Type =
        ContentControlExtensions.Content(control, content)

    [<Extension>]
    static member inline content<'Type when 'Type :> ContentControl>
        (
            control: 'Type,
            observable: IObservable<obj>,
            ?mode: Avalonia.Data.BindingMode,
            ?priority: Avalonia.Data.BindingPriority
        ) : 'Type =
        ContentControlExtensions.Content(control, observable, ?mode = mode, ?priority = priority)

    [<Extension>]
    static member inline children<'Type when 'Type :> Panel>(panel: 'Type, children: Control) : 'Type =
        PanelExtensions.Children(panel, children)

    [<Extension>]
    static member inline children<'Type when 'Type :> Panel>
        (
            panel: 'Type,
            [<ParamArray>] children: Control array
        ) : 'Type =
        PanelExtensions.Children(panel, children)

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
                mode = Avalonia.Data.BindingMode.OneWay
            )

    StackPanel().children (button, textbox, label)

let view () : Window =
    let mutable window = null

    Window(&window)
        .title("NXUI From F#")
        .width(300)
        .height(300)
        .content (panelContent window)