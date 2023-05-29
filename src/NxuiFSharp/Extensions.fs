namespace NxUIFSharp

open System
open Avalonia.Controls
open Avalonia.Layout

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