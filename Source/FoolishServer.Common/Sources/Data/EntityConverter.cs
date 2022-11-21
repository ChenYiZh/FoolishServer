using FoolishGames.Common;
using FoolishServer.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Data
{
    /// <summary>
    /// 转换的数据类型
    /// </summary>
    public enum EConvertType
    {
        /// <summary>
        /// 无效，抛出异常
        /// </summary>
        InValid = 0,
        /// <summary>
        /// string
        /// </summary>
        String = 1,
        /// <summary>
        /// byte[]
        /// </summary>
        Binary = 2,
    }
    /// <summary>
    /// 数据存储和读取时使用，基类,暂时只支持string和byte[]
    /// </summary>
    public abstract class EntityConverter<T> : IEntityConverter
    {
        /// <summary>
        /// 转换的数据类型
        /// </summary>
        public EConvertType Type { get; protected set; }
            = FType<T>.Type == FType<string>.Type ? EConvertType.String : 
            (FType<T>.Type == FType<byte[]>.Type ? EConvertType.Binary : EConvertType.InValid);
        /// <summary>
        /// 反序列化
        /// </summary>
        public abstract TEntity Deserialize<TEntity>(T data) where TEntity : MajorEntity, new();

        /// <summary>
        /// 序列化
        /// </summary>
        public abstract T Serialize(MajorEntity entity);
    }
}
