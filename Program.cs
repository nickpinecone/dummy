using System;
using System.ClientModel;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using OllamaSharp;
using OpenAI.Audio;

namespace Dummy;

public enum State
{
    Wait,
    Record,
    Generate,
    Ready,
    Speak,
    Pause,
}

public static class Program
{
    private static AudioManager _audioManager = new AudioManager();
    private static OllamaApiClient _aiManager = AIManager.CreateOllama();
    private static UIManager _uiManager = new UIManager();

    private static State _state = State.Wait;
    private static string _answer = "";

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
        window.Content = _uiManager.Stack;

        _uiManager.SetState(_state);

        window.Show();
        app.Run(window);
    }

    private static async void HandleKeyUp(object? sender, KeyEventArgs e)
    {
        if (_state == State.Speak && e.KeySymbol == "q")
        {
            _state = State.Wait;
            _uiManager.SetState(_state, "");
            _audioManager.Player.Stop();
        }

        if (e.KeySymbol != " ")
        {
            return;
        }

        if (_state == State.Wait)
        {
            _state = State.Record;
            _uiManager.SetState(_state, "");
            Record();
        }

        else if (_state == State.Record)
        {
            _state = State.Generate;
            _uiManager.SetState(_state);
            await Generate();
        }

        else if (_state == State.Ready)
        {
            _state = State.Speak;
            _uiManager.SetState(_state, _answer);
            await Speak();
            await _audioManager.Player.Wait();

            _state = State.Wait;
            _uiManager.SetState(_state, "");
        }

        else if (_state == State.Speak)
        {
            _state = State.Pause;
            _uiManager.SetState(_state);
            await Pause();
        }

        else if (_state == State.Pause)
        {
            _state = State.Speak;
            _uiManager.SetState(_state);
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

        ClientResult<AudioTranscription>? text = null;
        try
        {
            text = await _audioManager.SpeechToText("mic.wav");
        }
        catch (Exception error)
        {
            _state = State.Wait;
            _uiManager.SetState(_state, "Я тебя не расслышал");
            Console.WriteLine(error);
            return;
        }

        var answer = new StringBuilder();

        try
        {
            await foreach (var stream in _aiManager.GenerateAsync(text.Value.Text))
            {
                answer.Append(stream?.Response);
            }
        }
        catch (Exception error)
        {
            _state = State.Wait;
            _uiManager.SetState(_state, "Произошла какая-то ошибка");
            Console.WriteLine(error);
            return;
        }

        Console.WriteLine("[Ollama] " + answer.ToString());

        await _audioManager.TextToSpeech(answer.ToString(), "out.wav");
        _answer = answer.ToString();

        Ready();
    }

    public static void Ready()
    {
        _state = State.Ready;
        _uiManager.SetState(_state);
    }

    public static async Task Speak()
    {
        if (_audioManager.Player.Paused)
        {
            await _audioManager.Player.Resume();
        }
        else
        {
            _audioManager.PlayFile("out.wav");
            await _audioManager.Player.Wait();
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
}
