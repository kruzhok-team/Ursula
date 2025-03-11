using Godot;
using System;
using System.IO;

public partial class ModelLoader : Node
{
    public static ModelLoader instance;
    FileDialogTool dialogTool;

    public override void _Ready()
    {
        instance = this;
        dialogTool = new FileDialogTool(GetNode("FileDialog") as FileDialog);
    }

    public static void OpenObj(Action<string, Node3D> loadingCallback)
    {
        if (instance == null)
            throw new Exception("ModelLoader is not initialized in the scene");

        VoxLib.mapManager.SetCameraCursorShow(true);

        instance.dialogTool.Open(new string[] { "*.obj ; Obj model", "*.glb ; glTF model" }, (path) =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                var instance = LoadModelByPath(path);
                loadingCallback.Invoke(path, instance);
            }
            else
                loadingCallback.Invoke("", null);
        }
        , FileDialog.AccessEnum.Filesystem);
    }

    public static Node3D LoadModelByPath(string path)
    {
        var extension = Path.GetExtension(path);

        switch (extension)
        {
            case ".obj": return ObjImporter.LoadObjFile(path, GetMtlFilePath(path));
            case ".glb": return GLTFLoader.Load(path);
        }

        return null;
    }

    public static string GetMtlFilePath(string objFilePath)
    {
        return Path.ChangeExtension(objFilePath, ".mtl");
    }

    //public static MeshInstance3D Instantiate(Node parent, Mesh mesh)
    //{
    //    MeshInstance3D meshInstance = new MeshInstance3D();
    //    meshInstance.Mesh = mesh;

    //    parent.AddChild(meshInstance);

    //    return meshInstance;
    //}

    public void OpenModelGrass()
    {
        string objPath;
        string destPath = VoxLib.mapManager.GetProjectFolderPath() + MapManager.PATHCUSTOMGRASS;

        OpenObj((path, mesh) =>
        {
            if (mesh != null)
            {
                objPath = path;
                CopyModel(objPath, destPath);
            }
            else
            {
                GD.PrintErr($"Ошибка копирования {path} в {destPath}");
            }
        });
    }

    public void OpenModelTrees()
    {
        string objPath;
        string destPath = VoxLib.mapManager.GetProjectFolderPath() + MapManager.PATHCUSTOMTREES;

        OpenObj((path, mesh) =>
        {
            if (mesh != null)
            {
                objPath = path;
                CopyModel(objPath, destPath);
            }
            else
            {
                GD.PrintErr($"Ошибка копирования {path} в {destPath}");
            }
        });
    }

    public static void CopyModel(string objPath, string destPath)
    {
        objPath = ProjectSettings.GlobalizePath(objPath);
        string destObjPath = ProjectSettings.GlobalizePath(destPath + Path.GetFileName(objPath));
        
        File.Copy(objPath, destObjPath, true);

        if (!objPath.Contains(".glb"))
        {
            string mtlPath = ProjectSettings.GlobalizePath(GetMtlFilePath(objPath));
            string destMtlPath = ProjectSettings.GlobalizePath(destPath + Path.GetFileName(mtlPath));

            if (File.Exists(mtlPath))
                File.Copy(mtlPath, destMtlPath, true);
            else
            {
                GD.PrintErr("Нет файла *.mtl");
                VoxLib.ShowMessage("Нет файла *.mtl");
            }

            var filePaths = ObjParser._GetMtlTexPaths(mtlPath);
            foreach (var k in filePaths)
            {
                string texFilepath = k.Replace("/", "\\");
                string texFilename = Path.GetFileName(texFilepath);

                if (!Godot.FileAccess.FileExists(texFilepath))
                {
                    texFilepath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(mtlPath), System.IO.Path.GetFileName(texFilename));
                    if (!Godot.FileAccess.FileExists(texFilepath))
                    {
                        continue;
                    }
                }

                string destTexFilepath = ProjectSettings.GlobalizePath(destPath + texFilename);

                File.Copy(texFilepath, destTexFilepath, true);
            }
        }

        VoxLib.mapManager.LoadCustomItems();

        GD.Print($"Скопирована модель {objPath} в {destObjPath}");
    }
}
