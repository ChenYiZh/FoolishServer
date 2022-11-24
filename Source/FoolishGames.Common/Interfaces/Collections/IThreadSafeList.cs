using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Collections
{
    /// <summary>
    /// 线程安全列表接口
    /// </summary>
    public interface IThreadSafeList<T> : IList<T>, IReadOnlyList<T>
    {
    }
}
