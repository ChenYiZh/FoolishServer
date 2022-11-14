using FoolishGames.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Action
{
    /// <summary>
    /// 消息处理类
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        long MsgId { get; }

        /// <summary>
        /// ActionId
        /// </summary>
        int ActionId { get; }

        /// <summary>
        /// 是否统计时间
        /// </summary>
        bool AnalysisTime { get; }

        /// <summary>
        /// 警告超时时间，0为全部统计
        /// </summary>
        int AlertTimeout { get; }

        /// <summary>
        /// 刚创建时处理，所有参数都还没有赋值
        /// </summary>
        void Awake();

        /// <summary>
        /// 预处理数据在所有操作之前
        /// </summary>
        /// <param name="reader"></param>
        void SetReader(IMessageReader reader);

        /// <summary>
        /// 判断有效性
        /// </summary>
        bool Check();

        /// <summary>
        /// 处理操作
        /// </summary>
        void TakeAction(IMessageReader reader);
    }
}
