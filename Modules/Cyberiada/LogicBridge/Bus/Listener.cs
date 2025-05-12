using System;
using System.Collections.Generic;

namespace Talent.Logic.Bus
{
    public delegate bool Listener(List<Tuple<string, string>> value);
}
