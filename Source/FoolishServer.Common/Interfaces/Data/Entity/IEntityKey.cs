using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Data.Entity
{
    /// <summary>
    /// Entity的Key，害怕多主键的问题
    /// </summary>
    public interface IEntityKey
    {
        /// <summary>
        /// 主键
        /// </summary>
        IReadOnlyList<object> Keys { get; }
        /// <summary>
        /// 完整Key名称，用于判断
        /// </summary>
        string KeyName { get; }
    }
}
