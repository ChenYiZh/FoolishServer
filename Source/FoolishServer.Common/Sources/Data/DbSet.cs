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
        /// 现在真在使用的修改缓存池id
        /// </summary>
        private int modifiedEntitiesPoolIndex = 0;

        /// <summary>
        /// 已经改动过的实例，这些数据应该会出现在热数据中，主要用于推送。
        /// 防止多线程阻塞，所以使用集合池
        /// </summary>
        private IReadOnlyDictionary<EntityKey, T>[] modifiedEntitiesPool = null;

        /// <summary>
        /// 现在真在使用的修改缓存池id
        /// </summary>
        public int ModifiedEntitiesPoolIndex { get { return modifiedEntitiesPoolIndex; } }

        /// <summary>
        /// 已经改动过的实例，这些数据应该会出现在热数据中，主要用于推送。
        /// 防止多线程阻塞，所以使用集合池
        /// </summary>
        public IReadOnlyList<IReadOnlyDictionary<EntityKey, T>> ModifiedEntitiesPool { get { return modifiedEntitiesPool; } }

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
        /// 提交的线程管理
        /// </summary>
        internal Thread CommitThread { get; private set; }

        /// <summary>
        /// 提交判断标示
        /// </summary>
        private int commitFlag = 0;

        public DbSet()
        {
            modifiedEntitiesPool = new IReadOnlyDictionary<EntityKey, T>[Settings.ModifiedCacheCount];
            for (int i = 0; i < modifiedEntitiesPool.Length; i++)
            {
                modifiedEntitiesPool[i] = new ThreadSafeDictionary<EntityKey, T>();
            }
            //CommitThread = new Thread(OnCommitModifiedData);
            //CommitThread.Priority = ThreadPriority.Highest;
            //CommitThread.Start();
        }

        /// <summary>
        /// 非泛型修改数据通知，内部强制转换
        /// </summary>
        public void OnModified(EntityKey key, MajorEntity entity)
        {
            try
            {
                OnModified(key, entity as T);
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
            //判空
            if (string.IsNullOrEmpty(key))
            {
                FConsole.WriteExceptionWithCategory(Categories.ENTITY, new NullReferenceException("DbSet receive an empty-key entity!"));
                return;
            }

            //实例为空，就只做移除，通知事件在Remove里
            if (entity == null)
            {
                Remove(key);
                return;
            }

            //判断Key是否发生变化
            if (entity.KeyIsModified())
            {
                //老Key有效的话移除
                if (!string.IsNullOrEmpty(entity.GetOldEntityKey()))
                {
                    FConsole.WriteWarnFormatWithCategory(Categories.ENTITY, "DbSet<{0}>: A key of entity has been changed from {0} to {1}.", entity.GetOldEntityKey(), entity.GetEntityKey());
                    Remove(entity.GetOldEntityKey());
                }
            }

            //刷新Key
            entity.RefreshEntityKey();

            //如果当前的Key为空，就不会加入更新列表
            if (string.IsNullOrEmpty(entity.GetEntityKey()))
            {
                return;
            }

            //更新
           ((ThreadSafeDictionary<EntityKey, T>)modifiedEntitiesPool[modifiedEntitiesPoolIndex])[key] = entity;

            Modify(entity);
            //通知事件
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
                entity.SetState(EStorageState.Removed);
            }
            rawEntities.Remove(key);
            if (coldEntities.TryGetValue(key, out entity))
            {
                entity.SetState(EStorageState.Removed);
            }
            coldEntities.Remove(key);

            //提交更改
            ((ThreadSafeDictionary<EntityKey, T>)modifiedEntitiesPool[modifiedEntitiesPoolIndex])[key] = null;
            //通知
            OnDataModified?.Invoke(key, null);
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
            OnCommitModifiedData(null);
            //Interlocked.Exchange(ref commitFlag, 1);
        }
        /// <summary>
        /// 提交操作
        /// </summary>
        /// <param name="state"></param>
        private void OnCommitModifiedData(object state)
        {
            //while (true)
            //{
            //    if (commitFlag > 0)
            //    {
            //        Interlocked.Exchange(ref commitFlag, 0);
            try
            {
                int pushIndex = modifiedEntitiesPoolIndex - 1;
                if (pushIndex < 0)
                {
                    pushIndex = Settings.ModifiedCacheCount - 1;
                }
                int nextIndex = modifiedEntitiesPoolIndex + 1;
                if (nextIndex >= Settings.ModifiedCacheCount)
                {
                    nextIndex = 0;
                }
                Interlocked.Exchange(ref modifiedEntitiesPoolIndex, nextIndex);
                ThreadSafeDictionary<EntityKey, T> entities = (ThreadSafeDictionary<EntityKey, T>)modifiedEntitiesPool[pushIndex];
                ForceCommitModifiedData(entities);
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.ENTITY, e);
            }
            //    }
            //    Thread.Sleep(10);
            //}
        }

        /// <summary>
        /// 提交所有修改
        /// </summary>
        public void ForceCommitAllModifiedData()
        {
            lock (this)
            {
                foreach (ThreadSafeDictionary<EntityKey, T> entities in modifiedEntitiesPool)
                {
                    ForceCommitModifiedData(entities);
                }
            }
        }

        /// <summary>
        /// 强制提交修改
        /// </summary>
        private void ForceCommitModifiedData(ThreadSafeDictionary<EntityKey, T> entities)
        {
            try
            {
                CommitModifiedEntitys(entities, () => { entities.Clear(); });
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.ENTITY, e);
            }
        }

        /// <summary>
        /// 拉取所有热数据
        /// </summary>
        public void PullAllRawData()
        {
            IEnumerable<T> iterator = DataContext.RawDatabase.LoadAll<T>();
            rawEntities.Clear();
            foreach (T entity in iterator)
            {
                entity.OnPulledFromDb();
                rawEntities[entity.GetEntityKey()] = entity;
            }
        }

        /// <summary>
        /// 将所有的缓存的热数据全部推送出去
        /// </summary>
        public void PushAllRawData()
        {
            CommitModifiedEntitys(rawEntities);
        }

        /// <summary>
        /// 推送集合中的数据
        /// </summary>
        private void CommitModifiedEntitys(ThreadSafeDictionary<EntityKey, T> entities, System.Action onCopied = null)
        {
            if (entities.Count > 0)
            {
                List<DbCommition> commitions = new List<DbCommition>();
                Dictionary<EntityKey, T> dic = null;
                bool lockToken = false;
                try
                {
                    Monitor.TryEnter(entities.SyncRoot, Settings.LockerTimeout, ref lockToken);
                    if (lockToken)
                    {
                        //中间缓存池，防死锁
                        dic = new Dictionary<EntityKey, T>(entities);
                        onCopied?.Invoke();
                    }
                }
                finally
                {
                    if (lockToken)
                    {
                        Monitor.Exit(entities.SyncRoot);
                    }
                }
                if (lockToken)
                {
                    string database = DataContext.GetTableScheme<T>().ConnectKey;
                    foreach (KeyValuePair<EntityKey, T> kv in dic)
                    {
                        commitions.Add(new DbCommition(kv.Key, kv.Value == null ? EModifyType.Remove : kv.Value.ModifiedType, kv.Value));
                        kv.Value?.ResetModifiedType();
                    }
                    DataContext.RawDatabase.CommitModifiedEntitys(commitions);
                    if (!string.IsNullOrEmpty(database) && DataContext.Databases.ContainsKey(database))
                    {
                        DataContext.Databases[database].CommitModifiedEntitys(commitions);
                    }
                }
            }
        }

        public IReadOnlyDictionary<EntityKey, T> LoadAll()
        {
            // TODO: LoadAll;
            ReleaseCountdown = Settings.DataReleasePeriod;
            return new Dictionary<EntityKey, T>();
        }

        /// <summary>
        /// 执行释放
        /// </summary>
        public void Release()
        {
            //CommitThread.Abort();
        }
    }
}
