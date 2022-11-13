using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FoolishGames.Security
{
    /// <summary>
    /// AES加密
    /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.security.cryptography.aescryptoserviceprovider?view=netstandard-2.0</para>
    /// </summary>
    public class AESCryptoProvider : ICryptoProvider
    {
        private const int KEY_LENGTH = 16;

        private byte[] key = new byte[KEY_LENGTH];

        private byte[] iv = new byte[KEY_LENGTH];

        public string Key
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("The key of AESCryptoProvider is null.");
                }
                byte[] bits = Encoding.UTF8.GetBytes(value);
                Buffer.BlockCopy(bits, 0, key, 0, bits.Length < KEY_LENGTH ? bits.Length : KEY_LENGTH);
            }
        }

        public string IV
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("The iv of AESCryptoProvider is null.");
                }
                byte[] bits = Encoding.UTF8.GetBytes(value);
                Buffer.BlockCopy(bits, 0, iv, 0, bits.Length < KEY_LENGTH ? bits.Length : KEY_LENGTH);
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        public AESCryptoProvider(string key, string iv)
        {
            Key = key;
            IV = iv;
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] Encrypt(byte[] data)
        {
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.Zeros;
                using (MemoryStream stream = new MemoryStream())
                {
                    using (CryptoStream crypto = new CryptoStream(stream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        crypto.Write(data, 0, data.Length);
                    }
                    return stream.GetBuffer();
                }
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public byte[] Decrypt(byte[] buffer)
        {
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.Zeros;
                using (MemoryStream stream = new MemoryStream())
                {
                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        using (CryptoStream crypto = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            byte[] reader = new byte[1024];
                            int length;
                            while ((length = crypto.Read(reader, 0, reader.Length)) > 0)
                            {
                                stream.Write(reader, 0, length);
                            }
                        }
                    }
                    return stream.GetBuffer();
                }
            }
        }
    }
}
