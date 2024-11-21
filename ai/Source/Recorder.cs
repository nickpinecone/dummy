using System.IO;
using System.Threading.Tasks;

namespace Kin.Recorder;

public class Recorder
{
    public Recorder()
    {
    }

    public async Task Record(string name)
    {
        var output = Path.Combine(Directory.GetCurrentDirectory(), "Output", "mic.wav");
        var command = new Bash.Bash($"ffmpeg -f pulse -i default {output}");

        await command.Start();
        await command.Process.WaitForExitAsync();
    }
}
