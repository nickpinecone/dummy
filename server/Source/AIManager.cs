using System;
using OllamaSharp;

namespace Dumb;

public class AIManager
{
    public static OllamaApiClient CreateOllama()
    {
        var uri = new Uri("http://localhost:11434");
        var ollama = new OllamaApiClient(uri);
        ollama.SelectedModel = "bambucha/saiga-llama3";

        return ollama;
    }
}
