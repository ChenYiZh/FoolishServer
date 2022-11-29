using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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

        #region socket state
        /// <summary>
        /// 收发数据的标识
        /// <para>0: 无状态; 1: 接收状态; 2: 发送状态</para>
        /// </summary>
        private int isSendingOrReceivingFlag = 0;

        /// <summary>
        /// 是否是待处理状态
        /// </summary>
        internal bool IsWaitingSendOrReceive
        {
            get { return Interlocked.CompareExchange(ref isSendingOrReceivingFlag, 0, 0) == 0; }
        }

        /// <summary>
        /// 是否正在发送数据
        /// </summary>
        internal bool IsSending
        {
            get { return Interlocked.CompareExchange(ref isSendingOrReceivingFlag, 1, 1) == 1; }
        }

        /// <summary>
        /// 是否正在接收数据
        /// </summary>
        internal bool IsReceiving
        {
            get { return Interlocked.CompareExchange(ref isSendingOrReceivingFlag, 2, 2) == 2; }
        }

        /// <summary>
        /// 判断是否可以发送，如果可以则切换至发送状态
        /// </summary>
        internal bool Sendable()
        {
            return Interlocked.CompareExchange(ref isSendingOrReceivingFlag, 1, 0) == 0 || IsSending;
        }

        /// <summary>
        /// 判断是否可以接收，如果可以则切换至接收状态
        /// </summary>
        internal bool Receivable()
        {
            return Interlocked.CompareExchange(ref isSendingOrReceivingFlag, 2, 0) == 0 || IsReceiving;
        }

        /// <summary>
        /// 重置收发标识
        /// </summary>
        internal void ResetSendOrReceiveState(int fromState)
        {
            Interlocked.CompareExchange(ref isSendingOrReceivingFlag, 0, fromState);
        }
        #endregion

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
        /// 原本缓存的字节长度
        /// </summary>
        public int OriginalLength { get; private set; }

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
        internal void SetOriginalOffset(int offset, int length)
        {
            OriginalOffset = offset;
            OriginalLength = length;
        }
    }
}
