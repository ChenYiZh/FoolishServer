using FoolishGames.IO;
using FoolishGames.Security;
using FoolishGames.Sources.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace FoolishGames.Common
{
    /// <summary>
    /// 消息类型
    /// </summary>
    internal enum EMessageType : byte
    {
        /// <summary>
        /// 什么都不处理
        /// </summary>
        NoProcess = 0,
        /// <summary>
        /// 压缩过
        /// </summary>
        OnlyCompress = 10,
        /// <summary>
        /// 加密过
        /// </summary>
        OnlyCrypto = 15,
        /// <summary>
        /// 解压+加密
        /// </summary>
        CompressAndCrypto = 50,
    }
    /// <summary>
    /// 消息处理类
    /// </summary>
    public static class PackageFactory
    {
        /// <summary>
        /// 打包
        /// </summary>
        public static byte[] Pack(IMessageWriter message, int offset, ICompression compression, ICryptoProvider cryptography)
        {
            byte[] context = message.GetContext();
            byte[] buffer = new byte[offset + MessageInfo.HeaderLength + message.ContextLength];
            for (int i = 0; i < offset; i++)
            {
                buffer[i] = RandomUtil.RandomByte();
            }
            message.WriteHeader(buffer, offset);
            Buffer.BlockCopy(context, 0, buffer, offset + MessageInfo.HeaderLength, message.ContextLength);
            bool compress = message.Compress && compression != null;
            bool crypto = message.Secret && cryptography != null;
            EMessageType type = EMessageType.NoProcess;
            if (compress && crypto)
            {
                type = EMessageType.CompressAndCrypto;
            }
            else if (compress)
            {
                type = EMessageType.OnlyCompress;
            }
            else if (crypto)
            {
                type = EMessageType.OnlyCrypto;
            }
            if (crypto)
            {
                buffer = cryptography.Encrypt(buffer);
            }
            if (compress)
            {
                buffer = compression.Compress(buffer);
            }
            byte[] result = new byte[buffer.Length + 1];
            result[0] = (byte)type;
            Buffer.BlockCopy(buffer, 0, result, 1, buffer.Length);
            return result;
        }

        /// <summary>
        /// 解包
        /// </summary>
        public static IMessageReader Unpack(byte[] package, int offset, ICompression compression, ICryptoProvider cryptography)
        {
            byte[] data = new byte[package.Length - 1];
            Buffer.BlockCopy(package, 1, data, 0, data.Length);
            EMessageType type = (EMessageType)package[0];
            bool compress = type == EMessageType.OnlyCompress || type == EMessageType.CompressAndCrypto;
            bool crypto = type == EMessageType.OnlyCrypto || type == EMessageType.CompressAndCrypto;
            if (compress)
            {
                data = compression.Uncompress(data);
            }
            if (crypto)
            {
                data = cryptography.Decrypt(data);
            }
            return new MessageReader(data, offset, compress, crypto);
        }
    }
}
