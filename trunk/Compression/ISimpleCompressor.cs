using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace System.Core.Compression
{
    public interface ISimpleCompressor
    {
        void Compress(Stream inputStream, Stream outputStream);
        void Compress(string inputFile, string outputFile);
        void Decompress(string inputFile, string outputFile);
        void Decompress(Stream inputStream, Stream outputStream);
    }
}
