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
using FoolishServer.Collections;
using FoolishServer.Data.Entity;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FoolishServer.Model
{
    [Serializable, JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ProtoContract, EntityTable("default", "{0}_{1:MMdd}")]
    public class Test : MajorEntity
    {
        [EntityField(IsKey = true)]
        [JsonProperty, ProtoMember(1)]
        public long UserId { get; set; }
        [EntityField(IsKey = true)]
        [JsonProperty, ProtoMember(2)]
        public string UserName { get; set; }
        [EntityField(IsIndex = true)]
        [JsonProperty, ProtoMember(3)]
        public string Password { get; set; }
        [EntityField(FieldType = ETableFieldType.Text)]
        [JsonProperty, ProtoMember(4)]
        public EntityDictionary<int, Test2> Tests { get; set; }
        [EntityField(Nullable = false, DefaultValue = 100)]
        [JsonProperty, ProtoMember(5)]
        public long TestID { get; set; }
    }
}

