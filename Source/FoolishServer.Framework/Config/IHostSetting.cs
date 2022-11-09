﻿using FoolishServer.Framework.RPC;
using FoolishServer.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Framework.Config
{
    public interface IHostSetting
    {
        /// <summary>
        /// 服务器标识
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 端口号
        /// </summary>
        int Port { get; }

        /// <summary>
        /// 类型
        /// </summary>
        EHostType Type { get; }

        /// <summary>
        /// 挂起连接的最大长度
        /// </summary>
        int Backlog { get; }

        /// <summary>
        /// 最大并发数量
        /// </summary>
        int MaxConnections { get; }

        /// <summary>
        /// 默认连接对象池容量
        /// </summary>
        int MaxAcceptCapacity { get; }

        /// <summary>
        /// 执行类
        /// </summary>
        string MainClass { get; }

        /// <summary>
        /// 获取类别显示
        /// </summary>
        string GetCategory();
    }
}
