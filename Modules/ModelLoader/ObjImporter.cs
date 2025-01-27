using Godot;
using System;

public partial class ObjImporter 
{
    //static GodotObject GetObjParserGD()
    //{
    //    GDScript MyGDScript = GD.Load<GDScript>("res://addons/VoxLib/Modules/ModelLoader/ObjParse.gd");
    //    return (GodotObject)MyGDScript.New();
    //}

    //public static Mesh LoadObjFileAsMesh(string objPath, string mtlPath)
    //{
    //    //GDScript
    //    //var importer = GetObjParserGD();
    //    //return (Mesh)importer.Call("load_obj", objPath, mtlPath);

    //    //C#
    //    return (Mesh)ObjParser.LoadObj(objPath, mtlPath);
    //}

    public static Node3D LoadObjFile(string objPath, string mtlPath)
    {
        var mesh = (Mesh)ObjParser.LoadObj(objPath, mtlPath);

        MeshInstance3D meshInstance = new MeshInstance3D();
        meshInstance.Mesh = mesh;

        var instance = new Node3D();
        instance.Name = "Model";

        instance.AddChild(meshInstance);

        return instance;
    }
}
