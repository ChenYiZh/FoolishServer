using FoolishGames.Collections;
using FoolishGames.Common;
using FoolishGames.Log;
using FoolishServer.Config;
using FoolishServer.Data.Entity;
using FoolishServer.Log;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FoolishServer.Data
{
    /// <summary>
    /// 当DbSet中的数据发生变化时调用
    /// </summary>
    internal delegate void OnDbSetDataModified<T>(EntityKey key, T entity) where T : MajorEntity, new();
    /// <summary>
    /// 缓存中的数据结构
    /// </summary>
    internal class DbSet<T> : IDbSet<T> where T : MajorEntity, new()
    {
        /// <summary>
        /// 数据变化监听
        /// </summary>
        public event OnDbSetDataModified<T> OnDataModified;

        /// <summary>
        /// 缓存中的数据，热数据
        /// </summary>
        private ThreadSafeDictionary<EntityKey, T> rawEntities = new ThreadSafeDictionary<EntityKey, T>();

        /// <summary>
        /// 缓存中的数据，热数据
        /// </summary>
        public IReadOnlyDictionary<EntityKey, T> RawEntities { get { return rawEntities; } }

        /// <summary>
        /// 已经改动过的实例，这些数据应该会出现在热数据中，主要用于推送
        /// </summary>
        private ThreadSafeDictionary<EntityKey, T> modifiedEntities = new ThreadSafeDictionary<EntityKey, T>();

        /// <summary>
        /// 已经改动过的实例，这些数据应该会出现在热数据中，主要用于推送
        /// </summary>
        public IReadOnlyDictionary<EntityKey, T> ModifiedEntities { get { return modifiedEntities; } }

        /// <summary>
        /// 冷数据
        /// </summary>
        private ThreadSafeDictionary<EntityKey, T> coldEntities = new ThreadSafeDictionary<EntityKey, T>();

        /// <summary>
        /// 冷数据
        /// </summary>
        public IReadOnlyDictionary<EntityKey, T> ColdEntities { get { return coldEntities; } }

        /// <summary>
        /// 卸载冷数据倒计时
        /// </summary>
        public int ReleaseCountdown { get; set; }

        /// <summary>
        /// 推送倒计时
        /// </summary>
        public int CommitCountdown { get; set; } = Settings.DataCommitInterval;

        /// <summary>
        /// 用于锁的对象
        /// </summary>
        public object SyncRoot { get; private set; } = new object();

        /// <summary>
        /// 非泛型修改数据通知，内部强制转换
        /// </summary>
        public void OnModified(EntityKey key, MajorEntity entity)
        {
            try
            {
                OnModified(key, (T)entity);
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.ENTITY, "Cast error.", e);
            }
        }

        /// <summary>
        /// 当数据修改时执行
        /// </summary>
        public void OnModified(EntityKey key, T entity)
        {
            if (string.IsNullOrEmpty(key))
            {
                FConsole.WriteExceptionWithCategory(Categories.ENTITY, new NullReferenceException("DbSet receive an empty-key entity!"));
                return;
            }
            if (string.IsNullOrEmpty(entity.GetEntityKey()))
            {
                if (entity.GetOldEntityKey() != entity.GetEntityKey())
                {
                    FConsole.WriteWarnFormatWithCategory(Categories.ENTITY, "DbSet<{0}> origin-key:{1} is using an empty key and old key will be removed.", FType<T>.Type.FullName, entity.GetOldEntityKey());
                    Remove(entity.GetOldEntityKey());
                }
                return;
            }
            modifiedEntities[key] = entity;
            if (entity == null)
            {
                Remove(key);
            }
            else
            {
                Modify(entity);
            }
            OnDataModified?.Invoke(key, entity);
        }

        /// <summary>
        /// 需要从两个列表中删除
        /// </summary>
        /// <param name="key"></param>
        private void Remove(EntityKey key)
        {
            T entity;
            if (rawEntities.TryGetValue(key, out entity))
            {
                entity.State = EStorageState.Removed;
            }
            rawEntities.Remove(key);
            if (coldEntities.TryGetValue(key, out entity))
            {
                entity.State = EStorageState.Removed;
            }
            coldEntities.Remove(key);
        }

        /// <summary>
        /// 修改集合
        /// </summary>
        private void Modify(T entity)
        {
            rawEntities[entity.GetEntityKey()] = entity;
            coldEntities.Remove(entity.GetEntityKey());
        }

        /// <summary>
        /// 卸载冷数据
        /// </summary>
        public void ReleaseColdEntities()
        {
            coldEntities.Clear();
            FConsole.Write(GetType().Name + " ReleaseColdEntities.");
        }

        /// <summary>
        /// 将所有修改过的数据进行提交
        /// </summary>
        public void CommitModifiedData()
        {
            FConsole.Write(GetType().Name + " CommitModifiedData.");
            if (modifiedEntities.Count > 0)
            {
                List<DbCommition> entities = new List<DbCommition>();
                foreach (KeyValuePair<EntityKey, T> kv in modifiedEntities)
                {
                    entities.Add(new DbCommition(kv.Key, kv.Value == null ? EModifyType.Remove : kv.Value.ModifiedType, kv.Value));
                    kv.Value.ResetModifiedType();
                }
                modifiedEntities.Clear();
                DataContext.Redis.CommitModifiedEntitys(entities);
            }
        }

        public IReadOnlyDictionary<EntityKey, T> LoadAll()
        {
            // TODO: LoadAll;
            ReleaseCountdown = Settings.DataReleasePeriod;
            return new Dictionary<EntityKey, T>();
        }
    }
}
