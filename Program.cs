using Avalonia;
using Avalonia.Controls;

namespace Dummy;

public static class Program
{
    public static void Main(string[] args)
    {
        AppBuilder.Configure<Application>().UsePlatformDetect().Start(AppMain, args);
    }

    public static void AppMain(Application app, string[] args)
    {
        app.Styles.Add(new Avalonia.Themes.Simple.SimpleTheme());
        app.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Default;

        var window = new Window();
        window.Title = "Dummy";
        window.Width = 800;
        window.Height = 600;

        var text = new Label();
        window.Content = text;

        text.Content = "Hello";
        text.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        text.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        text.FontSize = 72;

        window.Show();
        app.Run(window);
    }
}
