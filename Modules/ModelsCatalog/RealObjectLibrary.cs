using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public partial class RealObjectLibrary : IObjectLibrary
{
    private List<ObjectData> objectsLibrary;

    public void LoadLibrary(string path)
    {
        if (!File.Exists(path))
        {
            GD.Print("Файл объектов не найден. Создается новый список.");
            objectsLibrary = new List<ObjectData>();
            return;
        }

        string json = File.ReadAllText(path);
        objectsLibrary = JsonSerializer.Deserialize<List<ObjectData>>(json);
    }

    public void SaveLibrary(string path)
    {
        string json = JsonSerializer.Serialize(objectsLibrary, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(path, json);
    }

    public ObjectData GetObject(string Id)
    {
        return objectsLibrary.Find(x => x.Id == Id);
    }

    public void AddObject(ObjectData data)
    {
        objectsLibrary.Add(data);
    }

    public void RemoveObject(string Id)
    {
        var model = objectsLibrary.Find(x => x.Id == Id);
        if (objectsLibrary.Contains(model)) objectsLibrary.Remove(model);
    }

}
