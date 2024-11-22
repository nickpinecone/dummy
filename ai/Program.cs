using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;
using System.Threading.Tasks;
using OllamaSharp;
using Microsoft.AspNetCore.SignalR;

namespace Dumb;

public static class Program
{
    static IHubContext<SignalHub> _hub = null;

    public async static Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCors(
            options =>
            {
                options.AddDefaultPolicy(
                    policy =>
                    {
                        policy.WithOrigins(new string[] { "http://localhost:8080" }).AllowAnyHeader().AllowAnyMethod();
                    });
            });

        builder.Services.AddSignalR();

        var app = builder.Build();

        app.UseCors();
        app.MapHub<SignalHub>("/signal");
        _hub = app.Services.GetService<IHubContext<SignalHub>>();

        var task = app.RunAsync();

        var recorder = new Recorder();
        var speech = new Speech();

        var uri = new Uri("http://localhost:11434");
        var ollama = new OllamaApiClient(uri);
        ollama.SelectedModel = "kin";

        while (true)
        {
            Console.WriteLine("Press any key to start recording");
            Console.ReadKey();

            await _hub.Clients.All.SendAsync("Recording");
            await recorder.Record("mic.wav");
            var prompt = await speech.Transcribe("mic.wav");

            await _hub.Clients.All.SendAsync("Thinking");
            StringBuilder answer = new StringBuilder();

            Console.WriteLine("[Ollama] Generating answer...");
            await foreach (var stream in ollama.GenerateAsync(prompt))
            {
                Console.Write("#");
                answer.Append(stream.Response);
            }

            await _hub.Clients.All.SendAsync("Ready");
            Console.WriteLine("Press any key to hear the answer");
            Console.ReadKey();

            await _hub.Clients.All.SendAsync("Speaking", answer.ToString());
            await speech.Synthesize(answer.ToString());
            await _hub.Clients.All.SendAsync("Done", answer.ToString());
        }
    }

    public class SignalHub : Hub
    {
    }
}
