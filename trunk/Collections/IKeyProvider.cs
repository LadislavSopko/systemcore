using System;
using System.Collections.Generic;
using System.Text;

namespace System.Core.Collections
{
    public interface IKeyProvider<T>
    {
        T Key { get; }
    }
}
