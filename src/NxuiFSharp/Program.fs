open Avalonia
open NXUI.Extensions
open NxUIFSharp

AppBuilder
    .Configure<Application>()
    .UsePlatformDetect()
    .UseFluentTheme()
    .WithApplicationName("NXUI and F#")
    .StartWithClassicDesktopLifetime(Counter.view, [||])
|> ignore