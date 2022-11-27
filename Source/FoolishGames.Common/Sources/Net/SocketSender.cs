﻿using FoolishGames.Common;
using FoolishGames.IO;
using FoolishGames.Log;
using FoolishGames.Proxy;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FoolishGames.Net
{
    /// <summary>
    /// 消息发送处理类
    /// </summary>
    public sealed class SocketSender : ISender
    {
        /// <summary>
        /// 套接字管理类
        /// </summary>
        ISocket Socket { get; set; }

        /// <summary>
        /// 发送队列的锁
        /// </summary>
        private readonly object SendableSyncRoot = new object();

        /// <summary>
        /// 待发送的消息列表
        /// </summary>
        private LinkedList<IWorker> WaitToSendMessages = new LinkedList<IWorker>();

        /// <summary>
        /// 消息Id，需要加原子锁
        /// </summary>
        private long messageNumber = DateTime.Now.Ticks;

        /// <summary>
        /// 消息Id
        /// <para>get 返回时会自动 +1</para>
        /// </summary>
        public long MessageNumber
        {
            get
            {
                return Interlocked.Increment(ref messageNumber);
            }
            set
            {
                Interlocked.Exchange(ref messageNumber, value);
            }
        }

        /// <summary>
        /// 消息发送处理类
        /// </summary>
        public SocketSender(ISocket socket)
        {
            Socket = socket;
        }

        /// <summary>
        /// 消息发送
        /// </summary>
        /// <param name="message">发送的消息</param>
        /// <param name="callback">消息回调</param>
        public void Send(IMessageWriter message, SendCallback callback = null)
        {
            CheckIn(message, callback, false);
        }

        /// <summary>
        /// 立即发送消息，会打乱消息顺序。只有类似心跳包这种及时的需要用到。一般使用Send就满足使用
        /// </summary>
        /// <param name="message">发送的消息</param>
        /// <param name="callback">消息回调</param>
        [Obsolete("Only used in important message. This method will confuse the message queue. You can use 'Send' instead.", false)]
        public void SendImmediately(IMessageWriter message, SendCallback callback = null)
        {
            CheckIn(message, callback, true);
        }

        /// <summary>
        /// 内部函数，直接传bytes，会影响数据解析
        /// </summary>
        public void SendBytes(byte[] data, SendCallback callback = null)
        {
            CheckIn(data, callback, false);
        }

        /// <summary>
        /// 内部函数，直接传bytes，会影响数据解析，以及解析顺序
        /// </summary>
        public void SendBytesImmediately(byte[] data, SendCallback callback = null)
        {
            CheckIn(data, callback, true);
        }

        /// <summary>
        /// 挤入消息队列
        /// </summary>
        private void CheckIn(byte[] data, SendCallback callback, bool immediately)
        {
            WaitToSendMessage worker = new WaitToSendMessage(this, data, callback);
            CheckIn(worker, immediately);
        }

        /// <summary>
        /// 挤入消息队列
        /// </summary>
        private void CheckIn(IMessageWriter message, SendCallback callback, bool immediately)
        {
            message.MsgId = MessageNumber;
            byte[] data = PackageFactory.Pack(message, Socket.MessageOffset, Socket.Compression, Socket.CryptoProvider);
            WaitToSendMessage worker = new WaitToSendMessage(this, data, callback);
            CheckIn(worker, immediately);
        }

        /// <summary>
        /// 挤入消息队列
        /// </summary>
        private void CheckIn(WaitToSendMessage worker, bool immediately)
        {
            lock (SendableSyncRoot)
            {
                if (immediately)
                {
                    WaitToSendMessages.AddFirst(worker);
                }
                else
                {
                    WaitToSendMessages.AddLast(worker);
                }
                //当有消息正在执行时，直接退出
                if (WaitToSendMessages.Count > 1)
                {
                    return;
                }
                IWorker execution = WaitToSendMessages.First.Value;
                WaitToSendMessages.RemoveFirst();
                ThreadPool.UnsafeQueueUserWorkItem((state) => { execution.Work(); }, null);
            }
        }

        /// <summary>
        /// 以同步方式发送
        /// </summary>
        private bool Send(byte[] data, out IAsyncResult result)
        {
            if (data == null)
            {
                result = null;
                return false;
            }
            IAsyncResult asyncSend = Socket.Socket.BeginSend(data, 0, data.Length, SocketFlags.None, null, Socket);
            result = asyncSend;
            if (!asyncSend.AsyncWaitHandle.WaitOne(5000, true))
            {
                FConsole.WriteErrorFormatWithCategory(Categories.SOCKET, "Process send error and close socket");
                Socket.Close();
                return false;
            }
            return true;
        }

        /// <summary>
        /// 消息执行完后，判断还有没有需要继续发送的消息
        /// </summary>
        private void SendCompleted()
        {
            lock (SendableSyncRoot)
            {
                //没有消息就退出
                if (WaitToSendMessages.Count == 0)
                {
                    return;
                }
                //有消息就继续执行
                IWorker execution = WaitToSendMessages.First.Value;
                WaitToSendMessages.RemoveFirst();
                execution.Work();
            }
        }

        /// <summary>
        /// 待发送的消息
        /// </summary>
        private struct WaitToSendMessage : IWorker
        {
            /// <summary>
            /// 消息
            /// </summary>
            public byte[] Message;
            /// <summary>
            /// 回调
            /// </summary>
            public SendCallback Callback;
            /// <summary>
            /// 发送接口的套接字
            /// </summary>
            public SocketSender Sender;
            /// <summary>
            /// 构造函数
            /// </summary>
            public WaitToSendMessage(SocketSender sender, byte[] message, SendCallback callback)
            {
                Sender = sender;
                Message = message;
                Callback = callback;
            }
            /// <summary>
            /// 执行函数
            /// </summary>
            public void Work()
            {
                if (Sender.Socket.Connected)
                {
                    IAsyncResult result;
                    try
                    {
                        bool success = Sender.Send(Message, out result);
                        SendCallback callback = Callback;
                        ISocket socket = Sender.Socket;
                        //执行消息回调
                        ThreadPool.UnsafeQueueUserWorkItem((state) =>
                        {
                            socket.MessageEventProcessor.CheckIn(new MessageWorker(callback, success, result));
                        }, null);
                        Sender.SendCompleted();
                    }
                    catch (Exception e)
                    {
                        FConsole.WriteExceptionWithCategory(Categories.SOCKET, e);
                        Sender.Socket.Close();
                    }
                }
            }

            /// <summary>
            /// 消息处理对象
            /// </summary>
            private struct MessageWorker : IWorker
            {
                /// <summary>
                /// 回调
                /// </summary>
                public SendCallback Callback { get; private set; }

                /// <summary>
                /// 处理是否成功
                /// </summary>
                public bool Success { get; private set; }

                /// <summary>
                /// 处理结果
                /// </summary>
                public IAsyncResult Result { get; private set; }

                /// <summary>
                /// 初始化
                /// </summary>
                public MessageWorker(SendCallback callback, bool success, IAsyncResult result)
                {
                    Callback = callback;
                    Success = success;
                    Result = result;
                }

                /// <summary>
                /// 事务处理
                /// </summary>
                public void Work()
                {
                    try
                    {
                        Callback?.Invoke(Success, Result);
                    }
                    catch (Exception e)
                    {
                        FConsole.WriteExceptionWithCategory(Categories.SOCKET, "Send callback execute error.", e);
                    }
                }
            }
        }
    }
}
