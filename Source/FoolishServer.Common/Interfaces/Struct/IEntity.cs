using FoolishServer.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Struct
{
    /// <summary>
    /// Model基类
    /// </summary>
    internal interface IEntity
    {
        /// <summary>
        /// 是否已经发生变化
        /// </summary>
        bool IsModified { get; }

        /// <summary>
        /// 上次修改的时间
        /// </summary>
        DateTime ModifiedTime { get; }

        /// <summary>
        /// 操作类型
        /// </summary>
        EModifyType ModifiedType { get; }
    }
}
