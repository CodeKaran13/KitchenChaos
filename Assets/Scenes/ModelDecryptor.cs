using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ModelDecryptor
{
    public class Decryptor
    {
        public Decryptor()
        {
        }

        public static byte[] DecryptModel(string modelPath, string key)
        {
            try
            {
                using (FileStream fileStream = new(modelPath, FileMode.Open))
                {
                    using (Aes aes = Aes.Create())
                    {
                        byte[] decryptionKey = Encoding.ASCII.GetBytes(key).Take(24).ToArray();
                        byte[] iv = new byte[aes.IV.Length];
                        int numBytesToRead = aes.IV.Length;
                        int numBytesRead = 0;
                        while (numBytesToRead > 0)
                        {
                            int n = fileStream.Read(iv, numBytesRead, numBytesToRead);
                            if (n == 0) break;

                            numBytesRead += n;
                            numBytesToRead -= n;
                        }

                        using (CryptoStream cryptoStream = new(
                           fileStream,
                           aes.CreateDecryptor(decryptionKey, iv),
                           CryptoStreamMode.Read))
                        {
                            // By default, the StreamReader uses UTF-8 encoding.
                            // To change the text encoding, pass the desired encoding as the second parameter.
                            // For example, new StreamReader(cryptoStream, Encoding.Unicode).
                            using (StreamReader decryptReader = new(cryptoStream))
                            {
                                var data = default(byte[]);
                                using (var memstream = new MemoryStream())
                                {
                                    decryptReader.BaseStream.CopyTo(memstream);
                                    data = memstream.ToArray();
                                }
                                return data;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }

            return new byte[0];
        }
    }
}