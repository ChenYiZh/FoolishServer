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
    /// 基于表的类区别于Minor
    /// </summary>
    public abstract class MajorEntity : Struct.Entity
    {
        /// <summary>
        /// 表名
        /// </summary>
        [JsonIgnore]
        internal ITableScheme TableScheme { get; private set; }
        /// <summary>
        /// EntityKey被修改前的数据
        /// </summary>
        [JsonIgnore]
        internal string OldEntityKey { get; private set; }
        /// <summary>
        /// Redis主键名称
        /// </summary>
        [JsonIgnore]
        private string EntityKey { get; set; }

        public MajorEntity()
        {
            Type type = GetType();
            TableScheme = DataContext.GetTableScheme(type);
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
                UpdateEntityKey(TableScheme.Fields[propertyName], value.ToString());
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
        private void UpdateEntityKey(ITableFieldScheme tableField, string value)
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
            string[] keys;
            if (string.IsNullOrEmpty(EntityKey))
            {
                keys = new string[tableScheme.KeyFields.Count + 1];
                keys[0] = TableScheme.Type.FullName;
            }
            else
            {
                keys = EntityKey.Split(Settings.SPLITE_KEY);
            }
            keys[index + 1] = value;
            EntityKey = GenerateKeys(null, keys);
        }
        /// <summary>
        /// 生成Redis遍历主键
        /// </summary>
        internal static string GenerateKeys(Type entityType, params object[] keys)
        {
            string entityKey = string.Join(Settings.SPLITE_KEY.ToString(), keys.Select(k =>
                 {
                     return k == null ? "" : HttpUtility.UrlEncode(k.ToString()).Replace(Settings.SPLITE_KEY.ToString(), "%" + (int)Settings.SPLITE_KEY);
                 }));
            return entityType == null ? entityKey : entityType.FullName + Settings.SPLITE_KEY + entityKey;
        }
        /// <summary>
        /// 主键
        /// </summary>
        /// <returns></returns>
        public string GetEntityKey()
        {
            return EntityKey;
        }
        /// <summary>
        /// 重置时，把Key也重置
        /// </summary>
        internal override void ResetModifiedType()
        {
            base.ResetModifiedType();
            OldEntityKey = EntityKey;
        }
    }
}
