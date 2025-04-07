using Core.UI.Constructor;
using Godot;
using System;
using Ursula.Core.DI;

namespace Ursula.ConstructorMenu.Model
{
    public partial class ConstructorMenuFileModel : ConstructorViewModel, IInjectable
    {
        void IInjectable.OnDependenciesInjected()
        {

        }
    }
}
