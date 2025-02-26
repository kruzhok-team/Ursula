using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
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
        }

        public string ModelPath { get; private set; }
        public string DestPath { get; private set; }

        List<string> audios = new List<string>();
        List<string> animations = new List<string>();

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
            audios.Add(value);
            return this;
        }
        public GameObjectUserSourceData RemoveSoundResources(string value)
        {
            audios.Remove(value);
            return this;
        }
        public GameObjectUserSourceData AddAnimationsResources(string value)
        {
            animations.Add(value);
            return this;
        }
        public GameObjectUserSourceData RemoveAnimationsResources(string value)
        {
            animations.Remove(value);
            return this;
        }
    }

    public partial class GameObjectAddUserSourceModel : IInjectable
    {
        public bool IsGameObjectAddUserSourceVisible { get; private set; } = false;

        public string modelName { get; private set; }
        public string ModelPath => _gameObjectUserSourceData.ModelPath;
        public string DestPath => _gameObjectUserSourceData.DestPath;

        public GameObjectAssetSources _gameObjectAssetSources { get; private set; }

        public event EventHandler GameGameObjectAddUserSourceVisible_EventHandler;
        public event EventHandler GameObjectAddUserSourceToCollection_EventHandler;

        private GameObjectLibraryManager _commonLibrary;
        private GameObjectUserSourceData _gameObjectUserSourceData = new GameObjectUserSourceData();


        //public GameObjectAddUserSourceModel()
        //{
        //    _gameObjectUserSourceData = new GameObjectUserSourceData();
        //}

        void IInjectable.OnDependenciesInjected()
        {
        }

        public GameObjectAddUserSourceModel SetAddUserSourceToCollection(string modelName, GameObjectAssetSources _gameObjectAssetSources)
        {
            this.modelName = modelName;
            this._gameObjectAssetSources = _gameObjectAssetSources;
            InvokeGameObjectAddUserSourceToCollectionEvent();
            return this;
        }

        public GameObjectAddUserSourceModel SetGameObjectAddUserSourceVisible(bool value)
        {
            IsGameObjectAddUserSourceVisible = value;
            EventArgs eventArgs = new EventArgs();
            InvokeGameObjectAddUserSourceVisibleEvent(eventArgs);
            return this;
        }

        public GameObjectAddUserSourceModel SetModelPath(string value)
        {
            if (_gameObjectUserSourceData.ModelPath == value) return this;
            _gameObjectUserSourceData.SetModelPath(value);
            return this;
        }

        public GameObjectAddUserSourceModel SetDestPath(string value)
        {
            if (_gameObjectUserSourceData.DestPath == value) return this;
            _gameObjectUserSourceData.SetDestPath(value);
            return this;
        }

        public GameObjectAddUserSourceModel AddSoundResources(string value)
        {
            _gameObjectUserSourceData.AddSoundResources(value);
            return this;
        }

        public GameObjectAddUserSourceModel RemoveSoundResources(string value)
        {
            _gameObjectUserSourceData.RemoveSoundResources(value);
            return this;
        }

        public GameObjectAddUserSourceModel AddAnimationResources(string value)
        {
            _gameObjectUserSourceData.AddAnimationsResources(value);
            return this;
        }

        public GameObjectAddUserSourceModel RemoveAnimationResources(string value)
        {
            _gameObjectUserSourceData.RemoveAnimationsResources(value);
            return this;
        }




        private void InvokeGameObjectAddUserSourceToCollectionEvent()
        {
            var handler = GameObjectAddUserSourceToCollection_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeGameObjectAddUserSourceVisibleEvent(EventArgs eventArgs)
        {
            var handler = GameGameObjectAddUserSourceVisible_EventHandler;
            handler?.Invoke(this, eventArgs);
        }

    }
}
