using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using OllamaSharp;

namespace Dummy;

public static class Program
{
    private static Label _status = new Label()
    {
        FontSize = 32,
        Padding = new Thickness(16),
    };

    private static TextBlock _answer = new TextBlock()
    {
        FontSize = 24,
        TextWrapping = TextWrapping.Wrap,
        Padding = new Thickness(16),
    };

    private static Image _image = new Image();

    private static AudioManager _audioManager = new AudioManager();
    private static OllamaApiClient _aiManager = AIManager.CreateOllama();

    private static string _state = "wait";
    private static string _answerText = "";

    public static void Main(string[] args)
    {
        AppBuilder.Configure<Application>().UsePlatformDetect().Start(AppMain, args);
    }

    public static void AppMain(Application app, string[] args)
    {
        app.Styles.Add(new Avalonia.Themes.Simple.SimpleTheme());
        app.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;

        var window = new Window();
        window.KeyUp += HandleKeyUp;

        _image = new Image()
        {
            Stretch = Stretch.Uniform,
            Width = 600,
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        _image.SetValue(Canvas.BottomProperty, window.Height);

        var stack = new StackPanel()
        {
            Width = window.Width,
            Height = window.Height,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        stack.Children.Add(_status);
        stack.Children.Add(_answer);
        stack.Children.Add(_image);

        window.Content = stack;

        Wait();
        window.Show();
        app.Run(window);
    }

    private static async void HandleKeyUp(object? sender, KeyEventArgs e)
    {
        if (_state == "wait")
        {
            _state = "record";
            _status.Content = "Слушает";
            var bitmap = new Bitmap("Static/record.jpg");
            _image.Source = bitmap;
            Record();
        }
        else if (_state == "record")
        {
            _state = "generate";
            _status.Content = "Думает...";
            var bitmap = new Bitmap("Static/generate.jpg");
            _image.Source = bitmap;
            await Generate();
        }
        else if (_state == "ready")
        {
            _state = "speak";
            _status.Content = "Говорит";
            var bitmap = new Bitmap("Static/speak.jpg");
            _image.Source = bitmap;
            await Speak();
        }

        else if (_state == "speak")
        {
            _state = "pause";
            await Pause();
        }
        else if (_state == "pause")
        {
            _state = "speak";
            await Speak();
        }
    }

    public static void Record()
    {
        _audioManager.RecordFile("mic.wav");
    }

    public static async Task Generate()
    {
        _audioManager.Recorder.Stop();

        var text = await _audioManager.SpeechToText("mic.wav");

        var answer = new StringBuilder();
        await foreach (var stream in _aiManager.GenerateAsync(text.Value.Text))
        {
            answer.Append(stream?.Response);
        }

        await _audioManager.TextToSpeech(answer.ToString(), "out.wav");
        _answerText = answer.ToString();

        Ready();
    }

    public static void Ready()
    {
        _state = "ready";
        _status.Content = "Готов!";
        var bitmap = new Bitmap("Static/ready.jpg");
        _image.Source = bitmap;
    }

    public static async Task Speak()
    {
        if (_audioManager.Player.Paused)
        {
            await _audioManager.Player.Resume();
        }
        else
        {
            _answer.Text = _answerText;
            _audioManager.PlayFile("out.wav");
            await _audioManager.Player.Wait();
            Wait();
        }
    }

    public static async Task Pause()
    {
        await _audioManager.Player.Pause();
    }

    public static void Stop()
    {
        _audioManager.Player.Stop();
    }

    private static void Wait()
    {
        _state = "wait";
        _status.Content = "Ждет вопроса";
        _answer.Text = "";
        Bitmap bitmap = new Bitmap("Static/wait.jpg");
        _image.Source = bitmap;
    }
}
