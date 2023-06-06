## Avalonia.MVU

Avalonia.MVU is a Model-View-Update (MVU) library for Avalonia.

This library works for both C# and F# and is best used with [NXUI] and probably plain avalonia code not markup.

## Existing Alternatives

Frankly speaking, there are already some MVU libraries for Avalonia and they are better for you to try out first.

- [Avalonia.FuncUI] - A true MVU plus react-like library for Avalonia. If you have a hint of react/elm then this is for you.
- [Fabulous.Avalonia] - Fabulous supports a wide range of backends not just Avalonia but they are MVU first.

## Call For Help!

Do you truly want to see MVU for C# in Avalonia? please help me out!

The current code is in [The Library.fs File](./src/Avalonia.Mvu/Library.fs) and you can look after `UseElmishView` function. If you can help me out to make it work then we're good to go full MVU in C# not just F#.

### Work In Progress

This library is still work in progress and is not yet ready for production use.

While library mentions MVU, it is not a strict MVU implementation (yet I hope).

The primary intended use case for this library is to allow MVU-like updates in Avalonia but using observables to update the UI.

Here's an example:

```csharp

public static class MainViewModule
{
  private sealed record State(int Count = 0);

  private abstract record Message
  {
    public sealed record Increment : Message;

    public sealed record Decrement : Message;
  }


  public static Control Build(Window window)
  {
    var (state, dispatch) =
      UseElmish<State, Message>(model: new State(), update: (message, state) =>
      {
        return message switch
        {
          Message.Increment =>
            (state with { Count = state.Count + 1 }, Cmd.ofMsg<Message>(new Message.SetName("Increment"))),
          Message.Decrement =>
            (state with { Count = state.Count - 1 }, Cmd.ofMsg<Message>(new Message.SetName("Decrement"))),
          _ => (state, Cmd.none<Message>())
        };
      });

    var counterText =
      state
        .Select(state => state.Count)
        .DistinctUntilChanged()
        .Select(count => $"You clicked {count} times");


    void IncrementOnClick(Button sender, IObservable<RoutedEventArgs> e) =>
      e.Subscribe(_ => dispatch(new Message.Increment()));

    void DecrementOnClick(Button sender, IObservable<RoutedEventArgs> e) =>
      e.Subscribe(_ => dispatch(new Message.Decrement()));

    return StackPanel()
      .Spacing(4.0)
      .Children(
        Button().Content("Increment").OnClick(IncrementOnClick),
        TextBlock().Text(counterText, BindingMode.OneWay),
        Button().Content("Decrement").OnClick(IncrementOnClick),
      );
  }
}
```

This is more of a MU than MVU where the model and update part is MVU but the view part is not. I personally think this is kind of the best of both worlds as you're not forced to fall within the MVU paradigm for the view part so you can freely interop with many of avalonia controls, even those made from thir party developers, you're still in control of a predictable state -> update -> view control flow using observables and making them bindings for your avalonia code, this is also performant as we're not re-rendering the whole view on partial state updates (e.g. update the text property in the model would mean to re-render the whole view or diff to patch the view) but rather controlling the view updates using observables.

---

#### For those who want full MVU:

I understand this may not be the MVU some people are looking after but doing the View part has it's own challenges, and that's why the MVU alternatives are front and center at the top of this readme, unfortunately for the C# devs those alternatives provided are F# ones I personally don't think that should be a show stopper as both are .NET langauges and frankly F# is a better suited language for an MVU paradigm, but I do understand that some people may not want to learn F# and that's fine, I'm not here to force you to learn F# but I do want to help you out to get MVU in C#.

However I did try to make it happen, ideally we want to also include the View part here so that it is a complete example and you can see that in [The Counter Module](./src/samples/CSharp/CounterModule.cs) but if you have ideas to be able to make it work let's make them a reality!.
