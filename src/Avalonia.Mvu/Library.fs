﻿namespace Avalonia.Mvu

open System
open Elmish
open System.Reactive.Subjects
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Templates
open Avalonia.Data

module CSharp =

  module Cmdcs =
    let inline map (f: Func<'Value, 'Message>, cmd: Cmd<'Value>) =
      Cmd.map (FuncConvert.FromFunc f) cmd

    module OfFunc =
      let either
        (
          fn: Func<'Arg, 'Return>,
          arg: 'Arg,
          ofSuccess: Func<'Return, 'Message>,
          ofError: Func<exn, 'Message>
        ) : Cmd<'Message> =
        let fn = FuncConvert.FromFunc fn
        let onSuccess = FuncConvert.FromFunc ofSuccess
        let onError = FuncConvert.FromFunc ofError
        Cmd.OfFunc.either fn arg onSuccess onError

      let perform
        (
          fn: Func<'Arg, 'Return>,
          arg: 'Arg,
          ofSuccess: Func<'Return, 'Message>
        ) =
        let fn = FuncConvert.FromFunc fn
        let onSuccess = FuncConvert.FromFunc ofSuccess
        Cmd.OfFunc.perform fn arg onSuccess

      let attempt (fn: Action<'Arg>, arg: 'Arg, ofError: Func<exn, 'Message>) =
        let fn = FuncConvert.FromAction fn
        let onError = FuncConvert.FromFunc ofError
        Cmd.OfFunc.attempt fn arg onError

    module OfTask =
      open System.Threading.Tasks

      let either
        (
          fn: Func<'Arg, Task<'Return>>,
          arg: 'Arg,
          ofSuccess: Func<'Return, 'Message>,
          ofError: Func<exn, 'Message>
        ) =
        let fn = FuncConvert.FromFunc fn
        let onSuccess = FuncConvert.FromFunc ofSuccess
        let onError = FuncConvert.FromFunc ofError
        Cmd.OfTask.either fn arg onSuccess onError

      let perform
        (
          fn: Func<'Arg, Task<'Return>>,
          arg: 'Arg,
          ofSuccess: Func<'Return, 'Message>
        ) =
        let fn = FuncConvert.FromFunc fn
        let onSuccess = FuncConvert.FromFunc ofSuccess
        Cmd.OfTask.perform fn arg onSuccess

      let attempt
        (
          fn: Func<'Arg, #Task>,
          arg: 'Arg,
          ofError: Func<exn, 'Message>
        ) =
        let fn = FuncConvert.FromFunc fn
        let onError = FuncConvert.FromFunc ofError
        Cmd.OfTask.attempt fn arg onError

module internal AvaloniaElmish =

  open Avalonia.Threading

  // Syncs view changes from non-UI threads through the Avalonia dispatcher.
  let syncDispatch (dispatch: Dispatch<'msg>) (msg: 'msg) =
    if
      Dispatcher.UIThread.CheckAccess() // Is this already on the UI thread?
    then
      dispatch msg
    else
      Dispatcher.UIThread.Post(fun () -> dispatch msg)

/// Starts an Elmish loop and provides a Dispatch method that calls the given setModel fn.
type internal ElmishState<'model, 'msg, 'arg>
  (mkProgram: unit -> Program<'arg, 'model, 'msg, unit>, arg: 'arg, setModel) =
  let program = mkProgram()

  let mutable _model = Program.init program arg |> fst
  let mutable _dispatch = fun (_: 'msg) -> ()

  let setState model dispatch =
    _dispatch <- dispatch

    if not(obj.ReferenceEquals(model, _model)) then
      _model <- model
      setModel model

  do
    program
    |> Program.withSetState setState
    |> Program.runWithDispatch AvaloniaElmish.syncDispatch arg

  member _.Model = _model
  member _.Dispatch = _dispatch

type AvaloniaElmish =

  static member UseElmish
    (
      model: 'Model,
      update: Func<'Msg, 'Model, struct ('Model * Cmd<'Msg>)>
    ) : ValueTuple<IObservable<'Model>, Action<'Msg>> =

    let ignoreView = (fun _ _ -> ())

    let mkProgram () =
      let init () = model, Cmd.none

      let update =
        let inline update msg model =
          let struct (model, msg) = update.Invoke(msg, model)
          (model, msg)

        update

      Program.mkProgram init update ignoreView

    let _model = new BehaviorSubject<'Model>(model)

    let state = ElmishState(mkProgram, (), _model.OnNext)

    let dispatch map = state.Dispatch map

    struct (_model :> IObservable<'Model>, Action<_>(dispatch))

  [<CompiledName "UseElmish">]
  static member useElmish
    (
      model: 'Model,
      update: 'Msg -> 'Model -> 'Model * Cmd<'Msg>
    ) : IObservable<'Model> * Dispatch<'Msg> =
    let ignoreView = (fun _ _ -> ())

    let mkProgram () =
      let init () = model, Cmd.none

      Program.mkProgram init update ignoreView

    let _model = new BehaviorSubject<'Model>(model)

    let state = ElmishState(mkProgram, (), _model.OnNext)

    let dispatch map = state.Dispatch map

    _model :> IObservable<'Model>, dispatch

  static member UseElmishView
    (
      model: 'Model,
      update: Func<'Msg, 'Model, struct ('Model * Cmd<'Msg>)>,
      view: Func<'Model, Action<'Msg>, Control>
    ) =

    let struct (model, dispatch) = AvaloniaElmish.UseElmish(model, update)

    let content = ContentControl(
      ContentTemplate = FuncDataTemplate<'Model>(fun model _ -> view.Invoke(model, dispatch))
    )

    let descriptor =
      ContentControl.ContentProperty
        .Bind()
        .WithMode(BindingMode.OneWay)

    content[descriptor] <- model.ToBinding()
    content

  [<CompiledName "UseElmishView">]

  static member useElmishView
    (
      model: 'Model,
      update: 'Msg -> 'Model -> 'Model * Cmd<'Msg>,
      view: 'Model -> ('Msg -> unit) -> Control
    ) =
    let model, dispatch = AvaloniaElmish.useElmish(model, update)

    let content = ContentControl(
      ContentTemplate = FuncDataTemplate<'Model>(fun model _ -> view model dispatch)
    )

    let descriptor =
      ContentControl
        .ContentProperty
        .Bind()
        .WithMode(BindingMode.OneWay)

    content[descriptor] <- model.ToBinding()

    content
