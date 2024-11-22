using System.Diagnostics;
using System.Threading.Tasks;

namespace Dumb;

public class Bash
{
    private Process _process = null;
    private string _command = "";

    public Process Process => _process;
    private string Command => _command;

    public Bash(string command)
    {
        _command = command;

        CreateProcess();
    }

    private void CreateProcess()
    {
        var escaped = _command.Replace("\"", "\\\"");

        _process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{escaped}\"",
                RedirectStandardOutput = true,
                RedirectStandardInput = false,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
    }

    public Task Start()
    {
        if (_process == null)
        {
            CreateProcess();
        }

        _process.Start();

        return Task.CompletedTask;
    }

    public Task Stop()
    {
        if (_process != null)
        {
            _process.Kill();
            _process.Dispose();
            _process = null;
        }

        return Task.CompletedTask;
    }
}
