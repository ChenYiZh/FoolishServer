using System;
using System.Collections.Generic;
using System.Text;
using FoolishServer.Data.Entity;

namespace FoolishServer.Data
{
    /// <summary>
    /// 数据库操作
    /// </summary>
    public abstract class Database : IDatabase
    {
        public bool Connected => throw new NotImplementedException();

        public void Close()
        {
            throw new NotImplementedException();
        }

        public bool Connect()
        {
            throw new NotImplementedException();
        }

        public bool RemoveEntity(MajorEntity entity)
        {
            throw new NotImplementedException();
        }

        public bool SaveOrAddEntity(MajorEntity entity)
        {
            throw new NotImplementedException();
        }

        public bool ModifyEntitys(ICollection<MajorEntity> entities)
        {
            throw new NotImplementedException();
        }
    }
}
