using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public partial class InteractiveObjectAudio : Node3D
{
    //public static string PATH_AUDIO = "res://addons/VoxLib/Audio/";
    public static string PATH_AUDIO 
    { 
        get
        {
            return $"{VoxLib.mapManager.GetCurrentProjectFolderPath()}/Audio/";
        }
    }

    public bool isPlaying = false;
    public string currentAudioKey = "";

    private AudioStreamPlayer audioStreamPlayer2D = null;
    private AudioStreamPlayer3D audioStreamPlayer3D = null;

    private Dictionary<string, string> audios;
    private List<AudioStream> soundStreams = new List<AudioStream>();

    private InteractiveObject _interactiveObject;
    public InteractiveObject interactiveObject
    {
        get
        {
            if (_interactiveObject == null)
                _interactiveObject = GetParent().GetNode("InteractiveObject") as InteractiveObject;

            return _interactiveObject;
        }
    }

    public override void _Ready()
    {
        CheckAudioStreamPlayer3D();
    }

    public override void _Process(double delta)
    {

    }

    public void SetAudiosPathes(List<string> audioPathes)
    {
        audios = new Dictionary<string, string>();
        for (int i = 0; i < audioPathes.Count; i++)
        {
            audios.Add(Path.GetFileNameWithoutExtension(audioPathes[i]), audioPathes[i]);
        }
    }

    public string GetRandomAudioKey()
    {
        var keys = new List<string>(audios.Keys);
        Random random = new Random();
        string randomKey = keys[random.Next(keys.Count)];

        return randomKey;
    }

    public object Play3D(string soundKey, string cycle)
    {
        currentAudioKey = soundKey;

        bool isCicle = cycle.IndexOf("False") != -1 ? false : true;

        PlayCurrent3D(isCicle);
        return null;
    }

    public object PlayRandom3D(string cycle)
    {
        string randomKey = GetRandomAudioKey();

        Play3D(randomKey, cycle);

        return null;
    }

    public object PlayRandom2D(string cycle)
    {
        bool isCicle = cycle.IndexOf("False") != -1 ? false : true;

        string randomKey = GetRandomAudioKey();

        if (audioStreamPlayer3D.MaxDistance > 0) Play3D(randomKey, cycle);
        else Play2D(randomKey, cycle);

        return null;
    }

    public object Play2D(string soundKey, string cycle)
    {
        currentAudioKey = soundKey;

        bool isCicle = cycle.IndexOf("False") != -1 ? false : true;

        if (audioStreamPlayer3D.MaxDistance > 0) PlayCurrent3D(isCicle);
        else PlayCurrent2D(isCicle);

        return null;
    }

    public object Stop()
    {
        if (audioStreamPlayer3D != null && audioStreamPlayer3D.Playing)
            audioStreamPlayer3D.Stop();

        if (audioStreamPlayer2D != null && audioStreamPlayer2D.Playing)
            audioStreamPlayer2D.Stop();

        isPlaying = false;

        return null;
    }

    public object Pause()
    {
        if (audioStreamPlayer3D != null)
            audioStreamPlayer3D.StreamPaused = !audioStreamPlayer3D.StreamPaused;

        if (audioStreamPlayer2D != null)
            audioStreamPlayer2D.StreamPaused = !audioStreamPlayer2D.StreamPaused;

        isPlaying = false;

        return null;
    }

    public object SetMaxDistance(float distance)
    {
        CheckAudioStreamPlayer3D();
        audioStreamPlayer3D.MaxDistance = distance;
        return null;
    }


    private void CheckAudioStreamPlayer3D()
    {
        if (audioStreamPlayer3D == null)
        {
            audioStreamPlayer3D = new AudioStreamPlayer3D();
            AddChild(audioStreamPlayer3D);
        }
    }

    private void CheckAudioStreamPlayer2D()
    {
        if (audioStreamPlayer2D == null)
        {
            audioStreamPlayer2D = new AudioStreamPlayer();
            AddChild(audioStreamPlayer2D);
        }
    }

    private void PlayCurrent3D(bool isCicle)
    {
        PlayAudio3D(currentAudioKey, isCicle);
    }

    private void PlayAudio3D(string index, bool isCicle)
    {
        isPlaying = true;

        CheckAudioStreamPlayer3D();

        if (!audios.ContainsKey(index))
        {
            HSMLogger.Print(interactiveObject, $"Ошибка: звук с именем '{index}' не найден");
            return;
        }

        var audioStream = (AudioStream)AudioImporter.OpenAudioFile(audios[index]);
        if (audioStream is AudioStreamMP3 mp3Stream)
        {
            mp3Stream.Loop = isCicle;
        }
        audioStreamPlayer3D.Stream = audioStream;

        audioStreamPlayer3D.VolumeDb = 1;

        audioStreamPlayer3D.Play();
    }

    private void PlayCurrent2D(bool isCicle)
    {
        PlayAudio2D(currentAudioKey, isCicle);
    }

    private void PlayAudio2D(string audioName, bool isCicle)
    {
        isPlaying = true;

        CheckAudioStreamPlayer2D();

        if (!audios.ContainsKey(audioName))
        {
            HSMLogger.Print(interactiveObject, $"Ошибка: звук с именем '{audioName}' не найден");
            return;
        }

        var audioStream = (AudioStream)AudioImporter.OpenAudioFile(audios[audioName]);
        if (audioStream is AudioStreamMP3 mp3Stream)
        {
            mp3Stream.Loop = isCicle;
        }
        audioStreamPlayer2D.Stream = audioStream;

        audioStreamPlayer2D.VolumeDb = 1;

        audioStreamPlayer2D.Play();
    }



}
