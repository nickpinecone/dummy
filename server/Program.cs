using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace Dumb;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        var task = app.RunAsync();

        var audioManager = new AudioManager();
        var aiManager = AIManager.CreateOllama();

        var text = await audioManager.SpeechToText("bak.mic.wav");
        Console.WriteLine(text.Value.Text);
        await audioManager.TextToSpeech(text.Value.Text, "test.wav");
        await audioManager.PlayFile("test.wav");

        await task;
    }
}
