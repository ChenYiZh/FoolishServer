using FoolishGames.Log;
using FoolishGames.Timer;
using FoolishServer.Log;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FoolishServer.Model
{
    /// <summary>
    /// 当属性发生变化时执行
    /// </summary>
    /// <param name="propertyName">属性名称</param>
    /// <param name="oldValue">原本的数据</param>
    /// <param name="value">现在的数据</param>
    public delegate void OnPropertyModified(Entity sender, string propertyName, object oldValue, object value);

    /// <summary>
    /// Model基类
    /// </summary>
    public class Entity : IEntity
    {
        /// <summary>
        /// 当属性发生变化时执行
        /// </summary>
        public event OnPropertyModified OnPropertyModified;

        /// <summary>
        /// 锁
        /// </summary>
        protected internal object SyncRoot = new object();

        /// <summary>
        /// 是否已经发生变化
        /// </summary>
        public bool IsModified { get { lock (SyncRoot) { return ModifiedType != EModifyType.UnModified; } } }

        /// <summary>
        /// 上次修改的时间
        /// </summary>
        private DateTime modifiedTime;

        /// <summary>
        /// 上次修改的时间
        /// </summary>
        public DateTime ModifiedTime { get { lock (SyncRoot) { return modifiedTime; } } }

        /// <summary>
        /// 操作类型
        /// </summary>
        private EModifyType modifiedType = EModifyType.Add;

        /// <summary>
        /// 操作类型
        /// </summary>
        public EModifyType ModifiedType { get { lock (SyncRoot) { return modifiedType; } } }

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

        /// <summary>
        /// 注入时调用
        /// </summary>
        protected void NotifyPropertyModified(string propertyName, object oldValue, object value)
        {
            NotifyModified(EModifyType.Modify, propertyName);
            try
            {
                OnPropertyModified?.Invoke(this, propertyName, oldValue, value);
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.MODEL, e);
            }
        }

        /// <summary>
        /// 通知实例已经发生变化
        /// </summary>
        protected void NotifyModified(EModifyType modifiedType, string propertyName)
        {
            lock (SyncRoot)
            {
                this.modifiedType = modifiedType;
                modifiedTime = TimeLord.Now;
            }
        }

        /// <summary>
        /// 修改已经提交时执行
        /// </summary>
        internal void OnModificationCommitted()
        {
            lock (SyncRoot)
            {
                modifiedType = EModifyType.UnModified;
                modifiedTime = TimeLord.Now;
            }
        }
    }
}
