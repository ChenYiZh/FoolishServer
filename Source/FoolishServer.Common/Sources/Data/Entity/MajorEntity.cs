using FoolishGames.Log;
using FoolishServer.Config;
using FoolishServer.Log;
using FoolishServer.Struct;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public EStorageState State
        {
            get { lock (SyncRoot) { return state; } }
            internal set { lock (SyncRoot) { state = value; } }
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
            object[] keys = new object[((TableScheme)TableScheme).KeyFields.Count];
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = null;
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
            base.OnNotifyPropertyModified(propertyName, oldValue, value);
            if (TableScheme != null && TableScheme.Fields.ContainsKey(propertyName) && TableScheme.Fields[propertyName].IsKey)
            {
                UpdateEntityKey(TableScheme.Fields[propertyName], value);
            }
            if (State == EStorageState.Stored)
            {
                DataContext.EntityPool[TableScheme.Type].OnModified(GetEntityKey(), this);
            }
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
        /// 主键
        /// </summary>
        /// <returns></returns>
        public EntityKey GetEntityKey()
        {
            lock (SyncRoot)
            {
                return entityKey;
            }
        }
        /// <summary>
        /// EntityKey被修改前的数据
        /// </summary>
        /// <returns></returns>
        internal EntityKey GetOldEntityKey()
        {
            lock (SyncRoot)
            {
                return oldEntityKey;
            }
        }
        /// <summary>
        /// 判断主键是否发生了变化
        /// </summary>
        internal bool KeyIsModified()
        {
            lock (SyncRoot)
            {
                return !string.IsNullOrEmpty(oldEntityKey) && oldEntityKey != entityKey;
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

        internal void RefreshEntityKey()
        {
            lock (SyncRoot)
            {
                oldEntityKey = entityKey;
            }
        }
    }
}
