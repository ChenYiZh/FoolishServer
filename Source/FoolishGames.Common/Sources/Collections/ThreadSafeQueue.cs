using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Collections
{
    public class ThreadSafeQueue<T> : IThreadSafeQueue<T>
    {
        #region ConcurrentQueue
        private ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

        public object SyncRoot { get { return this; } }

        public int Count { get { return queue.Count; } }

        public void Clear()
        {
            while (!queue.IsEmpty)
            {
                T value;
                queue.TryDequeue(out value);
            }
        }

        public T Dequeue()
        {
            T value;
            queue.TryDequeue(out value);
            return value;
        }

        public void Enqueue(T item)
        {
            queue.Enqueue(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return queue.GetEnumerator();
        }

        public T Peek()
        {
            T value;
            queue.TryPeek(out value);
            return value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return queue.GetEnumerator();
        }
        #endregion

        #region Queue+Lock
        //private Queue<T> _cache;

        //public readonly object SyncRoot = new object();

        //public int Count { get { return _cache.Count; } }

        //public ThreadSafeQueue()
        //{
        //    _cache = new Queue<T>();
        //}

        //public ThreadSafeQueue(int capacity)
        //{
        //    _cache = new Queue<T>(capacity);
        //}

        //public T Dequeue()
        //{
        //    lock (SyncRoot)
        //    {
        //        return _cache.Dequeue();
        //    }
        //}

        //public void Enqueue(T item)
        //{
        //    lock (SyncRoot)
        //    {
        //        _cache.Enqueue(item);
        //    }
        //}

        //public IEnumerator<T> GetEnumerator()
        //{
        //    lock (SyncRoot)
        //    {
        //        return _cache.GetEnumerator();
        //    }
        //}

        //public T Peek()
        //{
        //    lock (SyncRoot)
        //    {
        //        return _cache.Peek();
        //    }
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    lock (SyncRoot)
        //    {
        //        return _cache.GetEnumerator();
        //    }
        //}

        //public void Clear()
        //{
        //    lock (SyncRoot)
        //    {
        //        _cache.Clear();
        //    }
        //}
        #endregion
    }
}
