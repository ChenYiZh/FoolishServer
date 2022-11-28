using FoolishGames.Net;
using FoolishServer.RPC;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Config
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
        ESocketType Type { get; }

        /// <summary>
        /// 消息处理的完整类名，用{0}嵌入id
        /// </summary>
        string ActionClassFullName { get; }

        /// <summary>
        /// 执行类
        /// </summary>
        string MainClass { get; }

        /// <summary>
        /// TCP全连接队列长度
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
        /// 默认消息处理连接池容量大小
        /// </summary>
        int MaxIOCapacity { get; }

        /// <summary>
        /// 数据通讯缓存字节大小
        /// </summary>
        int BufferSize { get; }

        /// <summary>
        /// 通讯内容整体偏移
        /// </summary>
        int Offset { get; }

        /// <summary>
        /// 是否使用压缩
        /// </summary>
        bool UseGZip { get; }

        /// <summary>
        /// 获取类别显示
        /// </summary>
        string GetCategory();
    }
}
