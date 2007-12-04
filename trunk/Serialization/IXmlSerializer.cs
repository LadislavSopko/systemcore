using System;
using System.Collections.Generic;
using System.Text;

namespace System.Core.Serialization
{
    public interface IXmlSerializer<TValue>
    {
        void Serialize(string fileName, TValue value);
        TValue Deserialize(string fileName);

    }
}
