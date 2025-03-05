using Godot;
using System;
using Ursula.Core.DI;

namespace Ursula.GameObjects.Model
{
    public partial class GameObjectCollectionModel : IInjectable
    {
        public bool IsCollectionVisible => isCollectionVisible;
        public GameObjectAssetInfo AssetSelected => assetSelected;
        public string NameGameObjectGroup => nameGameObjectGroup;

        private bool isCollectionVisible { get; set; }
        private GameObjectAssetInfo assetSelected { get; set; }
        private string nameGameObjectGroup { get; set; }

        public event EventHandler GameObjectCollectionVisibleChangeEvent;
        public event EventHandler GameObjectAssetSelected_EventHandler;
        public event EventHandler GameObjectDrawCollectionEvent;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public GameObjectCollectionModel SetGameObjectCollectionVisible(bool value)
        {
            isCollectionVisible = value;
            InvokeGameObjectCollectionVisibleChangeEvent();
            return this;
        }

        public GameObjectCollectionModel SetGameObjectAssetSelected(GameObjectAssetInfo asset)
        {
            assetSelected = asset;
            InvokeGameObjectAssetSelectedEvent();
            return this;
        }

        public GameObjectCollectionModel DrawCollection(string nameGroup)
        {
            nameGameObjectGroup = nameGroup;
            InvokeGameObjectDrawCollectionEvent();
            return this;
        }

        private void InvokeGameObjectCollectionVisibleChangeEvent()
        {
            var handler = GameObjectCollectionVisibleChangeEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeGameObjectAssetSelectedEvent()
        {
            var handler = GameObjectAssetSelected_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeGameObjectDrawCollectionEvent()
        {
            var handler = GameObjectDrawCollectionEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
