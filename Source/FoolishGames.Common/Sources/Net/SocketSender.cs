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

using FoolishGames.Common;
using FoolishGames.IO;
using FoolishGames.Log;
using FoolishGames.Proxy;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using FoolishGames.Collections;

namespace FoolishGames.Net
{
    /// <summary>
    /// 消息发送处理类
    /// </summary>
    public sealed class SocketSender //: ISender
    {
        /// <summary>
        /// 套接字管理类
        /// </summary>
        ISocket Socket { get; set; }
        //
        // /// <summary>
        // /// 增强类
        // /// </summary>
        // public SocketAsyncEventArgs EventArgs
        // {
        //     get { return Socket.EventArgs; }
        // }
        //
        // /// <summary>
        // /// 数据管理对象
        // /// </summary>
        // internal UserToken UserToken { get; private set; }

        // /// <summary>
        // /// 发送队列的锁
        // /// </summary>
        // private readonly object _sendableSyncRoot = new object();


        /// <summary>
        /// 消息发送处理类
        /// </summary>
        public SocketSender(ISocket socket)
        {
            Socket = socket;
            // if (socket.EventArgs == null)
            // {
            //     throw new NullReferenceException(
            //         "Fail to create socket sender, because the SocketAsyncEventArgs is null.");
            // }
            //
            // UserToken usertoken = socket.UserToken;
            // if (usertoken == null)
            // {
            //     throw new NullReferenceException(
            //         "Fail to create socket sender, because the UserToken of SocketAsyncEventArgs is null.");
            // }
            //
            // UserToken = usertoken;
        }

        // /// <summary>
        // /// 消息发送
        // /// </summary>
        // /// <param name="message">发送的消息</param>
        // public void Send(IMessageWriter message /*, SendCallback callback = null*/)
        // {
        //     CheckIn(message, false);
        // }
        //
        // /// <summary>
        // /// 立即发送消息，会打乱消息顺序。只有类似心跳包这种及时的需要用到。一般使用Send就满足使用
        // /// </summary>
        // /// <param name="message">发送的消息</param>
        // [Obsolete(
        //     "Only used in important message. This method will confuse the message queue. You can use 'Send' instead.",
        //     false)]
        // public void SendImmediately(IMessageWriter message /*, SendCallback callback = null*/)
        // {
        //     CheckIn(message, true);
        // }
        //
        // /// <summary>
        // /// 内部函数，直接传bytes，会影响数据解析
        // /// </summary>
        // public void SendBytes(byte[] data /*, SendCallback callback = null*/)
        // {
        //     CheckIn(data, false);
        // }
        //
        // /// <summary>
        // /// 内部函数，直接传bytes，会影响数据解析，以及解析顺序
        // /// </summary>
        // public void SendBytesImmediately(byte[] data /*, SendCallback callback = null*/)
        // {
        //     CheckIn(data, true);
        // }
        //
        // /// <summary>
        // /// 挤入消息队列
        // /// </summary>
        // private void CheckIn(byte[] data, /*SendCallback callback, */bool immediately)
        // {
        //     WaitToSendMessage worker = new WaitToSendMessage(this, data);
        //     CheckIn(worker, immediately);
        // }
        //
        // /// <summary>
        // /// 挤入消息队列
        // /// </summary>
        // private void CheckIn(IMessageWriter message, /* SendCallback callback,*/ bool immediately)
        // {
        //     message.MsgId = MessageNumber;
        //     byte[] data = PackageFactory.Pack(message, Socket.MessageOffset, Socket.Compression, Socket.CryptoProvider);
        //     WaitToSendMessage worker = new WaitToSendMessage(this, data);
        //     CheckIn(worker, immediately);
        // }

        public void Push(ISocket socket, byte[] msg, bool immediate)
        {
            ((UserToken) socket.EventArgs.UserToken).Push(msg, immediate);
            // if (Socket.TrySend(true))
            // {
            //     ThreadPool.UnsafeQueueUserWorkItem((state) => { PostSend(socket.EventArgs); }, null);
            // }
        }

        // /// <summary>
        // /// 挤入消息队列
        // /// </summary>
        // private void CheckIn(WaitToSendMessage worker, bool immediately)
        // {
        //     // lock (_sendableSyncRoot)
        //     // {
        //     if (immediately)
        //     {
        //         _waitToSendMessages.AddFirst(worker);
        //     }
        //     else
        //     {
        //         _waitToSendMessages.AddLast(worker);
        //     }
        //
        //     //未连接时返回
        //     if (!Socket.Connected)
        //     {
        //         return;
        //     }
        //
        //     ThreadPool.UnsafeQueueUserWorkItem((state) => { BeginSend(); }, null);
        //     //}
        // }

        // /// <summary>
        // /// 最后的消息推送
        // /// </summary>
        // public void Post(byte[] data)
        // {
        //     if (data == null)
        //     {
        //         return;
        //     }
        //
        //     UserToken.SendingBuffer = data;
        //     UserToken.SendedCount = 0;
        //     ProcessSend();
        // }

