using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FoolishServer.Config;
using FoolishServer.Data.Entity;

namespace FoolishServer.Struct
{
    /// <summary>
    /// 用于属性的结构类
    /// </summary>
    public abstract class PropertyEntity : Entity
    {
        /// <summary>
        /// 父实例
        /// </summary>
        [NonSerialized]
        private Entity parent = null;

        /// <summary>
        /// 父实例
        /// </summary>
        internal Entity Parent { get { lock (SyncRoot) { return parent; } } }

        /// <summary>
        /// 当前实例的存储状态
        /// </summary>
        private EStorageState state = EStorageState.New;

        /// <summary>
        /// 当前实例的存储状态
        /// </summary>
        public override EStorageState State
        {
            get
            {
                //lock (SyncRoot)
                //{
                //    return parent == null ? state : parent.State;
                //}
                object syncRoot = SyncRoot;
                bool lockTaken = false;
                try
                {
                    Monitor.TryEnter(syncRoot, Settings.LockerTimeout, ref lockTaken);
                    return parent == null ? state : parent.State;

                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(syncRoot);
                    }
                }
            }
        }

        /// <summary>
        /// 当前实例在父实例中的属性名称
        /// </summary>
        [NonSerialized]
        private string propertyNameInParent = null;

        /// <summary>
        /// 当前实例在父实例中的属性名称
        /// </summary>
        internal string PropertyNameInParent
        {
            get
            {
                //lock (SyncRoot)
                //{
                //    return propertyNameInParent;
                //}
                object syncRoot = SyncRoot;
                bool lockTaken = false;
                try
                {
                    Monitor.TryEnter(syncRoot, Settings.LockerTimeout, ref lockTaken);
                    return propertyNameInParent;

                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(syncRoot);
                    }
                }
            }
        }

        /// <summary>
        /// 绑入父物体
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="propertyName"></param>
        internal virtual void SetParent(Entity parent, string propertyName)
        {
            //lock (SyncRoot)
            //{
            //    this.parent = parent;
            //    this.propertyNameInParent = propertyName;
            //}
            object syncRoot = SyncRoot;
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(syncRoot, Settings.LockerTimeout, ref lockTaken);
                this.parent = parent;
                this.propertyNameInParent = propertyName;
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(syncRoot);
                }
            }
        }

        /// <summary>
        /// 从父物体中移除
        /// </summary>
        internal void RemoveFromParent()
        {
            //lock (SyncRoot)
            //{
            //    parent = null;
            //    propertyNameInParent = null;
            //    state = EStorageState.Removed;
            //}
            object syncRoot = SyncRoot;
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(syncRoot, Settings.LockerTimeout, ref lockTaken);
                parent = null;
                propertyNameInParent = null;
                state = EStorageState.Removed;
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(syncRoot);
                }
            }
        }
        /// <summary>
        /// 通知修改
        /// </summary>
        /// <param name="modifiedType"></param>
        /// <param name="propertyName"></param>
        internal override void NotifyModified(EModifyType modifiedType, string propertyName = null)
        {
            base.NotifyModified(modifiedType, propertyName);
            lock (SyncRoot)
            {
                if (parent != null && modifiedType != EModifyType.UnModified)
                {
                    parent.NotifyModified(EModifyType.Modify, propertyNameInParent);
                }
            }
        }
    }
}
