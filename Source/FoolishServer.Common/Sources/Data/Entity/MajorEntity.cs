using FoolishGames.Log;
using FoolishServer.Log;
using FoolishServer.Struct;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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

        public MajorEntity()
        {

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
        /// 主键
        /// </summary>
        /// <returns></returns>
        public abstract long GetEntityId();
    }
}
