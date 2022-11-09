using FoolishServer.Framework.RPC.Sockets;
using FoolishServer.Framework.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Framework.RPC.Host
{
    public interface IHost
    {
        /// <summary>
        /// 状态
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 标识名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 开放的端口
        /// </summary>
        int Port { get; }

        /// <summary>
        /// 配置文件
        /// </summary>
        IHostSetting Setting { get; }

        /// <summary>
        /// 类型
        /// </summary>
        EHostType Type { get; }

        /// <summary>
        /// 监听套接字
        /// </summary>
        ISocket SocketListener { get; }

        /// <summary>
        /// 启动结构
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        bool Start(IHostSetting setting);

        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        void Shutdown();
    }
}
