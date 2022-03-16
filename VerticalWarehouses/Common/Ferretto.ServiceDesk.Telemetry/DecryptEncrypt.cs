using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Ferretto.ServiceDesk.Telemetry
{
    public static class DecryptEncrypt
    {
        #region Methods

        public static string Decrypt(string cipherText, string salt)
        {
            var iv = new byte[16];
            Buffer.BlockCopy(Convert.FromBase64String(cipherText), 0, iv, 0, iv.Length);
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Mode = CipherMode.CBC;
                using (var decryptor = aesAlg.CreateDecryptor(Convert.FromBase64String(salt), iv))
                {
                    byte[] encrypted = Convert.FromBase64String(cipherText);
                    using (var msDecrypt = new MemoryStream(encrypted, iv.Length, encrypted.Length - iv.Length))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var resultStream = new MemoryStream())
                            {
                                csDecrypt.CopyTo(resultStream);
                                return System.Text.Encoding.UTF8.GetString(resultStream.ToArray());
                            }
                        }
                    }
                }
            }
        }

        public static string Encrypt(string clearText, string salt)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Mode = CipherMode.CBC;
                using (var encryptor = aesAlg.CreateEncryptor(Convert.FromBase64String(salt), aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            byte[] data = System.Text.Encoding.UTF8.GetBytes(clearText);
                            csEncrypt.Write(data, 0, data.Length);
                        }
                        var ret = msEncrypt.ToArray();
                        return Convert.ToBase64String(ret);
                    }
                }
            }
        }

        public static string GeneratePasswordHash(string password, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);

            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hashed;
        }

        public static byte[] GeneratePasswordSalt()
        {
            var salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return salt;
        }

        #endregion
    }
}
