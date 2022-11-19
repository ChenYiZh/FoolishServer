using FoolishGames.Log;
using FoolishGames.Timer;
using FoolishServer.Data.Entity;
using FoolishServer.Log;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FoolishServer.Struct
{
    /// <summary>
    /// 当属性发生变化时执行
    /// </summary>
    /// <param name="propertyName">属性名称</param>
    /// <param name="oldValue">原本的数据</param>
    /// <param name="value">现在的数据</param>
    public delegate void OnPropertyModified<T>(T sender, string propertyName, object oldValue, object value) where T : Entity;

    /// <summary>
    /// Model基类
    /// </summary>
    public abstract class Entity : IEntity
    {
        /// <summary>
        /// 锁
        /// </summary>
        protected internal object SyncRoot = new object();

        /// <summary>
        /// 是否已经发生变化
        /// </summary>
        [JsonIgnore]
        public bool IsModified { get { lock (SyncRoot) { return ModifiedType != EModifyType.UnModified; } } }

        /// <summary>
        /// 上次修改的时间
        /// </summary>
        [NonSerialized]
        private DateTime modifiedTime;

        /// <summary>
        /// 上次修改的时间
        /// </summary>        
        [ProtoMember(ushort.MaxValue), EntityField]
        public DateTime ModifiedTime
        {
            get { lock (SyncRoot) { return modifiedTime; } }
            //加载的时候还原
            internal set { modifiedTime = value; }
        }

        /// <summary>
        /// 操作类型
        /// </summary>
        [NonSerialized]
        private EModifyType modifiedType = EModifyType.Add;

        /// <summary>
        /// 操作类型
        /// </summary>
        [JsonIgnore]
        public EModifyType ModifiedType { get { lock (SyncRoot) { return modifiedType; } } }

        /// <summary>
        /// 注入时调用
        /// </summary>
        protected void NotifyPropertyModified(string propertyName, object oldValue, object value)
        {
            OnNotifyPropertyModified(propertyName, oldValue, value);
        }

        /// <summary>
        /// 属性调用的实现函数
        /// </summary>
        internal virtual void OnNotifyPropertyModified(string propertyName, object oldValue, object value)
        {
            NotifyModified(EModifyType.Modify, propertyName);
            //设置数据关联
            PropertyEntity property = value as PropertyEntity;
            if (property != null && oldValue == null)
            {
                property.SetParent(this, propertyName);
            }
        }

        /// <summary>
        /// 通知实例已经发生变化
        /// </summary>
        internal virtual void NotifyModified(EModifyType modifiedType, string propertyName = null)
        {
            lock (SyncRoot)
            {
                NotifyModifiedType(modifiedType);
                modifiedTime = TimeLord.Now;
            }
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        private void NotifyModifiedType(EModifyType modifiedType)
        {
            switch (this.modifiedType)
            {
                case EModifyType.UnModified: this.modifiedType = modifiedType; break;
                case EModifyType.Modify:
                    {
                        if (modifiedType == EModifyType.Remove || modifiedType == EModifyType.Add)
                        {
                            this.modifiedType = modifiedType;
                        }
                    }
                    break;
                case EModifyType.Add:
                    {
                        if (modifiedType == EModifyType.Remove)
                        {
                            this.modifiedType = EModifyType.UnModified;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 修改已经提交时执行
        /// </summary>
        internal virtual void OnModificationCommitted()
        {
            lock (SyncRoot)
            {
                ResetModifiedType();
                modifiedTime = TimeLord.Now;
            }
        }

        internal virtual void ResetModifiedType()
        {
            lock (SyncRoot)
            {
                modifiedType = EModifyType.UnModified;
            }
        }
    }
}
