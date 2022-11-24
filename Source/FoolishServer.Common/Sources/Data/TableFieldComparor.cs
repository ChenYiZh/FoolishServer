using FoolishServer.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Data
{
    /// <summary>
    /// 表的列表判断是否更改
    /// </summary>
    public struct TableFieldComparor
    {
        /// <summary>
        /// 需要执行的操作
        /// </summary>
        public enum EOperation
        {
            /// <summary>
            /// 不需要操作
            /// </summary>
            UnModified = 0,
            /// <summary>
            /// 只需要修改字段属性
            /// </summary>
            Modified = 1,
            /// <summary>
            /// 先删除后添加
            /// </summary>
            DeleteThenInsert = 2,
            /// <summary>
            /// 添加新列
            /// </summary>
            ToInsert = 10,
            /// <summary>
            /// 删除列
            /// </summary>
            Deleted = 20,
        }
        /// <summary>
        /// 需要做的操作
        /// </summary>
        public EOperation Operation { get; private set; }
        /// <summary>
        /// 缓存中的结构
        /// </summary>
        public IEntityField TableField { get; private set; }
        /// <summary>
        /// 数据库中的结构
        /// </summary>
        public IEntityField DbFieldInfo { get; private set; }
        /// <summary>
        /// 判断两个结构是否需要进行操作
        /// </summary>
        /// <param name="tableField">缓存中的结构</param>
        /// <param name="dbFieldInfo">数据库中的结构</param>
        public TableFieldComparor(IEntityField tableField, IEntityField dbFieldInfo)
        {
            Operation = EOperation.UnModified;
            TableField = tableField;
            DbFieldInfo = dbFieldInfo;
            Operation = Compare(tableField, dbFieldInfo);
        }
        /// <summary>
        /// 判断两个结构是否需要进行操作
        /// </summary>
        public static EOperation Compare(IEntityField tableField, IEntityField dbFieldInfo)
        {
            return EOperation.UnModified;
        }
    }
}
