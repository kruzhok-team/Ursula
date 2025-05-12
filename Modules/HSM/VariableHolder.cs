namespace Modules.HSM
{
    public class VariableHolder<T>
    {
        public T Value { get; set; }

        public VariableHolder(T value)
        {
            Value = value;
        }
    }
}
