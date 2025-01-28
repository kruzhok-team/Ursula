using Godot;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;

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

    private AudioStreamPlayer audioStreamPlayer2D = null;
    private AudioStreamPlayer3D audioStreamPlayer3D = null;

    private List<string> audioPathes = new List<string>();
    private int currentAudioIndex = 0;

    public bool isPlaying = false;
    public string currentAudioName = "";

    public override void _Ready()
    {
        CheckAudioStreamPlayer3D();
    }

    public override void _Process(double delta)
    {

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

    private void AddAudio(string path)
    {
        audioPathes.Add(path);
    }

    public void SetAudio(string path)
    {
        if (audioPathes.Contains(path))
        {
            currentAudioIndex = audioPathes.IndexOf(path);
        }
        else
        {
            AddAudio(path);
            currentAudioIndex = audioPathes.IndexOf(path);
        }

        //currentAudioName = audioPathes[currentAudioIndex];
    }

    public object Play3D(string soundId, string cycle)
    {
        currentAudioName = soundId;

        string path = PATH_AUDIO + soundId + ".mp3";
        //path = Path.GetFullPath(path);

        if (Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read) == null) path = PATH_AUDIO + soundId + ".wav";
        if (Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read) == null) path = PATH_AUDIO + soundId + ".ogg";
        if (Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read) == null) return null;
        //if (!File.Exists(path)) path = PATH_AUDIO + soundId + ".ogg";
        //if (!File.Exists(path)) return null;

        bool isCicle = cycle.IndexOf("False") != -1 ? false : true;

        //List<string> soundFiles = GetAllSounds(PATH_AUDIO);
        //for (int i = 0; i < soundFiles.Count; i++)
        //{

        //}

        SetAudio(path);
        PlayCurrent3D(isCicle);
        return null;
    }

    private List<AudioStream> soundStreams = new List<AudioStream>();

    public object PlayRandom3D(string cycle)
    {
        List<string> soundFiles = GetAllSounds(PATH_AUDIO);

        var _rng = new RandomNumberGenerator();

        int id =_rng.RandiRange(0, soundFiles.Count);

        Play3D(soundFiles[id], cycle);

        return null;
    }

    public object PlayRandom2D(string cycle)
    {
        bool isCicle = cycle.IndexOf("False") != -1 ? false : true;

        List<string> soundFiles = GetAllSounds(PATH_AUDIO);

        var _rng = new RandomNumberGenerator();

        int id = _rng.RandiRange(0, soundFiles.Count);

        if (audioStreamPlayer3D.MaxDistance > 0) Play3D(soundFiles[id], cycle);
        else Play2D(soundFiles[id], cycle);

        return null;
    }


    public object Play2D(string soundId, string cycle)
    {
        currentAudioName = soundId;
        string path = PATH_AUDIO + soundId + ".mp3";

        bool isCicle = cycle.IndexOf("False") != -1 ? false : true;

        isPlaying = true;
        SetAudio(path);

        if (audioStreamPlayer3D.MaxDistance > 0) PlayCurrent3D(isCicle);
        else PlayCurrent2D(isCicle);

        return null;
    }

    private void PlayCurrent3D(bool isCicle)
    {
        PlayAudio3D(currentAudioIndex, isCicle);
    }

    private void PlayAudio3D(int index, bool isCicle)
    {
        isPlaying = true;

        CheckAudioStreamPlayer3D();

        var audioStream = (AudioStream)AudioImporter.OpenAudioFile(audioPathes[index]);
        if (audioStream is AudioStreamMP3 mp3Stream)
        {
            mp3Stream.Loop = isCicle;
        }
        audioStreamPlayer3D.Stream = audioStream;

        audioStreamPlayer3D.VolumeDb = 1;

        /*if (!audioStreamPlayer3D.Playing)*/ 
        audioStreamPlayer3D.Play();
    }

    private void PlayCurrent2D(bool isCicle)
    {
        PlayAudio2D(currentAudioIndex, isCicle);
    }

    private void PlayAudio2D(int index, bool isCicle)
    {
        isPlaying = true;

        CheckAudioStreamPlayer2D();

        var audioStream = (AudioStream)AudioImporter.OpenAudioFile(audioPathes[index]);
        if (audioStream is AudioStreamMP3 mp3Stream)
        {
            mp3Stream.Loop = isCicle;
        }
        audioStreamPlayer2D.Stream = audioStream;

        audioStreamPlayer2D.VolumeDb = 1;

        audioStreamPlayer2D.Play();
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

    public List<string> GetAllSounds(string path)
    {
        List<string> soundFiles = new List<string>();

        using var dir = DirAccess.Open(path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (!dir.CurrentIsDir() && (fileName.EndsWith(".wav") || fileName.EndsWith(".ogg") || fileName.EndsWith(".mp3")))
                {
                    soundFiles.Add(Path.GetFileNameWithoutExtension(fileName));
                }
                fileName = dir.GetNext();
            }
        }
        return soundFiles;
    }
}
