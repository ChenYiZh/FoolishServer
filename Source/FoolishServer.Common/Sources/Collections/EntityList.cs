using FoolishGames.Collections;
using FoolishGames.Common;
using FoolishServer.Common;
using FoolishServer.Data.Entity;
using FoolishServer.Log;
using FoolishServer.Struct;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Collections
{
    /// <summary>
    /// Model属性使用的列表
    /// </summary>
    public sealed class EntityList<T> : PropertyEntity, IThreadSafeList<T>
    {
        private bool isPropertyEntity = FType<T>.Type.IsSubclassOf(FType<PropertyEntity>.Type);

        private ThreadSafeList<T> List = new ThreadSafeList<T>();

        public T this[int index]
        {
            get
            {
                return List[index];
            }
            set
            {
                if (isPropertyEntity)
                {
                    if (value != null)
                    {
                        List[index] = value;
                        (value as PropertyEntity)?.SetParent(this, Categories.ENTITY);
                    }
                    else
                    {
                        List.RemoveAt(index);
                    }
                }
                else
                {
                    List[index] = value;
                }
                NotifyModified(EModifyType.Modify, PropertyNameInParent);
            }
        }

        public int Count { get { return List.Count; } }

        public bool IsReadOnly { get { return List.IsReadOnly; } }

        public void Add(T item)
        {
            if (isPropertyEntity)
            {
                if (item != null)
                {
                    List.Add(item);
                    (item as PropertyEntity)?.SetParent(this, Categories.ENTITY);
                    NotifyModified(EModifyType.Modify, PropertyNameInParent);
                }
            }
            else
            {
                List.Add(item);
                NotifyModified(EModifyType.Modify, PropertyNameInParent);
            }
        }

        public void Clear()
        {
            List.Clear();
            NotifyModified(EModifyType.Modify, PropertyNameInParent);
        }

        public bool Contains(T item)
        {
            return List.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            List.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return List.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (isPropertyEntity)
            {
                if (item != null)
                {
                    List.Insert(index, item);
                    (item as PropertyEntity)?.SetParent(this, Categories.ENTITY);
                    NotifyModified(EModifyType.Modify, PropertyNameInParent);
                }
            }
            else
            {
                List.Insert(index, item);
                NotifyModified(EModifyType.Modify, PropertyNameInParent);
            }
        }

        public bool Remove(T item)
        {
            bool result = List.Remove(item);
            NotifyModified(EModifyType.Modify, PropertyNameInParent);
            return result;
        }

        public void RemoveAt(int index)
        {
            List.RemoveAt(index);
            NotifyModified(EModifyType.Modify, PropertyNameInParent);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return List.GetEnumerator();
        }
    }
}
