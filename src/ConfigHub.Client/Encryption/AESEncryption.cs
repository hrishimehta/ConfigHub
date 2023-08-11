using System.Security.Cryptography;
using System.Text;

namespace ConfigHub.Client.Encryption
{
    public class AesEncryptor : IEncryptor
    {
        public string Encrypt(string plaintext, string key)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.GenerateIV(); // Generate a random IV for each encryption

            using MemoryStream memoryStream = new MemoryStream();
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, aesAlg.CreateEncryptor(), CryptoStreamMode.Write);
            using StreamWriter streamWriter = new StreamWriter(cryptoStream);

            // Write the IV to the beginning of the ciphertext
            memoryStream.Write(aesAlg.IV, 0, aesAlg.IV.Length);

            streamWriter.Write(plaintext);
            streamWriter.Flush();
            cryptoStream.FlushFinalBlock();

            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public string Decrypt(string ciphertext, string key)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(key);

            // Extract IV from the beginning of the ciphertext
            byte[] iv = new byte[aesAlg.BlockSize / 8];
            Array.Copy(Convert.FromBase64String(ciphertext), iv, iv.Length);
            aesAlg.IV = iv;

            using MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(ciphertext), iv.Length, Convert.FromBase64String(ciphertext).Length - iv.Length);
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, aesAlg.CreateDecryptor(), CryptoStreamMode.Read);
            using StreamReader streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }
    }
}
