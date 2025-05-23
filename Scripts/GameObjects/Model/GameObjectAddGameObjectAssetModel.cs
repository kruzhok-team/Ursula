﻿using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using Ursula.Core.DI;
using Ursula.Core.Model;
using Ursula.Environment.Settings;
using Ursula.GameObjects.Model;

namespace Ursula.GameObjects.View
{
    public class GameObjectUserSourceData
    {
        public GameObjectUserSourceData()
        {
            AudiosFrom = new List<string>();
            AnimationsFrom = new List<string>();
            AudiosTo = new List<string>();
            AnimationsTo = new List<string>();
        }

        public string ModelName { get; private set; }

        public string PreviewImageFilePath { get; private set; }
        public string TextureFilePath { get; private set; }
        
        public string ModelPath { get; private set; }
        public string DestPath { get; private set; }

        public string GameObjectGroup { get; private set; }
        public int GameObjectClass { get; private set; }
        public string GameObjectSample { get; private set; }

        public List<string> AudiosFrom { get; set; }
        public List<string> AnimationsFrom { get; set; }

        public List<string> AudiosTo { get; set; }
        public List<string> AnimationsTo { get; set; }

        public string GraphXmlPath { get; private set; }

        public GameObjectUserSourceData SetModelName(string value)
        {
            ModelName = value;
            return this;
        }

        public GameObjectUserSourceData SetModelPath(string value)
        {
            ModelPath = value;
            return this;
        }
        public GameObjectUserSourceData SetDestPath(string value)
        {
            DestPath = value;
            return this;
        }
        public GameObjectUserSourceData AddSoundResources(string value)
        {
            AudiosFrom.Add(value);
            return this;
        }
        public GameObjectUserSourceData RemoveSoundResources(string value)
        {
            AudiosFrom.Remove(value);
            return this;
        }

        public GameObjectUserSourceData AddAnimationsResources(string value)
        {
            AnimationsFrom.Add(value);
            return this;
        }
        public GameObjectUserSourceData RemoveAnimationsResources(string value)
        {
            AnimationsFrom.Remove(value);
            return this;
        }
        public GameObjectUserSourceData SetGraphXmlPath(string value)
        {
            GraphXmlPath = value;
            return this;
        }
        public GameObjectUserSourceData SetPreviewImageFilePath(string value)
        {
            PreviewImageFilePath = value;
            return this;
        }
        public GameObjectUserSourceData SetGameObjectGroup(string value)
        {
            GameObjectGroup = value;
            return this;
        }
        public GameObjectUserSourceData SetGameObjectClass(int value)
        {
            GameObjectClass = value;
            return this;
        }
        public GameObjectUserSourceData SetGameObjectSample(string value)
        {
            GameObjectSample = value;
            return this;
        }
    }

    public partial class GameObjectAddGameObjectAssetModel : IInjectable
    {
        public bool IsGameObjectAddUserSourceVisible { get; private set; } = false;
        public bool IsEditMode { get; private set; } = false;
        public string ProviderId { get; set; } = GameObjectAssetsUserSource.LibId;

        public GameObjectAssetInfo assetInfo;
        public GameObjectTemplate template;

        public string ModelName => _gameObjectUserSourceData.ModelName;
        public string TextureFilePath => _gameObjectUserSourceData.TextureFilePath;
        public string ModelPath => _gameObjectUserSourceData.ModelPath;
        public string DestPath => _gameObjectUserSourceData.DestPath;
        public string GraphXmlPathFrom => _gameObjectUserSourceData.GraphXmlPath;
        public string GraphXmlPathTo => DestPath + Path.GetFileName(_gameObjectUserSourceData.GraphXmlPath);
        public string PreviewImageFilePathFrom => _gameObjectUserSourceData.PreviewImageFilePath;
        public string PreviewImageFilePathTo => DestPath + Path.GetFileName(PreviewImageFilePathFrom);
        public string GameObjectGroup => _gameObjectUserSourceData.GameObjectGroup;
        public int GameObjectClass => _gameObjectUserSourceData.GameObjectClass;
        public string GameObjectSample => _gameObjectUserSourceData.GameObjectSample;

        public event EventHandler GameGameObjectAddGameObjectAssetVisible_EventHandler;
        public event EventHandler GameObjectAddAssetToCollection_EventHandler;

