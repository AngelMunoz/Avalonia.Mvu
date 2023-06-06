using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace CSharp;

using Elmish;
using static Avalonia.Mvu.CSharp;

public static class MainView
{
  private sealed record State(int Count = 0, string Name = "", bool NameFound = false);

  private abstract record Message
  {
    public sealed record Increment : Message;

    public sealed record Decrement : Message;

    public sealed record Reset : Message;

    public sealed record NameFound(bool Found) : Message;

    public sealed record SetName(string Name) : Message;
  }

  /// The only thing I don't like about this are the F# command signatures. but I guess that's the price to pay for
  /// being able to use MVU in C#. I'm not sure if there's a way to make this more sensible.
  /// Maybe in .NET8 that has a broader support for Type Aliasing
  private static (State, FSharpList<FSharpFunc<FSharpFunc<Message, Unit>, Unit>> cmd) UpdateSetname(State state, string name)
  {
    var wasNameFound = (string maybeName) => maybeName.Equals("peter", StringComparison.InvariantCultureIgnoreCase);

    Func<bool, Message> onSuccess = nameFound => new Message.NameFound(nameFound);

    var cmd = Cmdcs.OfFunc.perform(wasNameFound, name, onSuccess);

    return (state with { Name = name }, cmd);
  }

  public static Control Build(Window window)
  {
    var (counter, dispatch) =
      UseElmish<State, Message>(model: new State(Count: 0), update: (message, state) =>
      {
        return message switch
        {
          Message.Increment =>
            (state with { Count = state.Count + 1 }, Cmd.ofMsg<Message>(new Message.SetName("Increment"))),
          Message.Decrement =>
            (state with { Count = state.Count - 1 }, Cmd.ofMsg<Message>(new Message.SetName("Decrement"))),
          Message.Reset =>
            (state with { Count = 0 }, Cmd.ofMsg<Message>(new Message.SetName("Reset"))),
          Message.NameFound(var nameFound) =>
            (state with { NameFound = nameFound }, Cmd.none<Message>()),
          // for cases that require more than just an expression
          // you can always fallback to a method within the module
          Message.SetName({ } name) => UpdateSetname(state, name),
          _ => (state, Cmd.none<Message>())
        };
      });

    var counterText =
      counter
        .Select(state => state.Count)
        .DistinctUntilChanged()
        .Select(count => $"You clicked {count} times");

    var nameFound =
      counter
        .Select(state => state.NameFound)
        .DistinctUntilChanged();

    var actionPerformed = counter
      .Select(state => state.Name)
      .DistinctUntilChanged()
      .Select(name => $"Action Performed: {name}");

    void IncrementOnClick(Button sender, IObservable<RoutedEventArgs> e) =>
      e.Subscribe(_ => dispatch(new Message.Increment()));

    void CheckText(TextBox sender, IObservable<string> e) =>
      e
        .Throttle(TimeSpan.FromMilliseconds(250))
        .Subscribe(name => dispatch(new Message.SetName(name)));

    return StackPanel()
      .Children(
        Button().Content("Click me!").OnClick(IncrementOnClick),
        TextBox().Text(window.BindTitle()),
        TextBlock().Text(counterText, BindingMode.OneWay),
        TextBlock().Text(actionPerformed, BindingMode.OneWay),
        StackPanel()
          .Spacing(4.0)
          .Children(
            Label().Content("Find the name!"),
            TextBox().Watermark("Starts with P ends with R").OnText(CheckText)
          ),
        CheckBox()
          .IsChecked(nameFound.ToBinding(), BindingMode.OneWay)
          .IsEnabled(false)
          .Content("Name Found"),
        StackPanel()
          .Margin(new Thickness(10))
          .Children(
            Label().Content("Elmish Counter"),
            CounterModule.Build()
          )
      );
  }
}
