using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
namespace FlatTrade.Common.Helpers
{
    public class CipherHelper
    {
        public static (byte[] encryptedBytes, byte[] iv) EncryptObject<T>(T obj, byte[] key)
        {
            if (key == null || key.Length == 0)
            {
                throw new ArgumentNullException(nameof(key), "Encryption key cannot be null or empty.");
            }

            // 1. Serialize the object to a JSON string
            string jsonString = JsonConvert.SerializeObject(obj);

            // 2. Convert the JSON string to bytes
            byte[] plainBytes = Encoding.UTF8.GetBytes(jsonString);

            // 3. Perform AES encryption
            using Aes aesAlg = Aes.Create();

            aesAlg.Key = key;
            aesAlg.GenerateIV(); // Generate a new, unique IV for each encryption
            byte[] iv = aesAlg.IV; // Get the generated IV

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

            csEncrypt.Write(plainBytes, 0, plainBytes.Length);
            csEncrypt.FlushFinalBlock(); // Important to flush padding and final blocks
            return (msEncrypt.ToArray(), iv);
        }

        public static T DecryptObject<T>(byte[] encryptedBytes, byte[] iv, byte[] key)
        {
            if (encryptedBytes == null || encryptedBytes.Length == 0)
            {
                throw new ArgumentNullException(nameof(encryptedBytes), "Encrypted bytes cannot be null or empty.");
            }
            if (iv == null || iv.Length == 0)
            {
                throw new ArgumentNullException(nameof(iv), "IV cannot be null or empty.");
            }
            if (key == null || key.Length == 0)
            {
                throw new ArgumentNullException(nameof(key), "Decryption key cannot be null or empty.");
            }

            // 1. Perform AES decryption
            using Aes aesAlg = Aes.Create();

            aesAlg.Key = key;
            aesAlg.IV = iv; // Use the same IV that was generated during encryption

            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using var msDecrypt = new MemoryStream(encryptedBytes);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            // Read the decrypted bytes from the decrypting stream
            // and convert them to a string.
            string jsonString = srDecrypt.ReadToEnd();

            // 2. Deserialize the JSON string back to an object
            T decryptedObj = JsonConvert.DeserializeObject<T>(jsonString)!;
            return decryptedObj;
        }
    }
}
