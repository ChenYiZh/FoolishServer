using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.IO
{
    /// <summary>
    /// 消息构造接口
    /// </summary>
    public interface IMessageHeader
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        int MsgId { get; }

        /// <summary>
        /// 操作码
        /// </summary>
        sbyte OpCode { get; }

        /// <summary>
        /// 通讯协议Id
        /// </summary>
        int ActionId { get; }

        /// <summary>
        /// 是否有报错
        /// </summary>
        bool IsError { get; }

        /// <summary>
        /// 错误信息
        /// </summary>
        string Error { get; }

        /// <summary>
        /// 包体长度
        /// </summary>
        int GetPacketLength();

        /// <summary>
        /// 内容信息
        /// </summary>
        byte[] GetContext();

        /// <summary>
        /// 内容长度
        /// </summary>
        int ContextLength { get; }
    }
}
