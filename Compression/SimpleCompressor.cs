using System.IO;
using System.IO.Compression;

namespace System.Common.Compression
{
    /// <summary>
    /// Performs simple compression and decompression of file streams.
    /// </summary>
    public class SimpleCompressor : ISimpleCompressor
    {
        
        #region Private Delegates
        
        /// <summary>
        /// Defines a method used for compressing and decompressing a stream.
        /// </summary>
        /// <param name="inputStream">The source stream.</param>
        /// <param name="outputStream">The stream to output the result of the operation.</param>
        private delegate void StreamProcessor(Stream inputStream, Stream outputStream);

        #endregion

        #region Private Methods

        /// <summary>
        /// Processes the <paramref name="inputFile"/> and outputs the result to <paramref name="outputFile"/>
        /// </summary>
        /// <param name="inputFile">The file to compress/decompress</param>
        /// <param name="outputFile">The file containing the result of the operation</param>
        /// <param name="streamProcessor">The method to be used for compression/decompression.</param>
        private static void ProcessFile(string inputFile, string outputFile, StreamProcessor streamProcessor)
        {
            FileStream inputStream = null;
            FileStream outputStream = null;
            try
            {

                inputStream = new FileStream(inputFile, FileMode.Open);
                outputStream = new FileStream(outputFile, FileMode.OpenOrCreate);
                streamProcessor(inputStream, outputStream);
            }
            finally
            {
                if (inputStream != null)
                    inputStream.Close();
                if (outputStream != null)
                    outputStream.Close();
            }
        }


        /// <summary>
        /// Performs the compression/decompression
        /// </summary>
        /// <param name="inputStream">The source stream.</param>
        /// <param name="outputStream">The stream to output the result of the compression/decompression.</param>
        private static void ProcessStream(Stream inputStream, Stream outputStream)
        {
            if (inputStream == null)
                throw new ArgumentNullException("inputStream");
            if (outputStream == null)
                throw new ArgumentNullException("outputStream");

            byte[] buffer = new byte[1024];

            int bytesRead = inputStream.Read(buffer, 0, 1024);
            while (bytesRead > 0)
            {
                outputStream.Write(buffer, 0, bytesRead);
                bytesRead = inputStream.Read(buffer, 0, 1024);
            }

            outputStream.Close();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Compresses the <paramref name="inputStream"/> and outputs the result in the <paramref name="outputStream"/>.
        /// </summary>
        /// <param name="inputStream">The stream to compress.</param>
        /// <param name="outputStream">The stream containing the result of the compression.</param>
        public void Compress(Stream inputStream, Stream outputStream)
        {
            GZipStream writer = new GZipStream(outputStream,CompressionMode.Compress);            
            ProcessStream(inputStream,writer);            
        }

        /// <summary>
        /// Compresses the <paramref name="inputFile"/> and outputs the result in the <paramref name="outputFile"/>.
        /// </summary>
        /// <param name="inputFile">The file to compress.</param>
        /// <param name="outputFile">The file containing the result of the compression.</param>
        public void Compress(string inputFile, string outputFile)
        {
            ProcessFile(inputFile,outputFile,Compress);
        }

        /// <summary>
        /// Decompresses the <paramref name="inputFile"/> and outputs the result in the <paramref name="outputFile"/>.
        /// </summary>
        /// <param name="inputFile">The file to compress.</param>
        /// <param name="outputFile">The file containing the result of the compression.</param>
        public void Decompress(string inputFile, string outputFile)
        {
            ProcessFile(inputFile, outputFile, Decompress);
        }

        /// <summary>
        /// Decompresses the <paramref name="inputStream"/> and outputs the result in the <paramref name="inputStream"/>.
        /// </summary>
        /// <param name="inputStream">The file to compress.</param>
        /// <param name="outputStream">The file containing the result of the compression.</param>
        public void Decompress(Stream inputStream, Stream outputStream)
        {
            GZipStream writer = new GZipStream(outputStream, CompressionMode.Decompress);
            ProcessStream(inputStream, writer);
        }

       


        #endregion

    }
}
