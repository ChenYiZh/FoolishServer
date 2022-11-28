using FoolishServer.Config;
using System;
using System.Collections.Generic;
using System.Text;
using FoolishGames.IO;
using FoolishGames.Security;
using FoolishGames.Proxy;
using FoolishGames.Net;

namespace FoolishServer.Net
{
    /// <summary>
    /// 服务器对象接口定义
    /// </summary>
    public interface IServer
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
        ESocketType Type { get; }

        /// <summary>
        /// 监听套接字
        /// </summary>
        IServerSocket ServerSocket { get; }

        ///// <summary>
        ///// 压缩工具
        ///// </summary>
        //ICompression Compression { get; set; }

        ///// <summary>
        ///// 加密工具
        ///// </summary>
        //ICryptoProvider CryptoProvider { get; set; }

        ///// <summary>
        ///// 消息处理的中转站
        ///// </summary>
        //ISupervisor MessageContractor { get; set; }

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
