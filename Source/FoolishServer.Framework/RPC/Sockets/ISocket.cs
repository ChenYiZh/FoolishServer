using FoolishServer.Framework.Config;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FoolishServer.Framework.RPC.Sockets
{
    public interface ISocket
    {
        /// <summary>
        /// 对应Host的名称
        /// </summary>
        string HostName { get; }
        /// <summary>
        /// 是否在运行
        /// </summary>
        bool IsRunning { get; }

        ///// <summary>
        ///// 内部关键原生Socket
        ///// </summary>
        //Socket Socket { get; }

        /// <summary>
        /// 绑定的端口
        /// </summary>
        int Port { get; }

        /// <summary>
        /// 封装的地址
        /// </summary>
        IPEndPoint Address { get; }

        /// <summary>
        /// 获取类型
        /// </summary>
        EHostType Type { get; }

        /// <summary>
        /// 获取配置信息
        /// </summary>
        IHostSetting Setting { get; }

        /// <summary>
        /// 启动函数
        /// </summary>
        void Start(IHostSetting setting);

        /// <summary>
        /// 关闭函数
        /// </summary>
        void Close();
    }
}
