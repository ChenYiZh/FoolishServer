using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Collections
{
    public class ThreadSafeQueue<T> : IThreadSafeQueue<T>
    {
        private Queue<T> _cache;

        private object syncRoot = new object();

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
            lock (syncRoot)
            {
                return _cache.Dequeue();
            }
        }

        public void Enqueue(T item)
        {
            lock (syncRoot)
            {
                _cache.Enqueue(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (syncRoot)
            {
                return _cache.GetEnumerator();
            }
        }

        public T Peek()
        {
            lock (syncRoot)
            {
                return _cache.Peek();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (syncRoot)
            {
                return _cache.GetEnumerator();
            }
        }
    }
}
