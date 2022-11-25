using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Collections
{
    /// <summary>
    /// 只读队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReadOnlyQueue<T> : IEnumerable<T>, IReadOnlyCollection<T>
    {
        T Dequeue();

        T Peek();

        T[] ToArray();
    }
}
