﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FoolishGames.Collections
{
    public class ThreadSafeDictionary<TKey, TValue> : IThreadSafeDictionary<TKey, TValue>
    {
        #region ConcurrentDictionary 
        private ConcurrentDictionary<TKey, TValue> dictionary = new ConcurrentDictionary<TKey, TValue>();

        public object SyncRoot { get { return this; } }

        public ThreadSafeDictionary()
        {
            dictionary = new ConcurrentDictionary<TKey, TValue>();
        }

        public ThreadSafeDictionary(int capacity)
        {
            dictionary = new ConcurrentDictionary<TKey, TValue>(Environment.ProcessorCount, capacity);
        }

        public ThreadSafeDictionary(IDictionary<TKey, TValue> dictionary)
        {
            dictionary = new ConcurrentDictionary<TKey, TValue>(dictionary);
        }

        public TValue this[TKey key]
        {
            get { return dictionary[key]; }
            set { dictionary[key] = value; }
        }

        TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] { get { return dictionary[key]; } }

        public ICollection<TKey> Keys { get { return dictionary.Keys; } }

        public ICollection<TValue> Values { get { return dictionary.Values; } }

        public int Count { get { return dictionary.Count; } }

        public bool IsReadOnly { get { return false; } }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys { get { return dictionary.Keys; } }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values { get { return dictionary.Values; } }

        public void Add(TKey key, TValue value)
        {
            dictionary.TryAdd(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            dictionary.TryAdd(item.Key, item.Value);
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return dictionary.Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        public void OnDeserialization(object sender)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TKey key)
        {
            TValue value;
            return dictionary.TryRemove(key, out value);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            TValue value;
            return dictionary.TryRemove(item.Key, out value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }
        #endregion
        #region Dictionary+Lock
        //private Dictionary<TKey, TValue> _cache;

        //public readonly object SyncRoot = new object();

        //public ThreadSafeDictionary()
        //{
        //    _cache = new Dictionary<TKey, TValue>();
        //}

        //public ThreadSafeDictionary(int capacity)
        //{
        //    _cache = new Dictionary<TKey, TValue>(capacity);
        //}

        //public ThreadSafeDictionary(IDictionary<TKey, TValue> dictionary)
        //{
        //    _cache = new Dictionary<TKey, TValue>(dictionary);
        //}

        //public ICollection<TKey> Keys
        //{
        //    get
        //    {
        //        lock (SyncRoot)
        //        {
        //            return _cache.Keys.ToList();
        //        }
        //    }
        //}

        //public ICollection<TValue> Values
        //{
        //    get
        //    {
        //        lock (SyncRoot)
        //        {
        //            return _cache.Values.ToList();
        //        }
        //    }
        //}

        //public int Count
        //{
        //    get
        //    {
        //        lock (SyncRoot)
        //        {
        //            return _cache.Count;
        //        }
        //    }
        //}

        //public bool IsReadOnly { get { return false; } }

        //IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        //{
        //    get
        //    {
        //        lock (SyncRoot)
        //        {
        //            return _cache.Keys;
        //        }
        //    }
        //}

        //IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        //{
        //    get
        //    {
        //        lock (SyncRoot)
        //        {
        //            return _cache.Values;
        //        }
        //    }
        //}

        //public TValue this[TKey key]
        //{
        //    get
        //    {
        //        lock (SyncRoot)
        //        {
        //            return _cache[key];
        //        }
        //    }
        //    set
        //    {
        //        lock (SyncRoot)
        //        {
        //            _cache[key] = value;
        //        }
        //    }
        //}

        //public bool ContainsKey(TKey key)
        //{
        //    lock (SyncRoot)
        //    {
        //        return _cache.ContainsKey(key);
        //    }
        //}

        //public void Add(TKey key, TValue value)
        //{
        //    lock (SyncRoot)
        //    {
        //        _cache.Add(key, value);
        //    }
        //}

        //public bool Remove(TKey key)
        //{
        //    lock (SyncRoot)
        //    {
        //        return _cache.Remove(key);
        //    }
        //}

        //public bool TryGetValue(TKey key, out TValue value)
        //{
        //    lock (SyncRoot)
        //    {
        //        return _cache.TryGetValue(key, out value);
        //    }
        //}

        //public void Add(KeyValuePair<TKey, TValue> item)
        //{
        //    lock (SyncRoot)
        //    {
        //        _cache.Add(item.Key, item.Value);
        //    }
        //}

        //public void Clear()
        //{
        //    lock (SyncRoot)
        //    {
        //        _cache.Clear();
        //    }
        //}

        //public bool Contains(KeyValuePair<TKey, TValue> item)
        //{
        //    lock (SyncRoot)
        //    {
        //        return _cache.Contains(item);
        //    }
        //}

        //public bool Remove(KeyValuePair<TKey, TValue> item)
        //{
        //    lock (SyncRoot)
        //    {
        //        return _cache.Remove(item.Key);
        //    }
        //}

        //public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        //{
        //    lock (SyncRoot)
        //    {
        //        return _cache.GetEnumerator();
        //    }
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return GetEnumerator();
        //}

        //public void OnDeserialization(object sender)
        //{
        //    lock (SyncRoot)
        //    {
        //        _cache.OnDeserialization(sender);
        //    }
        //}

        //public void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    lock (SyncRoot)
        //    {
        //        _cache.GetObjectData(info, context);
        //    }
        //}

        //public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        //{
        //    throw new NotImplementedException();
        //}
        #endregion
    }
}
