using System;
using System.Text;
using System.Threading.Tasks;
using OllamaSharp;

namespace Dumb;

public static class Program
{
    public async static Task Main()
    {
        // var recorder = new Recorder.Recorder();
        var speech = new Speech();

        var uri = new Uri("http://localhost:11434");
        var ollama = new OllamaApiClient(uri);
        ollama.SelectedModel = "kin";

        // await recorder.Record("mic.wav");
        var prompt = await speech.Transcribe("mic.wav");

        StringBuilder answer = new StringBuilder();

        await foreach (var stream in ollama.GenerateAsync(prompt))
        {
            Console.Clear();
            Console.WriteLine(answer.ToString());
            answer.Append(stream.Response);
        }

        await speech.Synthesize(answer.ToString());
    }
}
