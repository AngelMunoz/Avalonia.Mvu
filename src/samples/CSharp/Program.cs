using Elmish;
using static Avalonia.Mvu.AvaloniaElmish;
using static Avalonia.Mvu.CSharp;


StackPanel PanelContent(Window window)
{
  var (counter, dispatch) =
    UseElmish<State, Message>(model: new State(Count: 0), update: (message, state) =>
    {
      switch (message)
      {
        case Message.Increment:
          return (state with { Count = state.Count + 1 }, Cmd.ofMsg<Message>(new Message.SetName("Increment")));

        case Message.Decrement:
          return (state with { Count = state.Count - 1 }, Cmd.ofMsg<Message>(new Message.SetName("Decrement")));

        case Message.Reset:
          return (state with { Count = 0 }, Cmd.ofMsg<Message>(new Message.SetName("Reset")));

        case Message.NameFound(bool nameFound):
          return (state with { NameFound = nameFound }, Cmd.none<Message>());

        case Message.SetName(string name):
          var wasNameFound = (string name) =>
            name.Equals("peter", StringComparison.InvariantCultureIgnoreCase);

          Func<bool, Message> onSuccess = (bool nameFound) => new Message.NameFound(nameFound);

          var cmd = Cmdcs.OfFunc.perform(wasNameFound, name, onSuccess);

          return (state with { Name = name }, cmd);

        default:
          return (state, Cmd.none<Message>());
      }
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

  var incrementOnClick = (Button sender, IObservable<RoutedEventArgs> e) =>
    {
      e.Subscribe(_ => dispatch(new Message.Increment()));
    };

  var checkText = (TextBox sender, IObservable<string> e) =>
    {
      e
      .Throttle(TimeSpan.FromMilliseconds(250))
      .Subscribe(name => dispatch(new Message.SetName(name)));
    };

  return StackPanel()
    .Children(
      Button().Content("Click me!").OnClick(incrementOnClick),
      TextBox().Text(window.BindTitle()),
      TextBlock().Text(counterText, BindingMode.OneWay),
      TextBlock().Text(actionPerformed, BindingMode.OneWay),
      StackPanel()
        .Spacing(4.0)
        .Children(
          Label().Content("Find the name!"),
          TextBox().Watermark("Starts with P ends with R").OnText(checkText)
        ),
      CheckBox()
        .IsChecked(nameFound.ToBinding(), BindingMode.OneWay)
        .IsEnabled(false)
        .Content("Name Found"),
      StackPanel()
        .Margin(new Thickness(10))
        .Children(
          Label().Content("Elmish Counter"),
          ElmishCounter.Build()
        )
    );
}

Window View() =>
  Window(out Window window)
    .Title("NXUI + Avalonia.MVU")
    .Width(300)
    .Height(300)
    .Content(PanelContent(window));


AppBuilder
  .Configure<Application>()
  .UsePlatformDetect()
  .UseFluentTheme(ThemeVariant.Dark)
  .WithApplicationName("NXUI + Avalonia.MVU")
  .StartWithClassicDesktopLifetime(View, args);


public record State(int Count = 0, string Name = "", bool NameFound = false);

public abstract record Message
{
  public sealed record Increment : Message;

  public sealed record Decrement : Message;

  public sealed record Reset : Message;

  public sealed record NameFound(bool Found) : Message;

  public sealed record SetName(string Name) : Message;
}

public static class ElmishCounter
{
  public record ElmishModel(int Count = 0);

  public abstract record ElmishMessage
  {
    public sealed record Increment : ElmishMessage;
    public sealed record Decrement : ElmishMessage;
  }


  public static Control Build() =>
    UseElmishView<ElmishModel, ElmishMessage>(
      new ElmishModel(Count: 10),
      (message, model) =>
        message switch
          {
            ElmishMessage.Increment =>
              (model with { Count = model.Count + 1 }, Cmd.none<ElmishMessage>()),
            ElmishMessage.Decrement =>
              (model with { Count = model.Count - 1 }, Cmd.none<ElmishMessage>()),
            _ => (model, Cmd.none<ElmishMessage>()),
          },
      (model, dispatch) =>
        StackPanel()
        .Spacing(4.0)
        .Children(
          Button()
            .Content("Increment")
            .OnClick((_, o) =>
              o.Subscribe(_ => dispatch(new ElmishMessage.Increment()))),
          TextBlock().Text($"You clicked {model.Count} times"),
          Button()
            .Content("Decrement")
            .OnClick((_, o) =>
              o.Subscribe(_ => dispatch(new ElmishMessage.Decrement())))
        )
    );

}
