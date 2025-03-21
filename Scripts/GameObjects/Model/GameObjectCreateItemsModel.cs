using Godot;
using System;
using Ursula.Core.DI;

namespace Ursula.GameObjects.Model
{
    public partial class GameObjectCreateItemsModel : IInjectable
    {
        public Node Collider => colliderNode;
        public Vector3 PositionNode => positionNode;
        public float ScaleNode => scaleNode;
        public byte RotationNode => rotationNode;

        private Node colliderNode { get; set; }
        private Vector3 positionNode { get; set; }
        private float scaleNode { get; set; }
        private byte rotationNode { get; set; }

        public event EventHandler GameObjectCreateItem_EventHandler;
        public event EventHandler GameObjectDeleteItem_EventHandler;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public GameObjectCreateItemsModel SetGameObjectCreateItem(Vector3 positionNode, float scaleNode, byte rotationNode)
        {
            this.positionNode = positionNode;
            this.scaleNode = scaleNode;
            this.rotationNode = rotationNode;
            InvokeGameObjectCreateItemEvent();
            return this;
        }

        public GameObjectCreateItemsModel SetGameObjectDeleteItem(Node colliderNode)
        {
            this.colliderNode = colliderNode;
            InvokeGameObjectDeleteItemEvent();
            return this;
        }

        private void InvokeGameObjectCreateItemEvent()
        {
            var handler = GameObjectCreateItem_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeGameObjectDeleteItemEvent()
        {
            var handler = GameObjectDeleteItem_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
