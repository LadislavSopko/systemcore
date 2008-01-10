using System.IO;

namespace System.Core.Compression
{
    /// <summary>
    /// Defines the basic operations for compression and decompression
    /// </summary>
    public interface ISimpleCompressor
    {
        /// <summary>
        /// Compresses the <paramref name="inputStream"/> and outputs the result in the <paramref name="outputStream"/>.
        /// </summary>
        /// <param name="inputStream">The stream to compress.</param>
        /// <param name="outputStream">The stream containing the result of the compression.</param>
        void Compress(Stream inputStream, Stream outputStream);

        /// <summary>
        /// Compresses the <paramref name="inputFile"/> and outputs the result in the <paramref name="outputFile"/>.
        /// </summary>
        /// <param name="inputFile">The file to compress.</param>
        /// <param name="outputFile">The file containing the result of the compression.</param>
        void Compress(string inputFile, string outputFile);


        /// <summary>
        /// Decompresses the <paramref name="inputFile"/> and outputs the result in the <paramref name="outputFile"/>.
        /// </summary>
        /// <param name="inputFile">The file to compress.</param>
        /// <param name="outputFile">The file containing the result of the compression.</param>
        void Decompress(string inputFile, string outputFile);


        /// <summary>
        /// Decompresses the <paramref name="inputStream"/> and outputs the result in the <paramref name="inputStream"/>.
        /// </summary>
        /// <param name="inputStream">The file to compress.</param>
        /// <param name="outputStream">The file containing the result of the compression.</param>
        void Decompress(Stream inputStream, Stream outputStream);
    }
}
