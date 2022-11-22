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

        #region HashSet+Lock
        private HashSet<T> _cache;

        public readonly object SyncRoot = new object();

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
                lock (SyncRoot)
                {
                    return _cache.Count;
                }
            }
        }

        public bool IsReadOnly { get { return false; } }

        public bool Add(T item)
        {
            lock (SyncRoot)
            {
                return _cache.Add(item);
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            lock (SyncRoot)
            {
                _cache.UnionWith(other);
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            lock (SyncRoot)
            {
                _cache.IntersectWith(other);
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            lock (SyncRoot)
            {
                _cache.ExceptWith(other);
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            lock (SyncRoot)
            {
                _cache.SymmetricExceptWith(other);
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            lock (SyncRoot)
            {
                return _cache.IsSubsetOf(other);
            }
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            lock (SyncRoot)
            {
                return _cache.IsSupersetOf(other);
            }
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            lock (SyncRoot)
            {
                return _cache.IsProperSupersetOf(other);
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            lock (SyncRoot)
            {
                return _cache.IsProperSubsetOf(other);
            }
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            lock (SyncRoot)
            {
                return _cache.Overlaps(other);
            }
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            lock (SyncRoot)
            {
                return _cache.SetEquals(other);
            }
        }

        void ICollection<T>.Add(T item)
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

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (SyncRoot)
            {
                _cache.CopyTo(array, arrayIndex);
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

        public void OnDeserialization(object sender)
        {
            lock (SyncRoot)
            {
                _cache.OnDeserialization(sender);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            lock (SyncRoot)
            {
                _cache.GetObjectData(info, context);
            }
        }
        #endregion
    }
}
