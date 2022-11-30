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
using FoolishGames.Collections;
using FoolishGames.IO;
using FoolishServer.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.RPC
{
    /// <summary>
    /// 会话窗口
    /// </summary>
    public partial class GameSession
    {
        /// <summary>
        /// 会话窗口池
        /// </summary>
        private static IDictionary<Guid, ISession> sessions;

        /// <summary>
        /// 静态初始化
        /// </summary>
        static GameSession()
        {
            sessions = new ThreadSafeDictionary<Guid, ISession>();
        }

        /// <summary>
        /// 创建会话窗口
        /// </summary>
        internal static ISession CreateNew(Guid keyCode, IRemoteSocket socket, IServerSocket server)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket", "session create failed because the socket is null");
            }
            if (server == null)
            {
                throw new ArgumentNullException("server", "session create failed because the server socket is null.");
            }
            GameSession session = new GameSession(keyCode, socket, server);
            sessions[keyCode] = session;
            return session;
        }

        /// <summary>
        /// 通过Guid获取Session
        /// </summary>
        public static ISession Get(Guid? KeyCode)
        {
            ISession session;
            return KeyCode != null && sessions.TryGetValue(KeyCode.Value, out session) ? session : null;
        }

        /// <summary>
        /// 异步给一堆客户端发消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessions"></param>
        public static void SendAsync(IMessageWriter message, IEnumerable<GameSession> sessions)
        {
            foreach (GameSession session in sessions)
            {
                session.Send(message);
            }
        }
    }
}
