using Godot;
using System;

namespace Core.UI.Constructor
{
    public partial class ReactiveProperty<T>
    {
        T value = default(T);

        public ReactiveProperty(T initialValue)
        {
            SetValue(initialValue);
        }

        public bool HasValue
        {
            get
            {
                return true;
            }
        }


        protected virtual void SetValue(T value)
        {
            this.value = value;
        }

    }
}
