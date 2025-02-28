using Godot;
using System;
using Ursula.Core.DI;

namespace Ursula.GameObjects.Model
{
    public partial class GameObjectCollectionModel : IInjectable
    {
        public bool IsCollectionVisible => isCollectionVisible;
        public string ItemIdSelected => itemIdSelected;

        private bool isCollectionVisible { get; set; }
        private string itemIdSelected { get; set; }

        public event EventHandler GameObjectCollectionVisibleChangeEvent;
        public event EventHandler GameObjectAssetSelectedEvent;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public GameObjectCollectionModel SetGameObjectCollectionVisible(bool value)
        {
            isCollectionVisible = value;
            InvokeGameObjectCollectionVisibleChangeEvent();
            return this;
        }

        public GameObjectCollectionModel SetGameObjectAssetSelected(string itemId)
        {
            itemIdSelected = itemId;
            InvokeGameObjectAssetSelectedEvent();
            return this;
        }

        private void InvokeGameObjectCollectionVisibleChangeEvent()
        {
            var handler = GameObjectCollectionVisibleChangeEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeGameObjectAssetSelectedEvent()
        {
            var handler = GameObjectAssetSelectedEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

    }
}
