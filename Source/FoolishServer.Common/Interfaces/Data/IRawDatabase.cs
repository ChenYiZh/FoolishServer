using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Data
{
    /// <summary>
    /// 热数据，数据库结构
    /// </summary>
    public interface IRawDatabase : IDatabase
    {
        /// <summary>
        /// 解析方案，默认Protobuff
        /// </summary>
        IEntityConverter Converter { get; set; }
    }
}
