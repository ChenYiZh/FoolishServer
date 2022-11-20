using FoolishServer.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Data
{
    /// <summary>
    /// 数据推送构造体
    /// </summary>
    public struct DbCommition
    {
        /// <summary>
        /// 数据Key
        /// </summary>
        public EntityKey Key { get; private set; }
        /// <summary>
        /// 推送行为
        /// </summary>
        public EModifyType ModifyType { get; private set; }
        /// <summary>
        /// 推送的实例
        /// </summary>
        public MajorEntity Entity { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        public DbCommition(EntityKey key, EModifyType modifyType, MajorEntity entity)
        {
            Key = key;
            ModifyType = modifyType;
            Entity = entity;
        }
    }
}
