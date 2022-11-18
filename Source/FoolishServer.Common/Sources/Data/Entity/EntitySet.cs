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
        private IDictionary<long, T> Dictionary { get; set; } = new Dictionary<long, T>();

        internal EntitySet() { }

        public bool AddOrUpdate(T entity)
        {
            NotifyModified(EModifyType.Modify);
            return true;
        }

        public T Find(long entityId)
        {
            if (Dictionary.ContainsKey(entityId))
            {
                return Dictionary[entityId];
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
            if (Dictionary.Remove(entity.GetEntityId()))
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
