using FoolishGames.Collections;
using FoolishServer.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FoolishServer.Data.Entity
{
    public sealed class EntitySet<T> : Struct.Entity, IEntitySet<T> where T : MajorEntity, new()
    {
        private Type EntityType = typeof(T);

        private IDictionary<string, T> Dictionary { get; set; } = new Dictionary<string, T>();

        internal EntitySet() { }

        public bool AddOrUpdate(T entity)
        {
            NotifyModified(EModifyType.Modify);
            return true;
        }
        /// <summary>
        /// 通过唯一主键查询
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public T Find(long entityId)
        {
            return Find(entityId);
        }

        /// <summary>
        /// 符合主键查询
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public T Find(params object[] keys)
        {
            string entityKey = MajorEntity.GenerateKeys(EntityType, keys);
            if (Dictionary.ContainsKey(entityKey))
            {
                return Dictionary[entityKey];
            }
            // TODO: 从数据库中加载
            return null;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Dictionary.Values.GetEnumerator();
        }

        public void LoadAll()
        {
            //TODO: LoadAll()
        }

        public bool Remove(T entity)
        {
            if (Dictionary.Remove(entity.GetEntityKey()))
            {
                return true;
            }
            //TODO: 从数据库中删除
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
