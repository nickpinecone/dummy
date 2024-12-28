using System;
using System.ClientModel;
using System.IO;
using System.Threading.Tasks;
using FFAudio;
using OpenAI;
using OpenAI.Audio;

namespace Dummy;

public class AudioManager
{
    public OpenAIClient OpenAI { get; private set; }
    public AudioClient Whisper { get; private set; }
    public AudioClient Piper { get; private set; }

    public FFPlayer Player { get; private set; }
    public FFRecorder Recorder { get; private set; }

    public AudioManager()
    {
        OpenAI = new OpenAIClient(new ApiKeyCredential("test"), new OpenAIClientOptions()
        {
            Endpoint = new Uri("http://localhost:8080/v1/"),
        });

        Whisper = OpenAI.GetAudioClient("whisper-small-ru");
        Piper = OpenAI.GetAudioClient("ruslan.onnx");

        Player = new FFPlayer("ffplay");
        Recorder = new FFRecorder("ffmpeg", "pulse");
    }

    private string OutputPath(string filename)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), "output", filename);
    }

    public void PlayFile(string filename)
    {
        Player.Play(OutputPath(filename));
    }

    public void RecordFile(string filename)
    {
        Recorder.Record(OutputPath(filename));
    }

    public async Task<ClientResult<AudioTranscription>> SpeechToText(string filename)
    {
        var path = OutputPath(filename);
        var text = await Whisper.TranscribeAudioAsync(path);

        return text;
    }

    public async Task TextToSpeech(string text, string filename)
    {
        var options = new SpeechGenerationOptions()
        {
            ResponseFormat = GeneratedSpeechFormat.Wav,
        };

        var data = await Piper.GenerateSpeechAsync(text, GeneratedSpeechVoice.Echo, options);

        var path = OutputPath(filename);
        BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create));
        writer.Write(data.Value.ToArray());

        writer.Close();
        await writer.DisposeAsync();
    }
}
