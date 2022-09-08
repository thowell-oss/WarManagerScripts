/*  MD5Passwod.cs
    Author: java2s.com, Microsoft Docs
    Modified By: Taylor Howell
    Notes: MD5 hash code pulled from webiste: http://www.java2s.com/Code/CSharp/Security/GetandverifyMD5Hash.htm
    Notes: AES encryption pulled from website: https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes?view=net-5.0
*/

using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace WarManager.Sharing.Security
{

    public static class Encryption
    {
        #region java2s.com http://www.java2s.com/Code/CSharp/Security/GetandverifyMD5Hash.htm

        /// <summary>
        /// Get 32 character MD5 hash from a given string
        /// </summary>
        /// <param name="input">the given string</param>
        /// <returns>returns an md5 hash</returns>
        [Notes.Author("java2s.com", 1.0, "Handles MD5 Hashing capabilities")]
        [Notes.Modified("Taylor Howell", "Changed a few small things to make the code work with War Manager Security Systems", 1.0)]
        public static string GetMD5Hash(string input)
        {
            if (input == null || input == string.Empty)
                throw new NullReferenceException("input cannot be null or empty");

            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        /// <summary>
        /// Verify hash against string
        /// </summary>
        /// <param name="input">the given string</param>
        /// <param name="hash">the hash code</param>
        /// <returns>returns true if the hash is correct, false if not/returns>
        [Notes.Author("java2s.com", 1.0, "Handles MD5 Hashing capabilities")]
        [Notes.Modified("Taylor Howell", "Changed a few small things to make the code work with War Manager Security Systems", 1.0)]
        public static bool VerifyMD5Hash(string input, string hash)
        {
            if (input == null || input == string.Empty)
                throw new NullReferenceException("input cannot be null empty");

            if (hash == null || hash == string.Empty)
                throw new NullReferenceException("hash cannot be null or empty");

            // Hash the input.
            string hashOfInput = GetMD5Hash(input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Microsoft Docs https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes?view=net-5.0


        /// <summary>
        /// Convert text into a password using a key and IV
        /// </summary>
        /// <param name="plainText">the plain text</param>
        /// <param name="Key">the key</param>
        /// <param name="IV">the initialization vector</param>
        /// <returns>returns a byte array containing the password</returns>
        [Notes.Author("Microsoft Docs", 1.0, "Handles AES encryption")]
        [Notes.Modified("Taylor Howell", "Changed a few small things to make the code work with War Manager Security Systems", 1.0)]
        public static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        /// <summary>
        /// Decrypt the byte array into a readable string
        /// </summary>
        /// <param name="cipherText">the password text</param>
        /// <param name="Key">the key</param>
        /// <param name="IV">the IV</param>
        /// <returns>returns the decrypted string</returns>
        [Notes.Author("Microsoft Docs", 1.0, "Handles AES encryption")]
        [Notes.Modified("Taylor Howell", "Changed a few small things to make the code work with War Manager Security Systems", 1.0)]
        public static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        /// <summary>
        /// Decrypt the files
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="path">the location fo the file</param>
        /// <returns></returns>
        [Notes.Author("Microsoft Docs", 1.0, "Handles AES encryption")]
        [Notes.Modified("Taylor Howell", "Changed a few small things to make the code work with War Manager Security Systems", 1.0)]
        public static string DecryptGetFile(byte[] key, string path)
        {
            var bytes = File.ReadAllBytes(path);

            byte[] IV = new byte[16];
            int i = 0;
            while (i < 16)
            {
                IV[i] = bytes[i];
                i++;
            }

            byte[] finishedFile = new byte[bytes.Length - 16];

            Array.Copy(bytes, 16, finishedFile, 0, finishedFile.Length);

            var dec = Encryption.DecryptStringFromBytes_Aes(bytes, key, IV);
            return dec;
        }

        public static void EncryptSaveFile(string text, byte[] key, string path)
        {
            byte[] IV = Guid.NewGuid().ToByteArray();
            byte[] file = EncryptStringToBytes_Aes(text, key, IV);

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                stream.Write(IV, 0, IV.Length);

                for (int i = 0; i < file.Length; i++)
                {
                    stream.WriteByte(file[i]);
                }
            }
        }

        #endregion
    }
}
