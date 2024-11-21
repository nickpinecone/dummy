using System.Diagnostics;

namespace Kin.Bash;

public class Bash
{
    private Process _process = null;

    public Bash(string command)
    {
        var escaped = command.Replace("\"", "\\\"");

        _process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{escaped}\"",
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
    }

    public void Start()
    {
        _process.Start();
    }
}
