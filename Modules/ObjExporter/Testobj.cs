using Godot;
using System;

public partial class Testobj : Node
{
    static GodotObject GetObjParserGD()
    {
        GDScript MyGDScript = GD.Load<GDScript>("res://addons/VoxLib/Modules/ModelLoader/ObjParse.gd");
        return (GodotObject)MyGDScript.New();
    }

    public static Mesh LoadObjFile(string objPath, string mtlPath)
    {
        var importer = GetObjParserGD();
        return (Mesh)importer.Call("load_obj", objPath, mtlPath);
    }

    public override void _Ready()
    {
        
    }
}
