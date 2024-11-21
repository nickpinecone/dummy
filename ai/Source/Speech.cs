using System.IO;
using System.Threading.Tasks;

namespace Kin.Speech;

public class Speech
{
    public Speech()
    {
    }

    public async Task<string> Transcribe(string name)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Output", name);
        var output = Path.Combine(Directory.GetCurrentDirectory(), "Output", "text.txt");
        var command = new Bash.Bash($"vosk-transcriber -l ru -i {path} -t txt -o {output}");

        await command.Start();
        await command.Process.WaitForExitAsync();

        var reader = new Bash.Bash($"cat {output}");
        await reader.Start();
        var result = reader.Process.StandardOutput.ReadToEnd();

        return result;
    }

    public async Task Synthesize(string text)
    {
        var command = new Bash.Bash(
            $"echo '{text}' | ./piper/piper --model ./piper/kin.onnx --output-raw | aplay -r 22050 -f S16_LE -t raw -");

        await command.Start();
        await command.Process.WaitForExitAsync();
    }
}
