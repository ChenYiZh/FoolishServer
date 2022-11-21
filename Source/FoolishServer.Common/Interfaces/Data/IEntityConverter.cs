using FoolishServer.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Data
{
    /// <summary>
    /// 数据存储和读取时使用，使用抽象类EntityConverter进行修改
    /// </summary>
    public interface IEntityConverter
    {
        /// <summary>
        /// 转换的数据类型
        /// </summary>
        EConvertType Type { get; }
    }
}
