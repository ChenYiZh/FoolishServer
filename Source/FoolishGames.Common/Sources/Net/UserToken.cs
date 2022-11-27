using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Net
{
    /// <summary>
    /// 寄宿在原生Socket的管理类
    /// </summary>
    internal class UserToken
    {
        /// <summary>
        /// 嵌套的Socket
        /// </summary>
        public ISocket Socket { get; internal set; }

        /// <summary>
        /// 套接字处理结果
        /// </summary>
        public SocketAsyncResult AsyncResult { get; internal set; }

        /// <summary>
        /// 已经接收的数据长度
        /// </summary>
        public int TempStartIndex { get; set; } = 0;

        /// <summary>
        /// 解析包时解析不完的数据
        /// </summary>
        public byte[] TempBuffer { get; set; } = null;

        /// <summary>
        /// 重置数据
        /// </summary>
        public void Reset()
        {
            TempBuffer = null;
            AsyncResult = null;
        }
    }
}
