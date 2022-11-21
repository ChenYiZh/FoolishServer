using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Collections
{
    public class ThreadSafeQueue<T> : IThreadSafeQueue<T>
    {
        private Queue<T> _cache;

        public readonly object SyncRoot = new object();

        public int Count { get { return _cache.Count; } }

        public ThreadSafeQueue()
        {
            _cache = new Queue<T>();
        }

        public ThreadSafeQueue(int capacity)
        {
            _cache = new Queue<T>(capacity);
        }

        public T Dequeue()
        {
            lock (SyncRoot)
            {
                return _cache.Dequeue();
            }
        }

        public void Enqueue(T item)
        {
            lock (SyncRoot)
            {
                _cache.Enqueue(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (SyncRoot)
            {
                return _cache.GetEnumerator();
            }
        }

        public T Peek()
        {
            lock (SyncRoot)
            {
                return _cache.Peek();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (SyncRoot)
            {
                return _cache.GetEnumerator();
            }
        }

        public void Clear()
        {
            lock (SyncRoot)
            {
                _cache.Clear();
            }
        }
    }
}
