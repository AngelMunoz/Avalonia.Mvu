namespace Avalonia.Mvu

open System
open Elmish
open System.Reactive.Subjects

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


module Cmd =
  let Empty<'a> = Cmd.Empty
