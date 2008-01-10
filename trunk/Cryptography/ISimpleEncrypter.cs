using System.IO;

namespace System.Core.Cryptography
{
    /// <summary>
    /// Defines the basic operations for encryption and decrypting operations.
    /// </summary>
    /// <remarks> 
    /// Use this interface to implement simple symmetric cryptographic transformations.
    /// </remarks>    
    public interface ISimpleEncrypter
    {
        /// <summary>
        /// Encrypts a stream using the <paramref name="key"/> as the encryption key.
        /// </summary>
        /// <param name="key">The key to be used as the encryption key</param>
        /// <param name="inputStream">The stream to be encrypted</param>
        /// <param name="outputStream">The encrypted stream</param>
        void Encrypt(string key, Stream inputStream, Stream outputStream);
        
        /// <summary>
        /// Encrypts a file using the <paramref name="key"/> as the encryption key.
        /// </summary>
        /// <param name="key">The key to be used as the encryption key</param>
        /// <param name="inputFile">The file to be encrypted</param>
        /// <param name="outputFile">The encrypted file</param>
        void Encrypt(string key, string inputFile, string outputFile);

        /// <summary>
        /// Decrypts a file using the <paramref name="key"/> as the decryption key.
        /// </summary>
        /// <param name="key">The key to be used as the decryption key</param>
        /// <param name="inputFile">The file to be decrypted</param>
        /// <param name="outputFile">The decrypted file</param>        
        void Decrypt(string key, string inputFile, string outputFile);


        /// <summary>
        /// Decrypts a stream using the <paramref name="key"/> as the decryption key.
        /// </summary>
        /// <param name="key">The key to be used as the decryption key</param>
        /// <param name="inputStream">The stream to be decrypted</param>
        /// <param name="outputStream">The decrypted stream</param>        
        void Decrypt(string key, Stream inputStream, Stream outputStream);
    }
}
