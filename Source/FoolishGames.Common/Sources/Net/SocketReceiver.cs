/****************************************************************************
THIS FILE IS PART OF Foolish Server PROJECT
THIS PROGRAM IS FREE SOFTWARE, IS LICENSED UNDER MIT

Copyright (c) 2022-2025 ChenYiZh
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
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FoolishGames.Net
{
    /// <summary>
    /// 消息接收处理类
    /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socketasynceventargs</para>
    /// </summary>
    public sealed class SocketReceiver<TSocket> : IReceiver where TSocket : ISocket
    {
        /// <summary>
        /// 收发消息处理
        /// </summary>
        public delegate void MessageReceiveEventHandler(IMessageEventArgs<TSocket> e);
        /// <summary>
        /// 封装的套接字
        /// </summary>
        public TSocket Socket { get; private set; }
        /// <summary>
        /// 增强类
        /// </summary>
        public SocketAsyncEventArgs EventArgs { get { return Socket.EventArgs; } }
        /// <summary>
        /// 数据管理对象
        /// </summary>
        internal UserToken UserToken { get; private set; }
        /// <summary>
        /// 接收到数据包事件
        /// </summary>
        public MessageReceiveEventHandler OnMessageReceived;
        private void MessageReceived(MessageReceiverEventArgs<TSocket> args) { OnMessageReceived?.Invoke(args); }

        /// <summary>
        /// 心跳探索事件
        /// </summary>
        public MessageReceiveEventHandler OnPing;
        private void Ping(MessageReceiverEventArgs<TSocket> args) { OnPing?.Invoke(args); }

        /// <summary>
        /// 心跳回应事件
        /// </summary>
        public MessageReceiveEventHandler OnPong;
        private void Pong(MessageReceiverEventArgs<TSocket> args) { OnPong?.Invoke(args); }
        /// <summary>
        /// 初始化
        /// </summary>
        public SocketReceiver(TSocket socket)
        {
            Socket = socket;
            if (socket.EventArgs == null) { throw new NullReferenceException("Fail to create socket receiver, because the SocketAsyncEventArgs is null."); }
            UserToken usertoken = socket.EventArgs.UserToken as UserToken;
            if (usertoken == null) { throw new NullReferenceException("Fail to create socket receiver, because the UserToken of SocketAsyncEventArgs is null."); }
            UserToken = usertoken;
        }
        /// <summary>
        /// 等待消息接收
        /// </summary>
        /// <returns>是否维持等待状态</returns>
        public bool BeginReceive(bool force = false)
        {
            if (Socket == null || Socket.Socket == null || !Socket.Connected)
            {
                FConsole.WriteWarnWithCategory(Categories.SOCKET, "System.Net.Socket is abnormal, and display disconnected.");
                Socket?.Close();
                return false;
            }
            if (EventArgs == null) { return false; }
            if (UserToken.IsReceiving) { return false; }
            if (UserToken.Receivable())
            {
                if (!force && Socket.Socket.Available == 0)
                {
                    UserToken.ResetSendOrReceiveState(2);
                    return true;
                }
                ThreadPool.UnsafeQueueUserWorkItem((state) =>
                {
                    try
                    {

                        //线程中需要重新加锁加判断
                        //https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socket.receiveasync?view=netstandard-2.0
                        if (/*UserToken.IsReceiving && */UserToken.Receivable()
                        && (force || Socket.Socket.Available > 0)
                        && Socket.EventArgs != null)
                        {
                            bool willRaiseEvent = true;
                            lock (Socket.EventArgs)
                            {
                                willRaiseEvent = Socket.Socket.ReceiveAsync(Socket.EventArgs);
                            }
                            if (!willRaiseEvent)
                            {
                                ProcessReceive();
                            }
                        }
                    }
                    catch /*(Exception e)*/
                    {
                        Socket.Close();
                    }
                }, null);
                return false;
            }
            return false;
        }

        /// <summary>
        /// 处理数据接收回调
        /// </summary>
        public bool ProcessReceive()
        {
            if (EventArgs == null || Socket.Socket == null) { return false; }
            if (EventArgs.BytesTransferred == 0)
            {
                Socket.Close(EOpCode.Empty);
                return false;
            }
            if (EventArgs.SocketError != SocketError.Success)
            {
                FConsole.WriteErrorFormatWithCategory(Categories.SOCKET,
                    "Process Receive IP {0} SocketError:{1}, bytes len:{2}",
                    (UserToken != null ? Socket.Address?.ToString() : ""),
                    EventArgs.SocketError.ToString(),
                    EventArgs.BytesTransferred);
                Socket.Close();
                return false;
            }

            //Process Receive
            if (EventArgs.BytesTransferred > 0)
            {
                //先缓存数据
                byte[] buffer = new byte[EventArgs.BytesTransferred];
                lock (EventArgs)
                {
                    Buffer.BlockCopy(EventArgs.Buffer, EventArgs.Offset, buffer, 0, buffer.Length);
                }
                //byte[] argsBuffer = EventArgs.Buffer;
                //int argsOffset = EventArgs.Offset;
                //int argsLength = EventArgs.BytesTransferred;
                byte[] argsBuffer = buffer;
                int argsOffset = 0;
                int argsLength = buffer.Length;

                //从当前位置数据开始解析
                int offset = argsOffset;

                //消息处理的队列
                List<IMessageReader> messages = new List<IMessageReader>();
                try
                {
                    //继续接收上次未接收完毕的数据
                    if (UserToken.ReceivedBuffer != null)
                    {
                        //上次连报头都没接收完
                        if (UserToken.ReceivedStartIndex < 0)
                        {
                            byte[] data = new byte[argsLength + UserToken.ReceivedBuffer.Length];
                            Buffer.BlockCopy(UserToken.ReceivedBuffer, 0, data, 0, UserToken.ReceivedBuffer.Length);
                            Buffer.BlockCopy(argsBuffer, argsOffset, data, UserToken.ReceivedBuffer.Length, argsLength);
                            UserToken.ReceivedBuffer = null;

                            argsBuffer = data;
                            offset = argsOffset = 0;
                            argsLength = data.Length;
                        }
                        //数据仍然接收不完
                        else if (UserToken.ReceivedStartIndex + argsLength < UserToken.ReceivedBuffer.Length)
                        {
                            Buffer.BlockCopy(argsBuffer, argsOffset, UserToken.ReceivedBuffer, UserToken.ReceivedStartIndex, argsLength);
                            UserToken.ReceivedStartIndex += argsLength;
                            offset += argsLength;

                            //buffer = null;
                        }
                        //这轮数据可以接受完
                        else
                        {
                            int deltaLength = UserToken.ReceivedBuffer.Length - UserToken.ReceivedStartIndex;
                            Buffer.BlockCopy(argsBuffer, argsOffset, UserToken.ReceivedBuffer, UserToken.ReceivedStartIndex, deltaLength);
                            IMessageReader bigMessage = PackageFactory.Unpack(UserToken.ReceivedBuffer, Socket.MessageOffset, Socket.Compression, Socket.CryptoProvider);
                            UserToken.ReceivedBuffer = null;
                            messages.Add(bigMessage);
                            offset += deltaLength;
                            //if (offset >= argsLength)
                            //{
                            //    buffer = null;
                            //}
                            //else
                            //{
                            //    buffer = new byte[argsLength];
                            //    Buffer.BlockCopy(argsBuffer, argsOffset, buffer, 0, argsLength);
                            //}
                        }
                    }
                    //else
                    //{
                    //    buffer = new byte[argsLength];
                    //    Buffer.BlockCopy(argsBuffer, argsOffset, buffer, 0, argsLength);
                    //}

                    //针对接收到的数据进行完整解析
                    while (offset - argsOffset < argsLength)
                    {
                        int totalLength = PackageFactory.GetTotalLength(argsBuffer, offset + Socket.MessageOffset);
                        //包头解析不全
                        if (totalLength < 0)
                        {
                            UserToken.ReceivedStartIndex = -1;
                            //UserToken.ReceivedBuffer = new byte[buffer.Length - offset];
                            UserToken.ReceivedBuffer = new byte[argsLength + argsOffset - offset];
                            Buffer.BlockCopy(argsBuffer, offset, UserToken.ReceivedBuffer, 0, UserToken.ReceivedBuffer.Length);
                            break;
                        }

                        //包体解析不全
                        if (totalLength > argsLength)
                        {
                            UserToken.ReceivedStartIndex = argsLength + argsOffset - offset;
                            UserToken.ReceivedBuffer = new byte[totalLength - offset];
                            Buffer.BlockCopy(argsBuffer, offset, UserToken.ReceivedBuffer, 0, totalLength - offset);
                            break;
                        }

                        offset += Socket.MessageOffset;
                        IMessageReader message = PackageFactory.Unpack(argsBuffer, offset, Socket.Compression, Socket.CryptoProvider);
                        messages.Add(message);
                        offset = totalLength;
                    }
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.SOCKET, "Process Receive error.", e);
                }

                for (int i = 0; i < messages.Count; i++)
                {
                    IMessageReader message = messages[i];
                    try
                    {
                        if (message.IsError)
                        {
                            FConsole.WriteErrorFormatWithCategory(Categories.SOCKET, message.Error);
                            continue;
                        }
                        switch (message.OpCode)
                        {
                            case (sbyte)EOpCode.Close:
                                {
                                    // TODO: 检查关闭协议是否有效
                                    //Close(ioEventArgs, EOpCode.Empty);
                                    Socket.Close(EOpCode.Empty);
                                }
                                break;
                            case (sbyte)EOpCode.Ping:
                                {
                                    Ping(new MessageReceiverEventArgs<TSocket> { Socket = Socket, Message = message });
                                }
                                break;
                            case (sbyte)EOpCode.Pong:
                                {
                                    Pong(new MessageReceiverEventArgs<TSocket> { Socket = Socket, Message = message });
                                }
                                break;
                            default:
                                {
                                    MessageReceived(new MessageReceiverEventArgs<TSocket> { Socket = Socket, Message = message });
                                }
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        FConsole.WriteExceptionWithCategory(Categories.SOCKET, "An exception occurred when resolve the message.", e);
                    }
                }
            }
            else if (UserToken.ReceivedBuffer != null)
            {
                //数据错乱
                UserToken.ReceivedBuffer = null;
                return false;
            }

            if (Socket.Socket == null || Socket.Socket.Available == 0)
            {
                UserToken.ResetSendOrReceiveState(2);
                return false;
            }
            //https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socket.receiveasync?view=netstandard-2.0
            bool willRaiseEvent = true;
            if (Socket != null)
            {
                lock (Socket.EventArgs)
                {
                    willRaiseEvent = Socket.Socket.ReceiveAsync(Socket.EventArgs);
                }
                if (!willRaiseEvent)
                {
                    ProcessReceive();
                }
            }
            //延迟关闭
            if (Socket == null || !Socket.IsRunning)
            {
                //ResetSocketAsyncEventArgs(ioEventArgs);
                Socket?.Close();
            }
            return true;
        }
    }
}
