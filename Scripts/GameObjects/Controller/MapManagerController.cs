using Godot;
using System;
using Ursula.Core.DI;

namespace Ursula.MapManagers.Controller
{
    public partial class MapManagerController : Node, IInjectable
    {


        void IInjectable.OnDependenciesInjected()
        {
        }
    }
}
