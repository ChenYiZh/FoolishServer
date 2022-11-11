using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Collections
{
    public interface IThreadSafeQueue<T> : IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
    {
        T Peek();

        T Dequeue();

        void Enqueue(T item);
    }
}
