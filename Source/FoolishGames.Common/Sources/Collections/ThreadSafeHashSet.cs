using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FoolishGames.Collections
{
    public class ThreadSafeHashSet<T> : IThreadSafeHashSet<T>
    {
        private HashSet<T> _cache;

        private readonly object syncRoot = new object();

        public ThreadSafeHashSet()
        {
            _cache = new HashSet<T>();
        }

        //public ThreadSafeHashSet(int capacity)
        //{
        //    _cache = new HashSet<T>(capacity);
        //}

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

        public bool Add(T item)
        {
            lock (syncRoot)
            {
                return _cache.Add(item);
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            lock (syncRoot)
            {
                _cache.UnionWith(other);
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            lock (syncRoot)
            {
                _cache.IntersectWith(other);
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            lock (syncRoot)
            {
                _cache.ExceptWith(other);
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            lock (syncRoot)
            {
                _cache.SymmetricExceptWith(other);
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            lock (syncRoot)
            {
                return _cache.IsSubsetOf(other);
            }
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            lock (syncRoot)
            {
                return _cache.IsSupersetOf(other);
            }
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            lock (syncRoot)
            {
                return _cache.IsProperSupersetOf(other);
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            lock (syncRoot)
            {
                return _cache.IsProperSubsetOf(other);
            }
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            lock (syncRoot)
            {
                return _cache.Overlaps(other);
            }
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            lock (syncRoot)
            {
                return _cache.SetEquals(other);
            }
        }

        void ICollection<T>.Add(T item)
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

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (syncRoot)
            {
                _cache.CopyTo(array, arrayIndex);
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
    }
}
