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
    [ProtoContract, EntityTable]
    public class Test : MajorEntity
    {
        [EntityField(IsKey = true)]
        [JsonProperty, ProtoMember(1)]
        public long UserId { get; set; }
        [EntityField(IsKey = true)]
        [JsonProperty, ProtoMember(2)]
        public string UserName { get; set; }
        [EntityField]
        [JsonProperty, ProtoMember(3)]
        public string Password { get; set; }
        [EntityField]
        [JsonProperty, ProtoMember(4)]
        public EntityDictionary<int, Test2> Tests { get; set; }

        private long testStr;
        public long TestStr
        {
            get
            {
                lock (SyncRoot)
                {
                    return testStr;
                }
            }
            set
            {
                bool locked = false;
                Monitor.TryEnter(SyncRoot, 1000, ref locked);
                lock (SyncRoot)
                {
                    if (!object.Equals(testStr, value))
                    {
                        object temp = (object)testStr;
                        testStr = value;
                        NotifyPropertyModified("testStr", temp, (object)testStr);
                    }
                }
            }
        }
    }
}

