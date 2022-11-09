using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FoolishServer.Framework.Collections
{
    public interface IThreadSafeHashSet<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ISet<T>, IDeserializationCallback, ISerializable
    {
    }
}
