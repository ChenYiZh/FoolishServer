using FoolishGames.Common;
using FoolishServer.Data.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Common
{
    /// <summary>
    /// 数据表类型转换
    /// </summary>
    public static class TableFieldConverter
    {
        /// <summary>
        /// 转换成枚举
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ETableFieldType ConvertFromType(Type type)
        {
            if (type == FType<bool>.Type)
            {
                return ETableFieldType.Bit;
            }
            else if (type == FType<byte>.Type)
            {
                return ETableFieldType.Byte;
            }
            else if (type == FType<char>.Type)
            {
                return ETableFieldType.Byte;
            }
            else if (type == FType<double>.Type)
            {
                return ETableFieldType.Double;
            }
            else if (type == FType<float>.Type)
            {
                return ETableFieldType.Float;
            }
            else if (type == FType<int>.Type)
            {
                return ETableFieldType.Int;
            }
            else if (type == FType<long>.Type)
            {
                return ETableFieldType.Long;
            }
            else if (type == FType<sbyte>.Type)
            {
                return ETableFieldType.SByte;
            }
            else if (type == FType<short>.Type)
            {
                return ETableFieldType.Short;
            }
            else if (type == FType<uint>.Type)
            {
                return ETableFieldType.UInt;
            }
            else if (type == FType<ulong>.Type)
            {
                return ETableFieldType.ULong;
            }
            else if (type == FType<ushort>.Type)
            {
                return ETableFieldType.UShort;
            }
            else if (type == FType<string>.Type)
            {
                return ETableFieldType.String;
            }
            else if (type == FType<DateTime>.Type)
            {
                return ETableFieldType.DateTime;
            }
            else if (type == FType<TimeSpan>.Type)
            {
                return ETableFieldType.Long;
            }
            else if (type == FType<byte[]>.Type)
            {
                return ETableFieldType.Blob;
            }
            else if (type.IsSubInterfaceOf(typeof(IList<>)))
            {
                return ETableFieldType.LongText;
            }
            else if (type.IsSubInterfaceOf(typeof(IDictionary<,>)))
            {
                return ETableFieldType.LongText;
            }
            return ETableFieldType.Error;
        }
    }
}
