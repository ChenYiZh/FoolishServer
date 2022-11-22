using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoolishGames.Collections
{
    public class ThreadSafeList<T> : IThreadSafeList<T>
    {
        #region List+Lock
        private List<T> _cache;

        public readonly object SyncRoot = new object();

        public ThreadSafeList()
        {
            _cache = new List<T>();
        }

        public ThreadSafeList(int capacity)
        {
            _cache = new List<T>(capacity);
        }

        public int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    return _cache.Count;
                }
            }
        }

        public bool IsReadOnly { get { return false; } }

        public T this[int index]
        {
            get
            {
                lock (SyncRoot)
                {
                    return _cache[index];
                }
            }
            set
            {
                lock (SyncRoot)
                {
                    _cache[index] = value;
                }
            }
        }

        public int IndexOf(T item)
        {
            lock (SyncRoot)
            {
                return _cache.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (SyncRoot)
            {
                _cache.Insert(index, item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (SyncRoot)
            {
                _cache.RemoveAt(index);
            }
        }

        public void Add(T item)
        {
            lock (SyncRoot)
            {
                _cache.Add(item);
            }
        }

        public void Clear()
        {
            lock (SyncRoot)
            {
                _cache.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (SyncRoot)
            {
                return _cache.Contains(item);
            }
        }

        public bool Remove(T item)
        {
            lock (SyncRoot)
            {
                return _cache.Remove(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (SyncRoot)
            {
                return _cache.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (SyncRoot)
            {
                _cache.CopyTo(array, arrayIndex);
            }
        }
        #endregion
    }
}
