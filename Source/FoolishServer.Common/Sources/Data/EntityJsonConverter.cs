using System;
using System.Collections.Generic;
using System.Text;
using FoolishServer.Common;
using FoolishServer.Data.Entity;
using Newtonsoft.Json;

namespace FoolishServer.Data
{
    /// <summary>
    /// 通过json解析
    /// </summary>
    public class EntityJsonConverter : EntityConverter<string>
    {
        public override TEntity Deserialize<TEntity>(string data)
        {
            return JsonUtility.ToEntity<TEntity>(data);
        }

        public override string Serialize(MajorEntity entity)
        {
            return JsonUtility.ToJson(entity);
        }
    }
}
