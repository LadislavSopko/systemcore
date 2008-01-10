using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace System.Core.Cryptography
{
    /// <summary>
    /// Performs simple cryptographic transformatations using the Rijndael algorithm. 
    /// </summary>
    public class SimpleRijndaelEncrypter : ISimpleEncrypter
    {

        #region Private Member Variables
        
        /// <summary>
        /// The alogortitm used for tranaformations
        /// </summary>
        private readonly SymmetricAlgorithm _algorithm = new RijndaelManaged();
        
        /// <summary>
        /// Hashprovider used to create the actual key to be used for transformation.
        /// </summary>
        private readonly HashAlgorithm _hashAlgorithm = new SHA256Managed();

        /// <summary>
        /// The actual key to be used for transformations
        /// </summary>
        private byte[] _keyHashBytes;
        
        /// <summary>
        /// The initialization vector to be used for tranformations
        /// </summary>
        private byte[] _ivBytes;

        #endregion

        #region Private Delegates

        /// <summary>
        /// Defines the method used for tranformations
        /// </summary>
        /// <param name="key">The key to be used for transformations.</param>
        /// <param name="inputStream">The source stream</param>
        /// <param name="outputStream">The stream used to output the result of the tranformation.</param>
        private delegate void StreamProcessor(string key, Stream inputStream, Stream outputStream);


        #endregion

        #region Private Methods

        /// <summary>
        /// Performs the transformation.
        /// </summary>
        /// <param name="key">The key to be used for tranformation.</param>
        /// <param name="inputStream">The source stream</param>
        /// <param name="writer">The stream to output the result of the transformation.</param>
        private static void TransformStream(string key, Stream inputStream, Stream writer)
        {
            if (inputStream == null)
                throw new ArgumentNullException("inputStream");
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (key == null || key.Length == 0)
                throw new ArgumentException("Cannot be null or empty", "key");

            byte[] buffer = new byte[1024];

            int bytesRead = inputStream.Read(buffer, 0, 1024);
            while (bytesRead > 0)
            {
                writer.Write(buffer, 0, bytesRead);
                bytesRead = inputStream.Read(buffer, 0, 1024);
            }
            writer.Close();
        }


        /// <summary>
        /// Creates a hashcode to be used as the encryption/decryption key
        /// </summary>
        /// <remarks>
        /// The initialization vector(IV) is created copying the requiried number of bytes from the generated hashcode.
        /// </remarks>
        /// <param name="key"></param>
        private void CreateHashCode(string key)
        {
            if (_keyHashBytes == null)
            {
                _ivBytes = new byte[_algorithm.BlockSize / 8];

                ASCIIEncoding encoding = new ASCIIEncoding();
                _keyHashBytes = _hashAlgorithm.ComputeHash(encoding.GetBytes(key));

                for (int i = 0; i <= (_algorithm.BlockSize / 8) - 1; i++)
                {
                    _ivBytes[i] = _keyHashBytes[i];
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Encrypts a stream using the <paramref name="key"/> as the encryption key.
        /// </summary>
        /// <param name="key">The key to be used as the encryption key</param>
        /// <param name="inputStream">The stream to be encrypted</param>
        /// <param name="outputStream">The encrypted stream</param>
        public void Encrypt(string key, Stream inputStream, Stream outputStream)
        {
            CreateHashCode(key);
            CryptoStream writer = new CryptoStream(outputStream, _algorithm.CreateEncryptor(_keyHashBytes, _ivBytes), CryptoStreamMode.Write);
            TransformStream(key, inputStream, writer);
        }


        /// <summary>
        /// Encrypts a file using the <paramref name="key"/> as the encryption key.
        /// </summary>
        /// <param name="key">The key to be used as the encryption key</param>
        /// <param name="inputFile">The file to be encrypted</param>
        /// <param name="outputFile">The encrypted file</param>
        public void Encrypt(string key, string inputFile, string outputFile)
        {
            TransformFile(key, inputFile, outputFile, Encrypt);
        }

        /// <summary>
        /// Decrypts a file using the <paramref name="key"/> as the decryption key.
        /// </summary>
        /// <param name="key">The key to be used as the decryption key</param>
        /// <param name="inputFile">The file to be decrypted</param>
        /// <param name="outputFile">The decrypted file</param>        
        public void Decrypt(string key, string inputFile, string outputFile)
        {
            TransformFile(key, inputFile, outputFile, Decrypt);
        }

        /// <summary>
        /// Decrypts a stream using the <paramref name="key"/> as the decryption key.
        /// </summary>
        /// <param name="key">The key to be used as the decryption key</param>
        /// <param name="inputStream">The stream to be decrypted</param>
        /// <param name="outputStream">The decrypted stream</param>        
        public void Decrypt(string key, Stream inputStream, Stream outputStream)
        {
            CreateHashCode(key);
            CryptoStream writer = new CryptoStream(outputStream, _algorithm.CreateDecryptor(_keyHashBytes, _ivBytes), CryptoStreamMode.Write);
            TransformStream(key, inputStream, writer);
        }

        private static void TransformFile(string key, string inputFile, string outputFile, StreamProcessor streamProcessor)
        {
            FileStream inputStream = null;
            FileStream outputStream = null;
            try
            {

                inputStream = new FileStream(inputFile, FileMode.Open);
                outputStream = new FileStream(outputFile, FileMode.OpenOrCreate);
                streamProcessor(key, inputStream, outputStream);
            }
            finally
            {
                if (inputStream != null)
                    inputStream.Close();
                if (outputStream != null)
                    outputStream.Close();
            }
        }

        #endregion
        
    }
}
