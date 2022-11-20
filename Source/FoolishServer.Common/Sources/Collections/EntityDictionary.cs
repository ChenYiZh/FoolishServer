﻿using FoolishGames.Collections;
using FoolishGames.Common;
using FoolishServer.Common;
using FoolishServer.Data.Entity;
using FoolishServer.Log;
using FoolishServer.Struct;
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
    public sealed class EntityDictionary<TKey, TValue> : PropertyEntity, IThreadSafeDictionary<TKey, TValue> where TKey : struct
    {
        private bool isPropertyEntity = FType<TValue>.Type.IsSubclassOf(FType<PropertyEntity>.Type);

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
    }
}