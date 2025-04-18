using Fractural.Tasks;
using Godot;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;

namespace Ursula.GameProjects.Model
{
    [Serializable]
    public partial class GameProjectAssetInfo: IInjectable
    {
        [JsonConstructor]
        public GameProjectAssetInfo(string name, string providerId, GameProjectTemplate template)
        {
            Name = name;
            ProviderId = providerId;
            Template = template;
        }

        public string Name { get; set; }
        public string ProviderId { get; }
        public GameProjectTemplate Template { get; set; }
        public string Id => ProviderId + "." + Name;

        public long ProjectSize
        {
            get
            {
                return GetDirectorySize(GetProjectPath());
            }
        }

        private Texture2D previewImage;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public async GDTask<Texture2D> GetPreviewImage()
        {
            if (previewImage != null) return previewImage;

            if (Template.PreviewImageFilePath == null) return null;

            string path = Template.PreviewImageFilePath;

            path = $"{GameProjectAssetsUserSource.CollectionPath}{Template.Folder}/{Template.PreviewImageFilePath}";

            previewImage = await _LoadPreviewImage(path);

            return previewImage;
        }

        public string GetJsonDataPath()
        {
            return $"{GetProjectPath()}/{GameObjectAssetsUserSource.NameSource}";
        }

        public string GetProjectPath()
        {
            string path = ProviderId == GameProjectAssetsUserSource.LibId
                ? $"{GameProjectAssetsUserSource.FolderPath}{Name}"
                : $"{GameProjectAssetsEmbeddedSource.FolderPath}{Name}";
            return ProjectSettings.GlobalizePath(path);
        }

        public string GetMapPath()
        {
            return $"{GetProjectPath()}/{GameProjectLibraryManager.MapName}";
        }

        public async GDTask LoadMap()
        {
            string pathMap = GetMapPath();
            await VoxLib.mapManager.LoadMapFromFile(pathMap);
        }

        public bool SaveMap()
        {
            string pathMap = GetMapPath();

            VoxLib.mapManager.SaveMapToFile(pathMap);

            return true;
        }

        public async GDTask ExportProject()
        {
            string exportFolder = GetExportFolderPath();
            string projectFolder = GetProjectPath();

            //if (!Directory.Exists(exportFolder))
            //    Directory.CreateDirectory(exportFolder);

            string pathZip = $"{projectFolder}.zip";
            if (File.Exists(pathZip)) File.Delete(pathZip);

            //CopyFolder(projectFolder, exportFolder);

            await GDTask.Delay(1000);

            ZipFile.CreateFromDirectory(projectFolder, pathZip);

            string path = Path.GetDirectoryName(pathZip);
            OpenInExplorer(path);
        }

        private string GetExportFolderPath()
        {
            string path = $"{GetExportFolderGamePath()}{Name}";
            return ProjectSettings.GlobalizePath(path);
        }

        private string GetExportFolderGamePath()
        {
            string executablePath = OS.GetExecutablePath();
            string executableDirectory = Path.GetDirectoryName(executablePath);

            string path = $"{executableDirectory}/{GameProjectAssetsEmbeddedSource.CollectionPath}";
            return ProjectSettings.GlobalizePath(path);
        }

        private void CopyFolder(string sourcePath, string destinationPath)
        {
            try
            {
                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                }

                foreach (string filePath in Directory.GetFiles(sourcePath))
                {
                    string fileName = Path.GetFileName(filePath);
                    string destinationFilePath = Path.Combine(destinationPath, fileName);
                    File.Copy(filePath, destinationFilePath, overwrite: true);
                    GD.Print($"Скопирован файл: {fileName}");
                }

                foreach (string subDirPath in Directory.GetDirectories(sourcePath))
                {
                    string subDirName = Path.GetFileName(subDirPath);
                    string destinationSubDirPath = Path.Combine(destinationPath, subDirName);
                    CopyFolder(subDirPath, destinationSubDirPath);
                }
            }
            catch (System.Exception ex)
            {
                GD.PrintErr($"Ошибка при копировании папки: {ex.Message}");
            }
        }

        private void OpenInExplorer(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch (System.Exception ex)
            {
                GD.PrintErr("Ошибка при открытии папки: ", ex.Message);
            }
        }

        private async GDTask<Texture2D> _LoadPreviewImage(string path)
        {
            Texture2D tex;
            Image img = new Image();

            var err = await Task.Run(() => img.Load(path));

            if (err != Error.Ok)
            {
                GD.Print("Failed to load image from path: " + path);
            }
            else
            {
                tex = ImageTexture.CreateFromImage(img);
                return tex;
            }
            return null;
        }

        private long GetDirectorySize(string directoryPath)
        {
            long totalSize = 0;

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);

                foreach (var file in dirInfo.GetFiles())
                {
                    totalSize += file.Length;
                }

                foreach (var subDir in dirInfo.GetDirectories())
                {
                    totalSize += GetDirectorySize(subDir.FullName);
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Ошибка при вычислении размера директории: {ex.Message}");
            }

            return totalSize;
        }
    }
}
