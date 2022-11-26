using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Collections
{
    public class ThreadSafeStack<T> : IThreadSafeStack<T>
    {
        #region ConcurrentStack
        private ConcurrentStack<T> stack = null;

        public object SyncRoot { get { return this; } }

        public ThreadSafeStack()
        {
            stack = new ConcurrentStack<T>();
        }

        public int Count { get { return stack.Count; } }

        public T Peek()
        {
            T value;
            stack.TryPeek(out value);
            return value;
        }

        public T Pop()
        {
            T value;
            stack.TryPop(out value);
            return value;
        }

        public void Push(T item)
        {
            stack.Push(item);
        }

        public void Clear()
        {
            stack.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return stack.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return stack.GetEnumerator();
        }
        #endregion

        #region Stack+Lock
        //private Stack<T> _cache;

        //public readonly object SyncRoot = new object();

        //public ThreadSafeStack()
        //{
        //    _cache = new Stack<T>();
        //}

        //public ThreadSafeStack(int capacity)
        //{
        //    _cache = new Stack<T>(capacity);
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

        //public T Pop()
        //{
        //    lock (SyncRoot)
        //    {
        //        return _cache.Pop();
        //    }
        //}

        //public void Push(T item)
        //{
        //    lock (SyncRoot)
        //    {
        //        _cache.Push(item);
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
