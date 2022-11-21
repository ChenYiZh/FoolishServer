using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Runtime
{
    /// <summary>
    /// 自定义运行时
    /// </summary>
    public interface IRuntime
    {
        /// <summary>
        /// 启动，但未启动服务器时执行
        /// </summary>
        void OnStartup();
        /// <summary>
        /// 在数据库管理对象初始化完成时执行，
        /// 并没有连接，也没有数据
        /// </summary>
        void OnDatebaseInitialized();
        /// <summary>
        /// 准备开始启动服务器时调用，
        /// 数据库已经连接，并且热数据已经载入
        /// </summary>
        void ReadyToStartServers();
        /// <summary>
        /// 服务器启动后执行
        /// </summary>
        void OnBegun();
        /// <summary>
        /// 在关闭前执行
        /// </summary>
        void OnShutdown();
        /// <summary>
        /// 在退出时执行
        /// </summary>
        void OnKilled();
    }
}
