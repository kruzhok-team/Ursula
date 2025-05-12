using Godot;
using System;
using System.IO;
using System.Text.Json;

public partial class ProxyObjectLibrary : IObjectLibrary
{
    private RealObjectLibrary realLibrary;

    public ProxyObjectLibrary()
    {
        realLibrary = new RealObjectLibrary();
    }

    public void LoadLibrary(string path)
    {
        GD.Print($"Прокси: Инициализация библиотеки объектов.");
        realLibrary.LoadLibrary(path);
    }

    public void SaveLibrary(string path)
    {
        realLibrary.SaveLibrary(path);
    }

    public ObjectData GetObject(string objectId)
    {
        ObjectData data = realLibrary.GetObject(objectId);
        if (data == null ) GD.PrintErr($"Прокси: Объект {objectId} не найден.");
        return data;
    }

    public void AddObject(ObjectData data)
    {
        realLibrary.AddObject(data);
    }

    public void RemoveObject(string objectId)
    {
        realLibrary.RemoveObject(objectId);
    }
}
