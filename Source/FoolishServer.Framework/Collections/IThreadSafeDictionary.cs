using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FoolishServer.Framework.Collections
{
    public interface IThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDeserializationCallback, ISerializable
    {
    }
}
