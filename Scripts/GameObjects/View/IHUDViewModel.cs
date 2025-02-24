using Fractural.Tasks;
using Godot;
using System;

namespace Ursula.GameObjects.View
{
    public interface IHUDViewModel
    {
        void SetNameMap(string nameMap);
        void SetInfo(string info);
    }
}
