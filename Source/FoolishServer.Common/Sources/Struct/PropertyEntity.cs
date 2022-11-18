using System;
using System.Collections.Generic;
using System.Text;
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
        private Entity parent = null;

        /// <summary>
        /// 父实例
        /// </summary>
        internal Entity Parent { get { lock (SyncRoot) { return parent; } } }

        /// <summary>
        /// 当前实例在父实例中的属性名称
        /// </summary>
        private string propertyNameInParent = null;

        /// <summary>
        /// 当前实例在父实例中的属性名称
        /// </summary>
        internal string PropertyNameInParent { get { lock (SyncRoot) { return propertyNameInParent; } } }

        /// <summary>
        /// 绑入父物体
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="propertyName"></param>
        internal void SetParent(Entity parent, string propertyName)
        {
            lock (SyncRoot)
            {
                this.parent = parent;
                this.propertyNameInParent = propertyName;
            }
        }

        /// <summary>
        /// 从父物体中移除
        /// </summary>
        internal void RemoveFromParent()
        {
            lock (SyncRoot)
            {
                parent = null;
                propertyNameInParent = null;
            }
        }

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