        /// <summary>
        /// 消息发送处理
        /// </summary>
        public void ProcessSend(SocketAsyncEventArgs ioEventArgs)
        {
            if (ioEventArgs == null)
            {
                //return true;
                Socket.OperationCompleted();
                return;
            }

            UserToken userToken = (UserToken) ioEventArgs.UserToken;
            if (userToken.SendingBuffer == null)
            {
                //if (UserToken.IsSending)
                //{
                //    return true;
                //}
                SendCompleted(ioEventArgs);
                return;
            }
            
            // lock (EventArgs)
            // {
            byte[] argsBuffer = ioEventArgs.Buffer;
            int argsCount = ioEventArgs.Count;
            int argsOffset = ioEventArgs.Offset;
            FConsole.Write($"Send: ({userToken.SendingBuffer.Length}, {userToken.SendedCount}), Thread: {Thread.CurrentThread.ManagedThreadId}");
            if (argsCount >= userToken.SendingBuffer.Length - userToken.SendedCount)
            {
                int length = userToken.SendingBuffer.Length - userToken.SendedCount;
                Buffer.BlockCopy(userToken.SendingBuffer, userToken.SendedCount, argsBuffer, argsOffset, length);
                ioEventArgs.SetBuffer(argsOffset, length);
                userToken.Reset();
            }
            else
            {
                Buffer.BlockCopy(userToken.SendingBuffer, userToken.SendedCount, argsBuffer, argsOffset, argsCount);
                ioEventArgs.SetBuffer(argsOffset, argsCount);
                userToken.SendedCount += argsCount;
            }
            //}

            if (Socket == null || Socket.Socket == null)
            {
                //return false;
                Socket.OperationCompleted();
                return;
            }

            if (!ioEventArgs.AcceptSocket.SendAsync(ioEventArgs))
            {
                ProcessSend(ioEventArgs);
            }
        }

        /// <summary>
        /// 开始执行发送
        /// </summary>
        public void PostSend(SocketAsyncEventArgs ioEventArgs)
        {
            UserToken usertoken = ioEventArgs.UserToken as UserToken;
            if (usertoken == null)
            {
                Socket.OperationCompleted();
                return;
            }

            //没有消息就退出
            if (Socket.TrySend())
            {
                byte[] msg;
                if (usertoken.TryDequeueMsg(out msg))
                {
                    usertoken.SendingBuffer = msg;
                    usertoken.SendedCount = 0;
                    ProcessSend(ioEventArgs);
                }
                else
                {
                    Socket.NextStep(ioEventArgs);
                }
            }
        }

        /// <summary>
        /// 消息执行完后，判断还有没有需要继续发送的消息
        /// </summary>
        private void SendCompleted(SocketAsyncEventArgs ioEventArgs)
        {
            // ((UserToken) ioEventArgs.UserToken).ResetSendAndReceiveState();
            // PostSend(ioEventArgs);
            //没有消息就退出
            // if (_waitToSendMessages.Count == 0)
            // {
            //     // try //防回收
            //     // {
            //     //     EventArgs.SetBuffer(EventArgs.Offset, UserToken.OriginalLength);
            //     // }
            //     // catch
            //     // {
            //     // }
            //
            //     return;
            // }
            //
            // BeginSend();
            ioEventArgs.SetBuffer(ioEventArgs.Offset, ((UserToken)ioEventArgs.UserToken).OriginalLength);
            Socket.NextStep(ioEventArgs);
        }

        // /// <summary>
        // /// 待发送的消息
        // /// </summary>
        // private struct WaitToSendMessage : IWorker
        // {
        //     /// <summary>
        //     /// 消息
        //     /// </summary>
        //     public byte[] Message;
        //
        //     /// <summary>
        //     /// 发送接口的套接字
        //     /// </summary>
        //     public SocketSender Sender;
        //
        //     /// <summary>
        //     /// 构造函数
        //     /// </summary>
        //     public WaitToSendMessage(SocketSender sender, byte[] message)
        //     {
        //         Sender = sender;
        //         Message = message;
        //     }
        //
        //     /// <summary>
        //     /// 执行函数
        //     /// </summary>
        //     public void Work()
        //     {
        //         if (Sender.Socket.Connected)
        //         {
        //             //IAsyncResult result;
        //             FConsole.Write(Thread.CurrentThread.ManagedThreadId.ToString());
        //             try
        //             {
        //                 Sender.Post(Message);
        //             }
        //             catch (Exception e)
        //             {
        //                 FConsole.WriteExceptionWithCategory(Categories.SOCKET, e);
        //                 Sender.Socket.Close();
        //             }
        //         }
        //     }
        //
        //     ///// <summary>
        //     ///// 消息处理对象
        //     ///// </summary>
        //     //private struct MessageWorker : IWorker
        //     //{
        //     //    /// <summary>
        //     //    /// 回调
        //     //    /// </summary>
        //     //    public SendCallback Callback { get; private set; }
        //
        //     //    /// <summary>
        //     //    /// 处理是否成功
        //     //    /// </summary>
        //     //    public bool Success { get; private set; }
        //
        //     //    /// <summary>
        //     //    /// 处理结果
        //     //    /// </summary>
        //     //    public IAsyncResult Result { get; private set; }
        //
        //     //    /// <summary>
        //     //    /// 初始化
        //     //    /// </summary>
        //     //    public MessageWorker(SendCallback callback, bool success, IAsyncResult result)
        //     //    {
        //     //        Callback = callback;
        //     //        Success = success;
        //     //        Result = result;
        //     //    }
        //
        //     //    /// <summary>
        //     //    /// 事务处理
        //     //    /// </summary>
        //     //    public void Work()
        //     //    {
        //     //        try
        //     //        {
        //     //            Callback?.Invoke(Success, Result);
        //     //        }
        //     //        catch (Exception e)
        //     //        {
        //     //            FConsole.WriteExceptionWithCategory(Categories.SOCKET, "Send callback execute error.", e);
        //     //        }
        //     //    }
        //     //}
        // }
    }
}