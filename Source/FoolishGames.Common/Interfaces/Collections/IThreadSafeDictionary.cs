using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FoolishGames.Collections
{
    /// <summary>
    /// 线程安全字典接口
    /// </summary>
    public interface IThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDeserializationCallback, ISerializable
    {
    }
}