        private GameObjectUserSourceData _gameObjectUserSourceData = new GameObjectUserSourceData();



        void IInjectable.OnDependenciesInjected()
        {
        }

        public GameObjectAddGameObjectAssetModel SetCurrentAssetToCollection()
        {
            SetAddGameObjectAssetToCollection(ModelName, GameObjectGroup, GameObjectClass, GameObjectSample);
            return this;
        }

        public GameObjectAddGameObjectAssetModel SetAddGameObjectAssetToCollection(string _modelName, string _gameObjectGroup, int _gameObjectClass, string _gameObjectSample)
        {
            SetModelName(_modelName);

            CopyGameObjectAsset();

            DirectoryInfo directoryInfo = new DirectoryInfo(DestPath);

            GameObjectAssetSources sources = new GameObjectAssetSources
                (
                _gameObjectUserSourceData.TextureFilePath,
                Path.GetFileName(_gameObjectUserSourceData.ModelPath),
                _gameObjectUserSourceData.AudiosTo,
                _gameObjectUserSourceData.AnimationsTo
                );

            template = new GameObjectTemplate
                (
                directoryInfo.Name,
                _gameObjectGroup,
                _gameObjectClass,
                _gameObjectSample,
                sources,
                Path.GetFileName(_gameObjectUserSourceData.GraphXmlPath),
                Path.GetFileName(PreviewImageFilePathFrom)
                );

            //if (assetInfo != null) Provider = assetInfo.ProviderId;
            //else Provider = GameObjectAssetsUserSource.LibId;

            InvokeGameObjectAddGameObjectAssetToCollectionEvent();

            return this;
        }

        public GameObjectAddGameObjectAssetModel SetGameObjectAddGameObjectAssetVisible(bool value, bool isEditMode = false)
        {
            IsGameObjectAddUserSourceVisible = value;
            this.IsEditMode = isEditMode;
            ProviderId = GameObjectAssetsUserSource.LibId;
            EventArgs eventArgs = new EventArgs();
            InvokeGameObjectAddUserSourceVisibleEvent(eventArgs);
            return this;
        }

        public GameObjectAddGameObjectAssetModel SetModelName(string value)
        {
            if (_gameObjectUserSourceData.ModelName == value) return this;
            _gameObjectUserSourceData.SetModelName(value);
            return this;
        }

        public GameObjectAddGameObjectAssetModel SetModelPath(string value)
        {
            if (_gameObjectUserSourceData.ModelPath == value) return this;
            _gameObjectUserSourceData.SetModelPath(value);
            return this;
        }

        public GameObjectAddGameObjectAssetModel SetDestPath(string value)
        {
            if (_gameObjectUserSourceData.DestPath == value) return this;
            _gameObjectUserSourceData.SetDestPath(value);
            return this;
        }

        public GameObjectAddGameObjectAssetModel AddSoundResources(string value)
        {
            _gameObjectUserSourceData.AddSoundResources(value);
            return this;
        }

        public GameObjectAddGameObjectAssetModel RemoveSoundResources(string value)
        {
            _gameObjectUserSourceData.RemoveSoundResources(value);
            return this;
        }

        public List<string> GetAnimationResources()
        {
            return _gameObjectUserSourceData.AnimationsFrom;
        }

        public GameObjectAddGameObjectAssetModel AddAnimationResources(string value)
        {
            _gameObjectUserSourceData.AddAnimationsResources(value);
            return this;
        }

        public GameObjectAddGameObjectAssetModel RemoveAnimationResources(string value)
        {
            _gameObjectUserSourceData.RemoveAnimationsResources(value);
            return this;
        }

        public GameObjectAddGameObjectAssetModel SetGraphXmlPath(string value)
        {
            if (_gameObjectUserSourceData.GraphXmlPath == value) return this;
            _gameObjectUserSourceData.SetGraphXmlPath(value);
            return this;
        }

        public GameObjectAddGameObjectAssetModel SetPreviewImageFilePath(string value)
        {
            if (_gameObjectUserSourceData.PreviewImageFilePath == value) return this;
            _gameObjectUserSourceData.SetPreviewImageFilePath(value);
            return this;
        }

        public GameObjectAddGameObjectAssetModel SetGameObjectGroup(string value)
        {
            if (_gameObjectUserSourceData.GameObjectGroup == value) return this;
            _gameObjectUserSourceData.SetGameObjectGroup(value);
            return this;
        }

