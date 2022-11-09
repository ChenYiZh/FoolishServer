using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Framework.Collections
{
    public interface IThreadSafeList<T> : IList<T>, IReadOnlyList<T>
    {
    }
}
