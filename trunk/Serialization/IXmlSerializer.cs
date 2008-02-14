using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace System.Common.Serialization
{
    public interface IXmlSerializer<TValue>
    {
        void Serialize(string fileName, TValue value);
        void Serialize(Stream stream, TValue value);
        TValue Deserialize(string fileName);

    }
}
