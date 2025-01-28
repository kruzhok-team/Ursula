using Godot;
using System;

public partial class AudioLoader : Node
{
	public FileDialog _fileDialog;

	public void OpenAudioFile(){
        _fileDialog = GetNode<FileDialog>("FileDialog_OpenAudio");

        _fileDialog.Show();

        _fileDialog.Filters = new string[] { "*.wav ; WAV аудио", "*.mp3 ; MP3 аудио", "*.ogg ; OGG аудио" };

        // Проверка, подключен ли сигнал
        if (!_fileDialog.IsConnected("file_selected",new Callable( this, nameof(AudioFileProcess))))
        {
            _fileDialog.Connect("file_selected", new Callable(this, nameof(AudioFileProcess)));
        }
	}

	public void AudioFileProcess(string path){
		var music = AudioImporter.LoadAudioFile(path);
		AddChild(music);
		music.Play();
	}

    public override void _Ready()
	{
		OpenAudioFile();
	}


    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.T)
            {
                GD.Print("T was pressed");
                OpenAudioFile();
            }
        }
    }

}
