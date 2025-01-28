using Godot;
using System;

public static class AudioImporter {
    // https://docs.godotengine.org/en/stable/tutorials/scripting/cross_language_scripting.html
    //static GodotObject GetAudioImporterGD() {
    //	GDScript MyGDScript = GD.Load<GDScript>("res://addons/Ursula/Modules/AudioLoader/GDScriptAudioImport.gd");
    //	return (GodotObject)MyGDScript.New();
    //}

    //https://github.com/Gianclgar/GDScriptAudioImport
    public static AudioStreamPlayer LoadAudioFile(string path){
		var music = new AudioStreamPlayer();
		music.Stream = OpenAudioFile(path);
		music.VolumeDb = 1;
        music.PitchScale = 1;
        return music;
	}

    public static AudioStream OpenAudioFile(string path)
    {
        return AudioParser.LoadFile(path);
    }

  //  public static AudioStream OpenAudioFile(string path){
		//var audioLoaderNode = GetAudioImporterGD();
		//var file = audioLoaderNode.Call("loadfile", ProjectSettings.GlobalizePath(path));
		//return (AudioStream)file;
  //  }
}
