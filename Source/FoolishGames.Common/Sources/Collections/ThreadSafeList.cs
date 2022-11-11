using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoolishGames.Collections
{
    public class ThreadSafeList<T> : IThreadSafeList<T>
    {
        private List<T> _cache;

        private readonly object syncRoot = new object();

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
                lock (syncRoot)
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
                lock (syncRoot)
                {
                    return _cache[index];
                }
            }
            set
            {
                lock (syncRoot)
                {
                    _cache[index] = value;
                }
            }
        }

        public int IndexOf(T item)
        {
            lock (syncRoot)
            {
                return _cache.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (syncRoot)
            {
                _cache.Insert(index, item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (syncRoot)
            {
                _cache.RemoveAt(index);
            }
        }

        public void Add(T item)
        {
            lock (syncRoot)
            {
                _cache.Add(item);
            }
        }

        public void Clear()
        {
            lock (syncRoot)
            {
                _cache.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (syncRoot)
            {
                return _cache.Contains(item);
            }
        }

        public bool Remove(T item)
        {
            lock (syncRoot)
            {
                return _cache.Remove(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
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

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (syncRoot)
            {
                _cache.CopyTo(array, arrayIndex);
            }
        }
    }
}
