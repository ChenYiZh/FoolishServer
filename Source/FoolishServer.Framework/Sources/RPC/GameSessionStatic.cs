using FoolishGames.Collections;
using FoolishServer.RPC.Sockets;
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
        internal static ISession CreateNew(Guid keyCode, ISocket socket, IServerSocket server)
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
    }
}
