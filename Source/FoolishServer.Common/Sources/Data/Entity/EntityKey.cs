using FoolishGames.Log;
using FoolishServer.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace FoolishServer.Data.Entity
{
    /// <summary>
    /// 主键管理类
    /// </summary>
    public struct EntityKey : IEntityKey
    {
        /// <summary>
        /// 数据类型
        /// </summary>
        private Type type;

        /// <summary>
        /// 数据类型
        /// </summary>
        internal Type Type
        {
            get { return type; }
            set
            {
                type = null;
                RefreshKeyName();
            }
        }

        /// <summary>
        /// 主键
        /// </summary>
        internal object[] keys;

        /// <summary>
        /// 主键
        /// </summary>
        public IReadOnlyList<object> Keys { get { return keys; } }

        /// <summary>
        /// 完整Key名称，用于判断
        /// </summary>
        private string keyName;

        /// <summary>
        /// 完整Key名称，用于判断
        /// </summary>
        public string KeyName { get { return keyName; } }

        /// <summary>
        /// 通过主键生成对象
        /// </summary>
        /// <param name="keys"></param>
        public EntityKey(params object[] keys)
        {
            type = null;
            this.keys = keys;
            keyName = MakeKeyName(type, keys);
        }

        /// <summary>
        /// 通过主键生成对象
        /// </summary>
        /// <param name="keys"></param>
        internal EntityKey(Type type, params object[] keys)
        {
            this.type = type;
            this.keys = keys;
            keyName = MakeKeyName(type, keys);
        }

        /// <summary>
        /// 重新刷新KeyName
        /// </summary>
        internal void RefreshKeyName()
        {
            keyName = MakeKeyName(Type, keys);
        }

        /// <summary>
        /// 生成Redis遍历主键
        /// </summary>
        internal static string MakeKeyName(Type entityType, object[] keys)
        {
            if (keys == null) { throw new ArgumentNullException("Entity Keys is null."); }
            string entityKey = string.Join(Settings.SPLITE_KEY.ToString(), keys.Select(k =>
            {
                return k == null ? "" : HttpUtility.UrlEncode(k.ToString()).Replace(Settings.SPLITE_KEY.ToString(), "%" + (int)Settings.SPLITE_KEY);
            }));
            return entityType == null ? entityKey : entityType.FullName + Settings.SPLITE_KEY + entityKey;
        }

        /// <summary>
        /// 重写相等运算符
        /// </summary>
        public static bool operator ==(EntityKey key1, EntityKey key2)
        {
            return key1.keyName == key2.keyName;
        }

        /// <summary>
        /// 重写不等运算符
        /// </summary>
        public static bool operator !=(EntityKey key1, EntityKey key2)
        {
            return key1.keyName != key2.keyName;
        }

        /// <summary>
        /// 重写相等运算符
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null || string.IsNullOrEmpty(keyName))
            {
                return false;
            }
            return keyName == obj.ToString();
        }

        /// <summary>
        /// HashCode是否要重写？
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return keyName.GetHashCode();
        }

        /// <summary>
        /// 重写ToString()
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return keyName.ToString();
        }

        /// <summary>
        /// 隐式转换成字符串
        /// </summary>
        /// <param name="key"></param>
        public static implicit operator string(EntityKey key)
        {
            return key.keyName;
        }
    }
}
