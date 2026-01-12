using Modules.HSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class VariableHolderLazy<T>
{
    public T Value => getter.Invoke();
    private Func<T> getter;

    public VariableHolderLazy(Func<T> getter)
    {
        this.getter = getter;
    }
}
