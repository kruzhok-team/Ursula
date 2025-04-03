using Godot;
using System;

namespace Core.UI.Constructor
{
    public abstract partial class ConstructorViewModel : ViewModel
    {
        public readonly ReactiveProperty<bool> OpenView = new(false);
    }
}
