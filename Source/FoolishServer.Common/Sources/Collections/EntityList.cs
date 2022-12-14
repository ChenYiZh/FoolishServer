/****************************************************************************
THIS FILE IS PART OF Foolish Server PROJECT
THIS PROGRAM IS FREE SOFTWARE, IS LICENSED UNDER MIT

Copyright (c) 2022-2030 ChenYiZh
https://space.bilibili.com/9308172

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
****************************************************************************/
using FoolishGames.Collections;
using FoolishGames.Common;
using FoolishGames.Timer;
using FoolishServer.Common;
using FoolishServer.Data.Entity;
using FoolishServer.Log;
using FoolishServer.Struct;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Collections
{
    /// <summary>
    /// Model属性使用的列表
    /// </summary>
    [ProtoContract, Serializable]
    public sealed class EntityList<T> : PropertyEntity, IThreadSafeList<T>
    {
        private readonly bool isPropertyEntity = FType<T>.Type.IsSubclassOf(FType<PropertyEntity>.Type);

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

        internal override void SetParent(Entity parent, string propertyName)
        {
            base.SetParent(parent, propertyName);
            if (isPropertyEntity)
            {
                lock (SyncRoot)
                {
                    foreach (T child in List)
                    {
                        PropertyEntity entity;
                        if ((entity = child as PropertyEntity) != null)
                        {
                            entity.ModifiedTime = TimeLord.Now;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 修改已经提交时执行
        /// </summary>
        internal override void OnModificationCommitted()
        {
            base.OnModificationCommitted();
            lock (SyncRoot)
            {
                foreach (T child in List)
                {
                    PropertyEntity entity;
                    if ((entity = child as PropertyEntity) != null)
                    {
                        entity.OnModificationCommitted();
                    }
                }
            }
        }

        /// <summary>
        /// 数据数据库中拉取下来
        /// </summary>
        internal override void OnPulledFromDb()
        {
            base.OnPulledFromDb();
            lock (SyncRoot)
            {
                foreach (T child in List)
                {
                    PropertyEntity entity;
                    if ((entity = child as PropertyEntity) != null)
                    {
                        entity.OnPulledFromDb();
                    }
                }
            }
        }
    }
}
