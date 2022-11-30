/****************************************************************************
THIS FILE IS PART OF Foolish Server PROJECT
THIS PROGRAM IS FREE SOFTWARE, IS LICENSED UNDER MIT

Copyright (c) 2022-2025 ChenYiZh
https://space.bilibili.com/9308172

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
****************************************************************************/
using FoolishGames.Collections;
using FoolishGames.Common;
using FoolishGames.Log;
using FoolishGames.Timer;
using FoolishServer.Common;
using FoolishServer.Data.Entity;
using FoolishServer.Log;
using FoolishServer.Struct;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FoolishServer.Collections
{
    /// <summary>
    /// Model层使用的数据字典
    /// </summary>
    [ProtoContract, Serializable]
    public sealed class EntityDictionary<TKey, TValue> : PropertyEntity, IThreadSafeDictionary<TKey, TValue> where TKey : struct
    {
        private readonly bool isPropertyEntity = FType<TValue>.Type.IsSubclassOf(FType<PropertyEntity>.Type);

        private ThreadSafeDictionary<TKey, TValue> Dictionary = new ThreadSafeDictionary<TKey, TValue>();

        public TValue this[TKey key]
        {
            get { return Dictionary[key]; }
            set
            {
                if (isPropertyEntity)
                {
                    if (value != null)
                    {
                        Dictionary[key] = value;
                        (value as PropertyEntity)?.SetParent(this, Categories.ENTITY);
                    }
                    else
                    {
                        Dictionary.Remove(key);
                    }
                }
                else
                {
                    Dictionary[key] = value;
                }
                NotifyModified(EModifyType.Modify, PropertyNameInParent);
            }
        }

        public ICollection<TKey> Keys { get { return Dictionary.Keys; } }

        public ICollection<TValue> Values { get { return Dictionary.Values; } }

        public int Count { get { return Dictionary.Count; } }

        public bool IsReadOnly { get { return Dictionary.IsReadOnly; } }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys { get { return Dictionary.Keys; } }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values { get { return Dictionary.Values; } }

        public void Add(TKey key, TValue value)
        {
            if (isPropertyEntity)
            {
                if (value != null)
                {
                    Dictionary.Add(key, value);
                    (value as PropertyEntity)?.SetParent(this, Categories.ENTITY);
                    NotifyModified(EModifyType.Modify, PropertyNameInParent);
                }
            }
            else
            {
                Dictionary.Add(key, value);
                NotifyModified(EModifyType.Modify, PropertyNameInParent);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (isPropertyEntity)
            {
                if (item.Value != null)
                {
                    Dictionary.Add(item);
                    (item.Value as PropertyEntity)?.SetParent(this, Categories.ENTITY);
                    NotifyModified(EModifyType.Modify, PropertyNameInParent);
                }
            }
            else
            {
                Dictionary.Add(item);
                NotifyModified(EModifyType.Modify, PropertyNameInParent);
            }
        }

        public void Clear()
        {
            Dictionary.Clear();
            NotifyModified(EModifyType.Modify, PropertyNameInParent);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return Dictionary.Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return Dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            Dictionary.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Dictionary.GetObjectData(info, context);
        }

        public void OnDeserialization(object sender)
        {
            Dictionary.OnDeserialization(sender);
        }

        public bool Remove(TKey key)
        {
            bool result = Dictionary.Remove(key);
            NotifyModified(EModifyType.Modify, PropertyNameInParent);
            return result;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            bool result = Dictionary.Remove(item);
            NotifyModified(EModifyType.Modify, PropertyNameInParent);
            return result;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        internal override void SetParent(Entity parent, string propertyName)
        {
            base.SetParent(parent, propertyName);
            if (isPropertyEntity)
            {
                lock (SyncRoot)
                {
                    foreach (TValue child in Dictionary.Values)
                    {
                        PropertyEntity entity;
                        if ((entity = child as PropertyEntity) != null)
                        {
                            entity.ModifiedTime = TimeLord.Now;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 修改已经提交时执行
        /// </summary>
        internal override void OnModificationCommitted()
        {
            base.OnModificationCommitted();
            lock (SyncRoot)
            {
                foreach (TValue child in Dictionary.Values)
                {
                    PropertyEntity entity;
                    if ((entity = child as PropertyEntity) != null)
                    {
                        entity.OnModificationCommitted();
                    }
                }
            }
        }

        /// <summary>
        /// 数据数据库中拉取下来
        /// </summary>
        internal override void OnPulledFromDb()
        {
            base.OnPulledFromDb();
            lock (SyncRoot)
            {
                foreach (TValue child in Dictionary.Values)
                {
                    PropertyEntity entity;
                    if ((entity = child as PropertyEntity) != null)
                    {
                        entity.OnPulledFromDb();
                    }
                }
            }
        }
    }
}
