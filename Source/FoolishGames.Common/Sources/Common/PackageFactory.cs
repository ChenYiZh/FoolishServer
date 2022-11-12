using FoolishGames.IO;
using FoolishGames.Sources.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace FoolishGames.Common
{
    /// <summary>
    /// 消息处理类
    /// </summary>
    public static class PackageFactory
    {
        /// <summary>
        /// 打包
        /// </summary>
        public static byte[] Pack(IMessageWriter message, int offset, bool compress)
        {
            byte[] context = message.GetContext();
            byte[] buffer = new byte[offset + MessageInfo.HeaderLength + context.Length];
            for (int i = 0; i < offset; i++)
            {
                buffer[i] = RandomUtil.RandomByte();
            }
            message.WriteHeader(buffer, offset);
            Buffer.BlockCopy(context, 0, buffer, offset + MessageInfo.HeaderLength, context.Length);
            if (compress)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (GZipStream gzip = new GZipStream(stream, CompressionLevel.Fastest))
                    {
                        gzip.Write(buffer, 0, buffer.Length);
                    }
                    return stream.GetBuffer();
                }
            }
            else
            {
                return buffer;
            }
        }

        /// <summary>
        /// 解包
        /// </summary>
        public static IMessageReader Unpack(byte[] package, int offset, bool uncompress)
        {
            if (uncompress)
            {
                using (MemoryStream reader = new MemoryStream())
                {
                    using (MemoryStream buffer = new MemoryStream(package))
                    {
                        using (GZipStream gzip = new GZipStream(buffer, CompressionMode.Decompress))
                        {
                            byte[] bits = new byte[1024];
                            int length;
                            while ((length = gzip.Read(bits, 0, bits.Length)) > 0)
                            {
                                reader.Write(bits, 0, length);
                            }
                        }
                    }
                    package = reader.GetBuffer();
                }
            }
            return new MessageReader(package, offset);
        }
    }
}
