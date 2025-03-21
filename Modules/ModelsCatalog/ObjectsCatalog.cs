using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.GameObjects.View;

public interface IObjectLibrary
{
    void LoadLibrary(string path);
    void SaveLibrary(string path);
    ObjectData GetObject(string objectId);
    void AddObject(ObjectData data);
    void RemoveObject(string objectId);
}

public class ObjectData
{
    public string Id { get; set; }
    public int TypePrefab { get; set; } // тип префаба для объекта
    public string modelPath { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public int currentGraph { get; set; }
    public List<string> Graphs { get; set; }
    public List<string> Sounds { get; set; }
    public List<string> Animations { get; set; }

    public ObjectData()
    {
        Graphs = new List<string>();
        Sounds = new List<string>();
        Animations = new List<string>();
    }
}

public partial class ObjectsCatalog : Node
{
    public static ObjectsCatalog instance;

    [Export] ObjectsCatalogLoadObject panelLoadObject;

    private string CatalogPath;

    private IObjectLibrary objectLibrary;

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

        objectLibrary = new ProxyObjectLibrary();
    }

    public void OnOpenPanelLoadObject()
    {
        panelLoadObject.OnOpenLoadObject();
        LoadLibrary();

        //panelLoadObject.OpenEditModel(objectsLibrary[0]);
    }

    public void OnClosePanelLoadObject()
    {
        panelLoadObject.Visible = false;
    }

    public void LoadLibrary()
    {
        CatalogPath = ProjectSettings.GlobalizePath(VoxLib.mapManager.GetProjectFolderPath() + MapManager.PATHCATALOG + "Catalog.json");
        objectLibrary.LoadLibrary(CatalogPath);


    }

    public void AddModel(ObjectData model)
    {
        objectLibrary.AddObject(model);
        objectLibrary.SaveLibrary(CatalogPath);
    }

    public void RemoveModel(string Id)
    {
        objectLibrary.RemoveObject(Id);
        objectLibrary.SaveLibrary(CatalogPath);
    }


}
