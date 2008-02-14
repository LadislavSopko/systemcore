using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace System.Common.Cryptography
{
    /// <summary>
    /// Performs simple cryptographic transformatations using the Rijndael algorithm. 
    /// </summary>
    public class SimpleRijndaelTransformer : ISimpleCryptoTransformer
    {

        #region Private Member Variables

        private const int BUFFER_SIZE = 1024;

        private const int HASH_SIZE = 32;


        /// <summary>
        /// The alogortitm used for tranaformations
        /// </summary>
        private readonly SymmetricAlgorithm _algorithm = new RijndaelManaged();
        

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

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleRijndaelTransformer"/> class.
        /// </summary>
        public SimpleRijndaelTransformer()
        {
            _algorithm.Padding = PaddingMode.PKCS7;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Performs the transformation. 
        /// </summary>
        /// <param name="key">The key to be used as the encryption key.</param>
        /// <param name="inputFile">The file to be transformed.</param>
        /// <param name="outputFile">The file to output the result of the transformation.</param>
        /// <param name="streamProcessor">A <see cref="StreamProcessor"/> delegate that performs the actual transformation</param>
        private static void TransformFile(string key, string inputFile, string outputFile, StreamProcessor streamProcessor)
        {
            FileStream inputStream = null;
            FileStream outputStream = null;
            try
            {
                inputStream = new FileStream(inputFile, FileMode.Open);
                outputStream = new FileStream(outputFile, FileMode.Create);
                streamProcessor(key, inputStream, outputStream);
            }
            finally
            {
                if (inputStream != null)
                {
                    inputStream.Close();
                    inputStream.Dispose();
                }
                if (outputStream != null)
                {
                    outputStream.Close();
                    outputStream.Dispose();
                }
            }
        }


        /// <summary>
        /// Performs the transformation.
        /// </summary>        
        /// <param name="inputStream">The source stream</param>
        /// <param name="outputStream">The stream to output the result of the transformation.</param>
        private static void TransformStream(Stream inputStream, Stream outputStream)
        {
            if (inputStream == null)
                throw new ArgumentNullException("inputStream");
            if (outputStream == null)
                throw new ArgumentNullException("outputStream");            
    
            byte[] buffer = new byte[BUFFER_SIZE];

            int bytesRead = inputStream.Read(buffer, 0, BUFFER_SIZE);
            while (bytesRead > 0)
            {
                outputStream.Write(buffer, 0, bytesRead);
                bytesRead = inputStream.Read(buffer, 0, BUFFER_SIZE);
            }
            //Make sure that everything is written to the destination stream.
            //This will otherwize cause a "Invalid Padding" exception.
            if (outputStream is CryptoStream)
                ((CryptoStream)outputStream).FlushFinalBlock();
            else
                outputStream.Flush();
        }

        /// <summary>
        /// Creates a new <see cref="CryptoStream"/> to be used for transformation.
        /// </summary>
        /// <param name="key">The to be used for transformation.</param>
        /// <param name="targetStream">The target stream</param>
        /// <param name="mode"><see cref="CryptoStreamMode.Read"/> creates a decryptor, while <see cref="CryptoStreamMode.Write"/> creates an encryptor.</param>
        /// <returns>A <see cref="CryptoStream"/> that can be used for transformation</returns>
        private CryptoStream CreateCryptoStream(string key, Stream targetStream, CryptoStreamMode mode)
        {
            byte[] ivBytes = new byte[_algorithm.BlockSize / 8];
            byte[] keyHashBytes;

            HashAlgorithm hashAlgorithm = new SHA256Managed();
            UnicodeEncoding encoding = new UnicodeEncoding();
            keyHashBytes = hashAlgorithm.ComputeHash(encoding.GetBytes(key));

            for (int i = 0; i <= (_algorithm.BlockSize / 8) - 1; i++)
            {
                ivBytes[i] = keyHashBytes[i];
            }

            CryptoStream cryptoStream;

            if (mode == CryptoStreamMode.Read)
            {
                cryptoStream = new CryptoStream(targetStream, _algorithm.CreateDecryptor(keyHashBytes, ivBytes), mode);
                byte[] embeddedBytes = new byte[HASH_SIZE];
                cryptoStream.Read(embeddedBytes, 0, HASH_SIZE);
                ValidateKey(embeddedBytes, keyHashBytes);
            }
            else
            {
                cryptoStream = new CryptoStream(targetStream, _algorithm.CreateEncryptor(keyHashBytes, ivBytes), mode);
                cryptoStream.Write(keyHashBytes, 0, HASH_SIZE);
            }

            return cryptoStream;
        }

        /// <summary>
        /// Validates the key from the encrypted data against the hashcode generated from the transformation key.
        /// </summary>
        /// <param name="embeddedKeyBytes">Byte array read from the beginning of the encrypted data.</param>
        /// <param name="keyHashBytes">Byte array representing the hashcode generated from the transformation key.</param>
        private static void ValidateKey(byte []embeddedKeyBytes,byte[] keyHashBytes)
        {
            UnicodeEncoding encoding = new UnicodeEncoding();
            if (!encoding.GetString(embeddedKeyBytes, 0, HASH_SIZE).Equals(encoding.GetString(keyHashBytes,0,HASH_SIZE)))
                throw new InvalidKeyException("The given key is invalid for decryption.");            
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
            TransformStream(inputStream, CreateCryptoStream(key,outputStream,CryptoStreamMode.Write));
        }

        /// <summary>
        /// Decrypts a stream using the <paramref name="key"/> as the decryption key.
        /// </summary>
        /// <param name="key">The key to be used for decryption.</param>
        /// <param name="inputStream">The stream to be decrypted</param>
        /// <param name="outputStream">The decrypted stream</param>        
        /// <exception cref="InvalidKeyException">When the <paramref name="key"/> is different from the one used for encryption.</exception>
        public void Decrypt(string key, Stream inputStream, Stream outputStream)
        {
            TransformStream(CreateCryptoStream(key, inputStream, CryptoStreamMode.Read),outputStream );         
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
        /// <exception cref="InvalidKeyException">When the <paramref name="key"/> is different from the one used for encryption.</exception>   
        public void Decrypt(string key, string inputFile, string outputFile)
        {
            TransformFile(key, inputFile, outputFile, Decrypt);
        }       
        
        #endregion
        
    }
}
