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
        [EntityField]
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
                object syncRoot = SyncRoot;
                bool lockTaken = false;
                try
                {
                    Monitor.TryEnter(syncRoot, 1000, ref lockTaken);
                    return testStr;
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(syncRoot);
                    }
                }
            }
            set
            {
                bool equals = true;
                object oldValue = null;
                object syncRoot = SyncRoot;
                bool lockTaken = false;
                try
                {
                    Monitor.TryEnter(syncRoot, 1000, ref lockTaken);
                    equals = object.Equals(testStr, value);
                    if (!equals)
                    {
                        oldValue = testStr;
                        testStr = value;
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(syncRoot);
                    }
                }
                if (!equals)
                {
                    NotifyPropertyModified("TestStr", oldValue, value);
                }
            }
        }
    }
}

