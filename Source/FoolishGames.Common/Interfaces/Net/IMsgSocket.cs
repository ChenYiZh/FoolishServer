using FoolishGames.IO;
using FoolishGames.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Net
{
    /// <summary>
    /// 消息配置的修改接口定义
    /// </summary>
    public interface IMsgSocket
    {
        /// <summary>
        /// 消息偏移值
        /// </summary>
        void SetMessageOffset(int offset);

        /// <summary>
        /// 压缩工具
        /// </summary>
        void SetCompression(ICompression compression);

        /// <summary>
        /// 加密工具
        /// </summary>
        void SetCryptoProvide(ICryptoProvider cryptoProvider);
    }
}
