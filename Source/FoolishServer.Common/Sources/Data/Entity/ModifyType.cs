using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Data.Entity
{
    /// <summary>
    /// 修改属性的操作类型
    /// </summary>
    public enum EModifyType
    {
        /// <summary>
        /// 未修改
        /// </summary>
        UnModified = -1,
        /// <summary>
        /// 添加
        /// </summary>
        Add = 0,
        /// <summary>
        /// 修改
        /// </summary>
        Modify = 1,
        /// <summary>
        /// 移除
        /// </summary>
        Remove = 2
    }
}
