﻿/****************************************************************************
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
