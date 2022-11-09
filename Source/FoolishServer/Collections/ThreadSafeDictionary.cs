using FoolishServer.Framework.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FoolishServer.Collections
{
    public class ThreadSafeDictionary<TKey, TValue> : IThreadSafeDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _cache;

        private readonly object syncRoot = new object();

        public ThreadSafeDictionary()
        {
            _cache = new Dictionary<TKey, TValue>();
        }

        public ThreadSafeDictionary(int capacity)
        {
            _cache = new Dictionary<TKey, TValue>(capacity);
        }

        public ICollection<TKey> Keys
        {
            get
            {
                lock (syncRoot)
                {
                    return _cache.Keys.ToList();
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                lock (syncRoot)
                {
                    return _cache.Values.ToList();
                }
            }
        }

        public int Count
        {
            get
            {
                lock (syncRoot)
                {
                    return _cache.Count;
                }
            }
        }

        public bool IsReadOnly { get { return false; } }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get
            {
                lock (syncRoot)
                {
                    return _cache.Keys;
                }
            }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            {
                lock (syncRoot)
                {
                    return _cache.Values;
                }
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (syncRoot)
                {
                    return _cache[key];
                }
            }
            set
            {
                lock (syncRoot)
                {
                    _cache[key] = value;
                }
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (syncRoot)
            {
                return _cache.ContainsKey(key);
            }
        }

        public void Add(TKey key, TValue value)
        {
            lock (syncRoot)
            {
                _cache.Add(key, value);
            }
        }

        public bool Remove(TKey key)
        {
            lock (syncRoot)
            {
                return _cache.Remove(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (syncRoot)
            {
                return _cache.TryGetValue(key, out value);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (syncRoot)
            {
                _cache.Add(item.Key, item.Value);
            }
        }

        public void Clear()
        {
            lock (syncRoot)
            {
                _cache.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (syncRoot)
            {
                return _cache.Contains(item);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (syncRoot)
            {
                return _cache.Remove(item.Key);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (syncRoot)
            {
                return _cache.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void OnDeserialization(object sender)
        {
            lock (syncRoot)
            {
                _cache.OnDeserialization(sender);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            lock (syncRoot)
            {
                _cache.GetObjectData(info, context);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
    }
}
