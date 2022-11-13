using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Security
{
    /// <summary>
    /// 加密类
    /// </summary>
    public interface ICryptoProvider
    {
        /// <summary>
        /// 加密
        /// </summary>
        byte[] Encrypt(byte[] data);

        /// <summary>
        /// 解密
        /// </summary>
        byte[] Decrypt(byte[] buffer);
    }
}
