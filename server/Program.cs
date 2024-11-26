using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;

namespace Dumb;

public static class Program
{
    private static AudioManager _audioManager = new AudioManager();
    private static OllamaApiClient _aiManager = AIManager.CreateOllama();
    private static IHubContext<SignalHub> _hub = null;

    public static void Main(string[] args)
    {
        Console.WriteLine("Started!");

        var builder = WebApplication.CreateBuilder(args);

        Action<CorsOptions> corsOptions = (options) =>
        {
            Action<CorsPolicyBuilder> configurePolicy = (policy) =>
            { policy.WithOrigins("http://localhost:8090").AllowAnyHeader().AllowAnyMethod(); };

            options.AddDefaultPolicy(configurePolicy);
        };

        builder.Services.AddCors(corsOptions);
        builder.Services.AddSignalR();

        var app = builder.Build();
        app.UseCors();
        app.MapHub<SignalHub>("/signal");

        _hub = app.Services.GetService<IHubContext<SignalHub>>();
        _audioManager.Player.PlaybackFinished += HandlePlaybackFinished;

        app.MapPost("/record", Record);
        app.MapPost("/generate", Generate);
        app.MapPost("/speak", Speak);
        app.MapPost("/pause", Pause);
        app.MapPost("/stop", Stop);

        app.Run();
    }

    public static async Task Record()
    {
        await _audioManager.RecordFile("mic.wav");
    }

    public static async Task Generate()
    {
        await _audioManager.Recorder.Stop();

        var text = await _audioManager.SpeechToText("mic.wav");

        var answer = new StringBuilder();
        await foreach (var stream in _aiManager.GenerateAsync(text.Value.Text))
        {
            answer.Append(stream.Response);
        }

        await _audioManager.TextToSpeech(answer.ToString(), "out.wav");

        await _hub.Clients.All.SendAsync("Ready", answer.ToString());
    }

    public static async Task Speak()
    {
        if (_audioManager.Player.Paused)
        {
            await _audioManager.Player.Resume();
        }
        else
        {
            await _audioManager.PlayFile("out.wav");
        }
    }

    public static async Task Pause()
    {
        await _audioManager.Player.Pause();
    }

    public static async Task Stop()
    {
        await _audioManager.Player.Stop();
    }

    private static void HandlePlaybackFinished(object sender, EventArgs args)
    {
        _hub.Clients.All.SendAsync("Wait");
    }
}

public class SignalHub : Hub
{
}
