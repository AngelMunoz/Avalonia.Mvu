namespace CSharp;

using Elmish;
using static Avalonia.Mvu.CSharp;

/// <summary>
/// This static class contains an Elmish View that matches the MVU paradigm.
/// T
/// </summary>
public static class CounterModule
{
  /// <summary>
  /// The M in MVU this represents the model or also called state
  /// that contains the data of the current view and it is used to populate it.
  /// </summary>
  /// <param name="Count">Represents internal data, in this case an integer for a counter</param>
  private sealed record Model(int Count = 0, string Label = "Type Something :)");

  /// <summary>
  /// As there are no Discriminated Unions in C# we can make a record that contains
  /// our "cases" for the messages that we want to send to the update function.
  /// Each message represents an action in the view that will be scheduled to be executed in the update function.
  /// </summary>
  private abstract record Message
  {
    public sealed record Increment : Message;
    public sealed record Decrement : Message;

    public sealed record SetText(string Text) : Message;
  }

  /// <summary>
  /// This build method prepares our MVU based control to be used as part of any other
  /// Avalonia application.
  /// </summary>
  /// <returns>Our Built UI based on the current state of our MVU</returns>
  public static Control Build() =>
    UseElmishView<Model, Message>(
      // Model is the initial state of our MVU
      new Model(10),
      // Update is the function that will be called when a message is dispatched
      // and it will return a new model and a command to be executed.
      (message, model) =>
        message switch
        {
          Message.Increment =>
            (model with { Count = model.Count + 1 }, Cmd.none<Message>()),
          Message.Decrement =>
            (model with { Count = model.Count - 1 }, Cmd.none<Message>()),
          Message.SetText(var text) =>
            (model with { Label = text }, Cmd.none<Message>()),
          _ => (model, Cmd.none<Message>()),
        },
      // View is the function that will be called to build the UI based on the current state of the MVU
      // and the dispatch function that will be used to send messages to the update function.
      (model, dispatch) =>
        StackPanel()
          .Spacing(4.0)
          .Children(
            Button()
              .Content("Increment").OnClick((_, o) =>
                // we'll observe clicks on the button and dispatch an increment message
                o.Subscribe(_ => dispatch(new Message.Increment()))),
            // We'll use the current state of the model where we would normally "bind" the text
            TextBlock().Text($"You clicked {model.Count} times"),
            StackPanel()
              .Spacing(4.0)
              .Children(
                Label().Content(model.Label),
                // currently screen state (not mvu state but what's on the screen) is lost as any change in the state
                // re-renders the whole thing, if I can get it fixed I'll publish this otherwise, It was a good project
                // to experiment with :)
                TextBox()
                  .Text(model.Label)
                  .OnText((_, o) =>
                    {
                      o.Subscribe(text => dispatch(new Message.SetText(text)));
                    })
                ),
            // dynamic content is also supported
            model.Count == 20
              ? TextBlock().Text("You have reached 20!")
              : TextBlock().Text("Not reached 20 yet!"),
            Button()
              .Content("Decrement").OnClick((_, o) =>
                // we'll observe clicks on the button and dispatch an decrement message
                o.Subscribe(_ => dispatch(new Message.Decrement())))
          )
    );

}