        public GameObjectAddGameObjectAssetModel SetGameObjectClass(int value)
        {
            if (_gameObjectUserSourceData.GameObjectClass == value) return this;
            _gameObjectUserSourceData.SetGameObjectClass(value);
            return this;
        }

        public GameObjectAddGameObjectAssetModel SetGameObjectSample(string value)
        {
            if (_gameObjectUserSourceData.GameObjectSample == value) return this;
            _gameObjectUserSourceData.SetGameObjectSample(value);
            return this;
        }

        public GameObjectAddGameObjectAssetModel ClearAudios()
        {
            _gameObjectUserSourceData.AudiosFrom = new List<string>();
            _gameObjectUserSourceData.AudiosTo = new List<string>();
            return this;
        }

        public GameObjectAddGameObjectAssetModel ClearAnimations()
        {
            _gameObjectUserSourceData.AnimationsFrom = new List<string>();
            _gameObjectUserSourceData.AnimationsTo = new List<string>();
            return this;
        }

        private void CopyGameObjectAsset()
        {
            MapManager.CreateDir(DestPath);

            if (ProviderId == GameObjectAssetsUserSource.LibId)
                ModelLoader.CopyModel(ModelPath, DestPath);

            _gameObjectUserSourceData.AudiosTo = new List<string>();
            string pathAudio = DestPath + MapManager.PATHAUDIO;
            MapManager.CreateDir(pathAudio);
            for (int i = 0; i < _gameObjectUserSourceData.AudiosFrom.Count; i++)
            {
                string pathFrom = ProjectSettings.GlobalizePath(_gameObjectUserSourceData.AudiosFrom[i]);
                string pathTo = ProjectSettings.GlobalizePath($"{pathAudio}/{Path.GetFileName(_gameObjectUserSourceData.AudiosFrom[i])}");
                if (File.Exists(pathFrom) && pathFrom != pathTo)
                {
                    File.Copy(pathFrom, ProjectSettings.GlobalizePath(pathTo), true);               
                }
                _gameObjectUserSourceData.AudiosTo.Add($"{MapManager.PATHAUDIO}/{Path.GetFileName(pathTo)}");
            }

            _gameObjectUserSourceData.AnimationsTo = new List<string>();
            string pathAnimations = DestPath + MapManager.PATHANIMATION;
            MapManager.CreateDir(pathAnimations);
            for (int i = 0; i < _gameObjectUserSourceData.AnimationsFrom.Count; i++)
            {
                string pathFrom = _gameObjectUserSourceData.AnimationsFrom[i];
                string pathTo = $"{pathAnimations}/{Path.GetFileName(_gameObjectUserSourceData.AnimationsFrom[i])}";

                if (File.Exists(pathFrom) && pathFrom != pathTo)
                {
                    File.Copy(pathFrom, ProjectSettings.GlobalizePath(pathTo), true);
                }
                _gameObjectUserSourceData.AnimationsTo.Add($"{MapManager.PATHANIMATION}/{Path.GetFileName(pathTo)}");
            }

            string _graphXmlPathFrom = ProjectSettings.GlobalizePath(GraphXmlPathFrom);
            if (File.Exists(_graphXmlPathFrom)
                && _graphXmlPathFrom != ProjectSettings.GlobalizePath(GraphXmlPathTo))
            {
                File.Copy(_graphXmlPathFrom, ProjectSettings.GlobalizePath(GraphXmlPathTo), true);
            }

            string _previewImageFilePathFrom = ProjectSettings.GlobalizePath(PreviewImageFilePathFrom);
            if (File.Exists(_previewImageFilePathFrom)
                && _previewImageFilePathFrom != ProjectSettings.GlobalizePath(PreviewImageFilePathTo))
            {
                File.Copy(_previewImageFilePathFrom, ProjectSettings.GlobalizePath(PreviewImageFilePathTo), true);
            }
        }


        private void InvokeGameObjectAddGameObjectAssetToCollectionEvent()
        {
            var handler = GameObjectAddAssetToCollection_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeGameObjectAddUserSourceVisibleEvent(EventArgs eventArgs)
        {
            var handler = GameGameObjectAddGameObjectAssetVisible_EventHandler;
            handler?.Invoke(this, eventArgs);
        }


    }
}
