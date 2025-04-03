using System;
using System.Text.Json.Serialization;

namespace Ursula.GameProjects.Model
{
    [Serializable]
    public partial class GameProjectTemplate
    {
        public readonly string Folder;
        public readonly string Alias;
        public readonly string PreviewImageFilePath;

        [JsonConstructor]
        public GameProjectTemplate(string folder, string alias, string previewImageFilePath)
        {
            Folder = folder;
            Alias = alias;
            PreviewImageFilePath = previewImageFilePath;
        }
    }
}