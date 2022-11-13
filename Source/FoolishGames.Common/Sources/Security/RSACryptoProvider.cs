using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace FoolishGames.Security
{
    /// <summary>
    /// RSA加密类
    /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.security.cryptography.rsacryptoserviceprovider?view=netstandard-2.0</para>
    /// </summary>
    [Obsolete("代码有问题，密钥有问题")]
    public class RSACryptoProvider : ICryptoProvider
    {
        /// <summary>
        /// 非对称性填充
        /// </summary>
        public bool OAEP { get; set; } = true;

        /// <summary>
        /// 密钥
        /// </summary>
        public CspParameters RSAKeyInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="oaep">非对称性填充</param>
        public RSACryptoProvider(string key, bool oaep = true)
        {
            if (!string.IsNullOrEmpty(key))
            {
                RSAKeyInfo = new CspParameters();
                RSAKeyInfo.KeyContainerName = key;
            }
            OAEP = oaep;
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] Encrypt(byte[] data)
        {
            if (RSAKeyInfo == null)
            {
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(rsa.ExportParameters(false));
                    return rsa.Encrypt(data, OAEP);
                }
            }
            else
            {
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(RSAKeyInfo))
                {
                    return rsa.Encrypt(data, OAEP);
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
            if (RSAKeyInfo == null)
            {
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(rsa.ExportParameters(false));
                    return rsa.Decrypt(buffer, OAEP);
                }
            }
            else
            {
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(RSAKeyInfo))
                {
                    return rsa.Decrypt(buffer, OAEP);
                }
            }
        }
    }
}
