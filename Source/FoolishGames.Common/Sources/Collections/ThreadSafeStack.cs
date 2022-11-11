using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Collections
{
    public class ThreadSafeStack<T> : IThreadSafeStack<T>
    {
        private Stack<T> _cache;

        private readonly object syncRoot = new object();

        public ThreadSafeStack()
        {
            _cache = new Stack<T>();
        }

        public ThreadSafeStack(int capacity)
        {
            _cache = new Stack<T>(capacity);
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

        public T Pop()
        {
            lock (syncRoot)
            {
                return _cache.Pop();
            }
        }

        public void Push(T item)
        {
            lock (syncRoot)
            {
                _cache.Push(item);
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
