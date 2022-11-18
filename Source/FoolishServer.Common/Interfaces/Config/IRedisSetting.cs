using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Config
{
    /// <summary>
    /// Redis设置信息
    /// </summary>
    public interface IRedisSetting
    {
        /// <summary>
        /// Redis链接地址
        /// </summary>
        string Host { get; }
        /// <summary>
        /// Redis端口
        /// </summary>
        int Port { get; }
        /// <summary>
        /// 连接密码
        /// </summary>
        string Password { get; }
        /// <summary>
        /// DbIndex
        /// </summary>
        int DbIndex { get; }
    }
}
