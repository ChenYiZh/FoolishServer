using FoolishGames.Log;
using FoolishServer.Log;
using FoolishServer.Struct;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Data.Entity
{
    /// <summary>
    /// 用于属性的结构类
    /// </summary>
    public class MinorEntity : PropertyEntity
    {
        /// <summary>
        /// 当属性发生变化时执行
        /// </summary>
        public event OnPropertyModified<MinorEntity> OnPropertyModified;

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
    }
}
