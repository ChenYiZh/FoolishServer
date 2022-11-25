using FoolishGames.Log;
using FoolishServer.Config;
using FoolishServer.Log;
using FoolishServer.Struct;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

namespace FoolishServer.Data.Entity
{
    /// <summary>
    /// 当前实例的存储状态
    /// </summary>
    public enum EStorageState
    {
        /// <summary>
        /// 刚创建的
        /// </summary>
        New,
        /// <summary>
        /// 已经在堆栈中
        /// </summary>
        Stored,
        /// <summary>
        /// 移除了
        /// </summary>
        Removed,
    }
    /// <summary>
    /// 基于表的类区别于Minor
    /// </summary>
    public abstract class MajorEntity : Struct.Entity
    {
        /// <summary>
        /// 当前实例的存储状态
        /// </summary>
        [JsonIgnore]
        private EStorageState state = EStorageState.New;
        /// <summary>
        /// 当前实例的存储状态
        /// </summary>
        [JsonIgnore]
        public override EStorageState State
        {
            get
            {
                //lock (SyncRoot)
                //{
                //    return state;
                //}
                object syncRoot = SyncRoot;
                bool lockTaken = false;
                try
                {
                    Monitor.TryEnter(syncRoot, Settings.LockerTimeout, ref lockTaken);
                    return state;
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
        /// 当前实例的存储状态
        /// </summary>
        /// <param name="state"></param>
        internal void SetState(EStorageState state)
        {
            //lock (SyncRoot)
            //{
            //    this.state = state;
            //}
            object syncRoot = SyncRoot;
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(syncRoot, Settings.LockerTimeout, ref lockTaken);
                this.state = state;
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
        /// 表名
        /// </summary>
        [JsonIgnore]
        internal ITableScheme TableScheme { get; private set; }
        /// <summary>
        /// EntityKey被修改前的数据
        /// </summary>
        [JsonIgnore]
        private EntityKey oldEntityKey;
        /// <summary>
        /// Redis主键名称
        /// </summary>
        [JsonIgnore]
        private EntityKey entityKey;

        /// <summary>
        /// 构造函数
        /// </summary>
        public MajorEntity()
        {
            Type type = GetType();
            TableScheme = DataContext.GetTableScheme(type);
            IReadOnlyList<ITableFieldScheme> keyFields = ((TableScheme)TableScheme).KeyFields;
            object[] keys = new object[keyFields.Count];
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = keyFields[i].DefaultValue;
            }
            oldEntityKey = entityKey = new EntityKey(type, keys);
        }

        /// <summary>
        /// 当属性发生变化时执行
        /// </summary>
        public event OnPropertyModified<MajorEntity> OnPropertyModified;

        /// <summary>
        /// 属性调用的实现函数
        /// </summary>
        internal override void OnNotifyPropertyModified(string propertyName, object oldValue, object value)
        {
            if (TableScheme != null && TableScheme.FieldsByProperty.ContainsKey(propertyName) && TableScheme.FieldsByProperty[propertyName].IsKey)
            {
                UpdateEntityKey(TableScheme.FieldsByProperty[propertyName], value);
            }
            base.OnNotifyPropertyModified(propertyName, oldValue, value);
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
        /// 更新存入Redis的Key
        /// </summary>
        /// <param name="tableField"></param>
        /// <param name="value"></param>
        private void UpdateEntityKey(ITableFieldScheme tableField, object value)
        {
            TableScheme tableScheme = TableScheme as TableScheme;
            if (tableScheme == null) return;
            int index = -1;
            for (int i = 0; i < tableScheme.KeyFields.Count; i++)
            {
                if (tableScheme.KeyFields[i] == tableField)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                FConsole.WriteErrorFormatWithCategory(tableScheme.Type.Name, "Failed to generate entity id... check this class");
                return;
            }
            entityKey.keys[index] = value;
            entityKey.RefreshKeyName();
        }

        /// <summary>
        /// 数据中心提交更新
        /// </summary>
        internal override void NotifyModified(EModifyType modifiedType, string propertyName = null)
        {
            base.NotifyModified(modifiedType, propertyName);
            if (State == EStorageState.Stored)
            {
                DataContext.EntityPool[TableScheme.Type].OnModified(GetEntityKey(), this);
            }
        }

        /// <summary>
        /// 主键
        /// </summary>
        /// <returns></returns>
        public EntityKey GetEntityKey()
        {
            //lock (SyncRoot)
            //{
            //    return entityKey;
            //}
            object syncRoot = SyncRoot;
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(syncRoot, Settings.LockerTimeout, ref lockTaken);
                return entityKey;
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
        /// EntityKey被修改前的数据
        /// </summary>
        /// <returns></returns>
        internal EntityKey GetOldEntityKey()
        {
            //lock (SyncRoot)
            //{
            //    return oldEntityKey;
            //}
            object syncRoot = SyncRoot;
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(syncRoot, Settings.LockerTimeout, ref lockTaken);
                return oldEntityKey;
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
        /// 判断主键是否发生了变化
        /// </summary>
        internal bool KeyIsModified()
        {
            //lock (SyncRoot)
            //{
            //    return !string.IsNullOrEmpty(oldEntityKey) && oldEntityKey != entityKey;
            //}
            object syncRoot = SyncRoot;
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(syncRoot, Settings.LockerTimeout, ref lockTaken);
                return !string.IsNullOrEmpty(oldEntityKey) && oldEntityKey != entityKey;
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
        /// 重置时，把Key也重置
        /// </summary>
        internal override void ResetModifiedType()
        {
            base.ResetModifiedType();
            RefreshEntityKey();
        }

        /// <summary>
        /// 刷新Key
        /// </summary>
        internal void RefreshEntityKey()
        {
            //lock (SyncRoot)
            //{
            //    oldEntityKey = entityKey;
            //}
            object syncRoot = SyncRoot;
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(syncRoot, Settings.LockerTimeout, ref lockTaken);
                oldEntityKey = entityKey;
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
        /// 数据数据库中拉取下来
        /// </summary>
        internal override void OnPulledFromDb()
        {
            base.OnPulledFromDb();
            //lock (SyncRoot)
            //{
            //    state = EStorageState.Stored;
            //}
            object syncRoot = SyncRoot;
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(syncRoot, Settings.LockerTimeout, ref lockTaken);
                state = EStorageState.Stored;
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
}
