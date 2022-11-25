using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Collections
{
    /// <summary>
    /// 特殊队列，继承只读队列的接口
    /// </summary>
    public class TQueue<T> : Queue<T>, IReadOnlyQueue<T>
    {
        public TQueue() : base() { }

        public TQueue(IEnumerable<T> collection) : base(collection) { }

        public TQueue(int capacity) : base(capacity) { }
    }
}
