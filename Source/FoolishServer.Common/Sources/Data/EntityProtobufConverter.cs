﻿/****************************************************************************
THIS FILE IS PART OF Foolish Server PROJECT
THIS PROGRAM IS FREE SOFTWARE, IS LICENSED UNDER MIT

Copyright (c) 2022-2025 ChenYiZh
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FoolishGames.Log;
using FoolishServer.Data.Entity;
using FoolishServer.Log;
using ProtoBuf;

namespace FoolishServer.Data
{
    /// <summary>
    /// 通过Protobuf解析
    /// </summary>
    public class EntityProtobufConverter : EntityConverter<byte[]>
    {
        public override TEntity Deserialize<TEntity>(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                return Serializer.Deserialize<TEntity>(stream);
            }
        }

        public override byte[] Serialize(MajorEntity entity)
        {
            if (entity == null)
            {
                FConsole.WriteErrorWithCategory(Categories.ENTITY, "An empty entity appearred on serializing.");
                return null;
            }
            using (MemoryStream stream = new MemoryStream())
            {
                Serializer.Serialize(stream, entity);
                return stream.ToArray();
            }
        }
    }
}
