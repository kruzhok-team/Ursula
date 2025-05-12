using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileAccess = Godot.FileAccess;

public partial class VideoPlayer : Control
{
    public static VideoPlayer instance;

    private VideoStreamPlayer videoStreamPlayer;
    Action cbVideoFinished;

    public override void _Ready()
    {
        if (instance != null)
        {
            if (IsInstanceValid(instance))
                instance.Free();
            else
                instance = null;
        }

        instance = this;

        videoStreamPlayer = GetNode<VideoStreamPlayer>("VideoStreamPlayer");
        videoStreamPlayer.Connect("finished", new Callable(this, nameof(OnVideoFinished)));

        Visible = false;
    }

    private void LoadAndPlayVideo(string videoPath)
    {
        var file = FileAccess.Open(videoPath, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr("Не удалось загрузить видеофайл: " + videoPath);
            OnVideoFinished();
            return;
        }

        var videoStream = ResourceLoader.Load<VideoStream>(videoPath);

        if (videoStream != null)
        {
            Visible = true;
            videoStreamPlayer.Stream = videoStream;
            videoStreamPlayer.Play();
        }
        else
        {
            GD.PrintErr("Не удалось загрузить видеофайл: " + videoPath);
            OnVideoFinished();
        }
    }


    private void OnVideoFinished()
    {
        Visible = false;
        cbVideoFinished?.Invoke();
        cbVideoFinished = null;
    }

    private List<string> FindVideoFiles(string folderPath)
    {      
        string[] videoPathes = Directory.GetFiles(folderPath, "*.ogv");
        var videoFiles = new List<string>(videoPathes);
        return videoFiles;
    }

    public void PlayVideo(string pathDir, Action cb = null)
    {
        cbVideoFinished = cb;
        List<string> videoFiles = FindVideoFiles(pathDir);
        if (videoFiles != null && videoFiles.Count > 0)
        {
            LoadAndPlayVideo(videoFiles[0]);
        }
        else OnVideoFinished();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed)
        {
            if (Visible == true && videoStreamPlayer.IsPlaying()) OnVideoFinished();
        }
    }
}
