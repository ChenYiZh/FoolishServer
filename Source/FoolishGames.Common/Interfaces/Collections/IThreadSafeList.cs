using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Collections
{
    public interface IThreadSafeList<T> : IList<T>, IReadOnlyList<T>
    {
    }
}
