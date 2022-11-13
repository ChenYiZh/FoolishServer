using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace FoolishGames.IO
{
    /// <summary>
    /// gzip压缩
    /// </summary>
    public class GZipCompression : ICompression
    {
        /// <summary>
        /// 压缩级别
        /// </summary>
        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Fastest;

        /// <summary>
        /// 压缩
        /// </summary>
        public byte[] Compress(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(stream, CompressionLevel))
                {
                    gzip.Write(data, 0, data.Length);
                }
                return stream.GetBuffer();
            }
        }

        /// <summary>
        /// 解压
        /// </summary>
        public byte[] Uncompress(byte[] buffer)
        {
            using (MemoryStream reader = new MemoryStream())
            {
                using (MemoryStream stream = new MemoryStream(buffer))
                {
                    using (GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        byte[] bits = new byte[1024];
                        int length;
                        while ((length = gzip.Read(bits, 0, bits.Length)) > 0)
                        {
                            reader.Write(bits, 0, length);
                        }
                    }
                }
                return reader.GetBuffer();
            }
        }
    }
}
