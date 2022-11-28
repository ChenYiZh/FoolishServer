using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace FoolishGames.Net
{
    /// <summary>
    /// 寄宿在原生Socket的管理类
    /// </summary>
    internal sealed class UserToken : IUserToken
    {
        /// <summary>
        /// 嵌套的Socket
        /// </summary>
        public ISocket Socket { get; internal set; }

        ///// <summary>
        ///// 套接字处理结果
        ///// </summary>
        //public SocketAsyncResult AsyncResult { get; internal set; }

        /// <summary>
        /// 已经接收的数据长度
        /// </summary>
        public int ReceivedStartIndex { get; set; } = 0;

        /// <summary>
        /// 解析包时解析不完的数据
        /// </summary>
        public byte[] ReceivedBuffer { get; internal set; } = null;

        /// <summary>
        /// 正在发送的数据
        /// </summary>
        public byte[] SendingBuffer { get; internal set; }

        /// <summary>
        /// 已经发送的字节数量
        /// </summary>
        public int SendedCount { get; internal set; }

        /// <summary>
        /// 原本的生成时的offset
        /// </summary>
        public int OriginalOffset { get; private set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public UserToken(SocketAsyncEventArgs eventArgs)
        {
            OriginalOffset = eventArgs.Offset;
            ReceivedBuffer = null;
        }

        ///// <summary>
        ///// 析构时执行
        ///// </summary>
        //~UserToken()
        //{
        //    ReceiverStream.Dispose();
        //    ReceiverStream = null;
        //}

        /// <summary>
        /// 重置数据
        /// </summary>
        public void Reset()
        {
            SendingBuffer = null;
            ReceivedStartIndex = 0;
            ReceivedBuffer = null;
            //ReceiverStream.Flush();
            //AsyncResult = null;
            Socket = null;
        }

        /// <summary>
        /// 设置原本的生成时的offset
        /// </summary>
        internal void SetOriginalOffset(int offset)
        {
            OriginalOffset = offset;
        }
    }
}
