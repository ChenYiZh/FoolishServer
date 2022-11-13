using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.IO
{
    /// <summary>
    /// 压缩接口
    /// </summary>
    public interface ICompression
    {
        /// <summary>
        /// 压缩
        /// </summary>
        byte[] Compress(byte[] data);

        /// <summary>
        /// 解压缩
        /// </summary>
        byte[] Uncompress(byte[] buffer);
    }
}
