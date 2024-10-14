/****************************************************************************
THIS FILE IS PART OF Foolish Server PROJECT
THIS PROGRAM IS FREE SOFTWARE, IS LICENSED UNDER MIT

Copyright (c) 2022-2030 ChenYiZh
https://space.bilibili.com/9308172

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
****************************************************************************/
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
        private int _isSendingOrReceivingFlag = 0;

        /// <summary>
        /// 是否是待处理状态
        /// </summary>
        internal bool IsWaitingSendOrReceive
        {
            get { return Interlocked.CompareExchange(ref _isSendingOrReceivingFlag, 0, 0) == 0; }
        }

        /// <summary>
        /// 是否正在发送数据
        /// </summary>
        internal bool IsSending
        {
            get { return Interlocked.CompareExchange(ref _isSendingOrReceivingFlag, 1, 1) == 1; }
        }

        /// <summary>
        /// 是否正在接收数据
        /// </summary>
        internal bool IsReceiving
        {
            get { return Interlocked.CompareExchange(ref _isSendingOrReceivingFlag, 2, 2) == 2; }
        }

        /// <summary>
        /// 判断是否可以发送，如果可以则切换至发送状态
        /// </summary>
        internal bool Sendable()
        {
            return Interlocked.CompareExchange(ref _isSendingOrReceivingFlag, 1, 0) == 0 || IsSending;
        }

        /// <summary>
        /// 判断是否可以接收，如果可以则切换至接收状态
        /// </summary>
        internal bool Receivable()
        {
            return Interlocked.CompareExchange(ref _isSendingOrReceivingFlag, 2, 0) == 0 || IsReceiving;
        }

        /// <summary>
        /// 重置收发标识
        /// </summary>
        internal void ResetSendOrReceiveState(int fromState)
        {
            if (fromState == 0)
            {
                Interlocked.Exchange(ref _isSendingOrReceivingFlag, 0);
            }
            else
            {
                Interlocked.CompareExchange(ref _isSendingOrReceivingFlag, 0, fromState);
            }
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
            //Socket = null;
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
