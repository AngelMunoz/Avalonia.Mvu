using Elmish;
using static Avalonia.Mvu.AvaloniaElmish;

StackPanel PanelContent(Window window)
{
  var (counter, dispatch) =
    UseElmish<State, Message>(model: new State(Count: 0), update: (message, state) =>
    {
      return message switch
      {
        Message.Increment => (state with { Count = state.Count + 1 }, Cmd.none<Message>()),
        Message.Decrement => (state with { Count = state.Count - 1 }, Cmd.none<Message>()),
        _ => (state with { Count = 0 }, Cmd.none<Message>())
      };
    });

  var counterText =
    counter.Select(state => $"You clicked {state.Count} times");

  return StackPanel()
    .Children(
      Button().Content("Click me!").OnClick((_, observable) =>
      {
        observable.Subscribe(_ => dispatch(new Message.Increment()));
      }),
      TextBox().Text(window.BindTitle()),
      TextBlock().Text(counterText, BindingMode.OneWay)
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


public record State(int Count = 0);

public abstract class Message
{
  public sealed class Increment : Message
  {
  }

  public sealed class Decrement : Message
  {
  }

  public sealed class Reset : Message
  {
  }
}
