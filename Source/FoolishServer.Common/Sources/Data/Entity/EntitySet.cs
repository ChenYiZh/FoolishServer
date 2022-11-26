﻿using FoolishGames.Collections;
using FoolishGames.Common;
using FoolishGames.Log;
using FoolishServer.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FoolishServer.Data.Entity
{
    /// <summary>
    /// 数据对象集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class EntitySet<T> :/* Struct.Entity,*/ IEntitySet<T> where T : MajorEntity, new()
    {
        /// <summary>
        /// 全局管理的数据对象池
        /// </summary>
        private IDbSet<T> DbSet;

        /// <summary>
        /// 泛型的类
        /// </summary>
        public Type EntityType { get; private set; } = FType<T>.Type;

        /// <summary>
        /// 全数据
        /// </summary>
        private ThreadSafeDictionary<EntityKey, T> FullData = null;

        /// <summary>
        /// 这个类的缓存数据
        /// </summary>
        private ThreadSafeDictionary<EntityKey, T> Dictionary
        {
            get { return FullData != null ? FullData : (ThreadSafeDictionary<EntityKey, T>)DbSet.RawEntities; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        internal EntitySet(IDbSet<T> dbSet)
        {
            DbSet = dbSet;
            ThreadSafeDictionary<EntityKey, T> source = (ThreadSafeDictionary<EntityKey, T>)dbSet.RawEntities;

            lock (source.SyncRoot)
            {
                FullData = new ThreadSafeDictionary<EntityKey, T>(source);
            }

            lock (dbSet.SyncRoot)
            {
                dbSet.OnDataModified += OnDbSetDataModified;
            }
        }

        /// <summary>
        /// 析构时注销
        /// </summary>
        ~EntitySet()
        {
            DbSet.OnDataModified -= OnDbSetDataModified;
        }

        ///// <summary>
        ///// 通过唯一主键查询
        ///// </summary>
        //public T Find(long entityId)
        //{
        //    return Find(entityId);
        //}

        /// <summary>
        /// 主键查找，如果缓存中找不到，会从数据库中查询
        /// </summary>
        public T Find(params object[] keys)
        {
            EntityKey entityKey = new EntityKey(EntityType, keys);
            return Find(entityKey);
        }

        /// <summary>
        /// 主键类查询
        /// </summary>
        public T Find(EntityKey key)
        {
            if (key.Type == null)
            {
                key.Type = EntityType;
            }
            T result;
            if (Dictionary.TryGetValue(key, out result)) { return result; }
            //if (FullData != null && FullData.TryGetValue(key, out result)) { return result; }
            return DbSet.Find(key);
        }

        /// <summary>
        /// 根据Lamda返回新的列表，不会影响内部数据列表
        /// <para>如果要遍历所有，需要预先使用LoadAll</para>
        /// </summary>
        public IList<T> Find(Func<T, bool> condition)
        {
            ThreadSafeDictionary<EntityKey, T> dic = Dictionary;
            List<T> entities = new List<T>(dic.Count);
            lock (dic.SyncRoot)
            {
                foreach (T t in dic.Values)
                {
                    if (condition(t))
                    {
                        entities.Add(t);
                    }
                }
            }
            return entities;
        }

        /// <summary>
        /// 添加或保存
        /// </summary>
        public bool AddOrUpdate(T entity)
        {
            if (entity == null)
            {
                FConsole.WriteWarnFormatWithCategory(Categories.ENTITY, "DbSet<{0}> add or update an empty data.", EntityType.FullName);
                return false;
            }
            entity.GetEntityKey().RefreshKeyName();
            if (string.IsNullOrEmpty(entity.GetEntityKey()))
            {
                //当Key发生变化是，删除原数据
                if (entity.GetOldEntityKey() != entity.GetEntityKey())
                {
                    FConsole.WriteWarnFormatWithCategory(Categories.ENTITY, "DbSet<{0}> origin-key:{1} is using an empty key and old key will be removed.", EntityType.FullName, entity.GetOldEntityKey());
                    Remove(entity.GetOldEntityKey());
                }
                return false;
            }
            //如果这个实例已经时删除状态，将不再退入
            if (entity.ModifiedType == EModifyType.Remove || entity.State == EStorageState.Removed)
            {
                Remove(entity);
                return false;
            }

            ThreadSafeDictionary<EntityKey, T> dictionary = Dictionary;
            lock (dictionary.SyncRoot)
            {
                EntityKey key = entity.GetEntityKey();
                if (entity.ModifiedType == EModifyType.Add && dictionary.ContainsKey(key))
                {
                    entity.ModifiedType = dictionary[key].ModifiedType;
                }
                dictionary[key] = entity;
            }
            //Dictionary.Add(entity.GetEntityKey(), entity);

            entity.SetState(EStorageState.Stored);
            if (entity.ModifiedType == EModifyType.Modify && entity.KeyIsModified())
            {
                FConsole.WriteWarnFormatWithCategory(Categories.ENTITY, "DbSet<{0}>: A key of entity has been changed from {0} to {1}.", entity.GetOldEntityKey(), entity.GetEntityKey());
                Remove(entity.GetOldEntityKey());
            }
            //刷新Key
            entity.RefreshEntityKey();
            //通知数据中心更新数据
            DbSet.OnModified(entity.GetEntityKey(), entity);
            return true;
        }

        /// <summary>
        /// 删除实例
        /// </summary>
        public bool Remove(T entity)
        {
            if (entity == null)
            {
                FConsole.WriteWarnFormatWithCategory(Categories.ENTITY, "DbSet<{0}> remove an empty data.", EntityType.FullName);
                return false;
            }
            return Remove(entity.GetEntityKey());
        }

        /// <summary>
        /// 通过Key删除
        /// </summary>
        public bool Remove(EntityKey key)
        {
            bool result = Dictionary.Remove(key);
            //通知数据中心更新数据
            DbSet.OnModified(key, null);
            return result;
        }

        /// <summary>
        /// 迭代器
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return Dictionary.Values.GetEnumerator();
        }

        /// <summary>
        /// 迭代器
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 拉取所有数据
        /// </summary>
        public void LoadAll()
        {
            lock (this)
            {
                IReadOnlyDictionary<EntityKey, T> entities = DbSet.LoadAll();
                //Dictionary.Clear();
                //foreach (KeyValuePair<EntityKey, T> kv in entities)
                //{
                //    Dictionary.Add(kv.Key, kv.Value);
                //}
                FullData = new ThreadSafeDictionary<EntityKey, T>((IDictionary<EntityKey, T>)entities);
            }
        }

        /// <summary>
        /// 监听DbSet的数据变化
        /// </summary>
        void OnDbSetDataModified(EntityKey key, T entity)
        {
            if (entity == null)
            {
                Dictionary.Remove(key);
                //FConsole.Write("DbSet removed " + key + ".");
            }
            else if (entity.ModifiedType != EModifyType.Remove)
            {
                Dictionary[entity.GetEntityKey()] = entity;
                //FConsole.Write("DbSet modified " + key + ".");
            }
        }
    }
}
