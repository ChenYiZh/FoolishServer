using FoolishGames.Common;
using FoolishServer.Data.Entity;
using FoolishServer.Struct;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace FoolishServer.Common
{
    /// <summary>
    /// Json处理中心
    /// </summary>
    public class JsonUtility
    {
        /// <summary>
        /// 转Json
        /// </summary>
        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// 转实例
        /// </summary>
        public static T ToEntity<T>(string json) where T : Entity, new()
        {
            return ToObject<T>(json);
        }

        /// <summary>
        /// 转对象
        /// </summary>
        public static T ToObject<T>(string json) where T : new()
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 转对象
        /// </summary>
        public static object ToObject(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type);
        }
    }
}
