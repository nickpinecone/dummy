using System;
using System.ClientModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using OllamaSharp;
using OpenAI;
using OpenAI.Audio;

namespace Dumb;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        var task = app.RunAsync();

        var uri = new Uri("http://localhost:11434");
        var ollama = new OllamaApiClient(uri);
        ollama.SelectedModel = "saiga-llama3";

        var openai = new OpenAIClient(new ApiKeyCredential("test"), new OpenAIClientOptions() {
            Endpoint = new Uri("http://localhost:8080/v1/"),
        });
        var whisper = openai.GetAudioClient("whisper-small-ru");

        var path = Path.Combine(Directory.GetCurrentDirectory(), "output", "bak.mic.wav");
        var text = await whisper.TranscribeAudioAsync(path);

        Console.WriteLine(text.Value.Text);

        await task;
    }
}
