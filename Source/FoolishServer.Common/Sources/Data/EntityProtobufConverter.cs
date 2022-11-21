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
