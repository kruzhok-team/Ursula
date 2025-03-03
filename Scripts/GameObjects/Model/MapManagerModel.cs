using Godot;
using System;
using Ursula.Core.DI;

namespace Ursula.MapManagers.Model
{
    public partial class MapManagerModel : IInjectable
    {
        public event EventHandler GameObjectCreateItem_EventHandler;

        void IInjectable.OnDependenciesInjected()
        {
        }

    }
}
