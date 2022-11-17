using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Runtime
{
    /// <summary>
    /// 自定义运行时
    /// </summary>
    public class CustomRuntime : IRuntime
    {
        /// <summary>
        /// 启动，但未启动服务器时执行
        /// </summary>
        public virtual void OnStartup() { }
        /// <summary>
        /// 服务器启动后执行
        /// </summary>
        public virtual void OnBegun() { }
        /// <summary>
        /// 在关闭前执行
        /// </summary>
        public virtual void OnShutdown() { }
        /// <summary>
        /// 在退出时执行
        /// </summary>
        public virtual void OnKilled() { }
    }
}
