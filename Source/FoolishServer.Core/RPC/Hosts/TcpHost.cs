using FoolishServer.Framework.RPC;
using FoolishServer.Framework.RPC.Host;
using FoolishServer.Framework.RPC.Sockets;
using FoolishServer.Framework.Config;
using FoolishServer.Log;
using FoolishServer.RPC.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.RPC.Hosts
{
    public class TcpHost : IHost
    {
        /// <summary>
        /// 是否启动
        /// </summary>
        public bool IsRunning { get; private set; } = false;

        /// <summary>
        /// 标识名称
        /// </summary>
        public string Name { get { return Setting.Name; } }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get { return Setting.Port; } }

        /// <summary>
        /// 类型
        /// </summary>
        public EHostType Type { get { return Setting.Type; } }

        /// <summary>
        /// 监听套接字
        /// </summary>
        public IServerSocket SocketListener { get; private set; }

        /// <summary>
        /// 配置文件
        /// </summary>
        public IHostSetting Setting { get; private set; }

        /// <summary>
        /// 启动结构
        /// </summary>
        public bool Start(IHostSetting setting)
        {
            if (IsRunning) { return false; }
            Setting = setting;
            FConsole.WriteInfoWithCategory(setting.GetCategory(), "Server is starting...", setting.Name);
            try
            {
                if (SocketListener != null)
                {
                    SocketListener.Close();
                }
                SocketListener = new SocketListener();
                SocketListener.Start(setting);
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(setting.GetCategory(), e);
                return false;
            }
            IsRunning = true;
            return true;
        }

        public void Shutdown()
        {
            if (Setting == null)
            {
                return;
            }
            if (SocketListener != null)
            {
                try
                {

                    SocketListener.Close();
                    SocketListener = null;
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Setting.GetCategory(), e);
                }
            }
        }
    }
}
