using FoolishServer.Data.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Model
{
    [Serializable]
    [ProtoContract]
    public class Test2 : MinorEntity
    {
        [EntityField, ProtoMember(1)]
        public string TestId { get; set; }
    }
}
