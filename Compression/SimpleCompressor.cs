using System.IO;
using System.IO.Compression;

namespace System.Core.Compression
{
    public class SimpleCompressor : ISimpleCompressor
    {

        private delegate void StreamProcessor(Stream inputStream, Stream outputStream);

        #region ISimpleCompressor Members

        public void Compress(Stream inputStream, Stream outputStream)
        {
            GZipStream writer = new GZipStream(outputStream,CompressionMode.Compress);            
            ProcessStream(inputStream,writer);            
        }

        public void Compress(string inputFile, string outputFile)
        {
            ProcessFile(inputFile,outputFile,Compress);
        }

        public void Decompress(string inputFile, string outputFile)
        {
            ProcessFile(inputFile, outputFile, Decompress);
        }

        public void Decompress(Stream inputStream, Stream outputStream)
        {
            GZipStream writer = new GZipStream(outputStream, CompressionMode.Decompress);
            ProcessStream(inputStream, writer);
        }

        private static void ProcessFile(string inputFile, string outputFile,StreamProcessor streamProcessor)
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



        private static void ProcessStream(Stream inputStream,Stream outputStream)
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
    }
}
