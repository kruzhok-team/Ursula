using Godot;
using System;

public static partial class GLTFLoader
{
    public static Node3D Load(string path)
    {
        GD.Print("spawnGlTF");
        var gltf = new GltfDocument();
        var gltfState = new GltfState();

        var globalPath = ProjectSettings.GlobalizePath(path);

        if (FileAccess.FileExists(globalPath))
        {
            GD.Print("File to spawn exists");
        }
        else
        {
            GD.Print("File to spawn does not exist");
            return null;
        }

        var sndFile = FileAccess.Open(globalPath, FileAccess.ModeFlags.Read);
        var fileBytes = sndFile.GetBuffer((long)sndFile.GetLength());

        gltf.AppendFromBuffer(fileBytes, "base_path?", gltfState);

        var node = gltf.GenerateScene(gltfState);


        var animationPlayer = node.GetNodeOrNull("AnimationPlayer") as AnimationPlayer;

        if (animationPlayer != null)
        {
            var animationList = animationPlayer.GetAnimationList();

            if (animationList.Length > 0)
            {
                string firstAnimation = animationList[0];

                var animation = animationPlayer.GetAnimation(firstAnimation);
                if (animation != null)
                {
                    animation.LoopMode = Animation.LoopModeEnum.Linear;
                }

                animationPlayer.Play(firstAnimation);
            }
            else
            {
                GD.Print("AnimationPlayer не содержит анимаций.");
            }
        }

        return node as Node3D; //AddChild(node);
    }
}
