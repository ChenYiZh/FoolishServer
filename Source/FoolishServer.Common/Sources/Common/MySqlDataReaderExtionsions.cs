﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Common
{
    /// <summary>
    /// MySqlDataReader拓展防空
    /// </summary>
    public static class MySqlDataReaderExtionsions
    {
        /// <summary>
        /// 泛型获取数据
        /// </summary>
        public static T GetFieldValue<T>(this MySqlDataReader reader, string columnName, T defaultValue)
        {
            if (reader == null || reader.IsClosed)
            {
                return defaultValue;
            }
            int index = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(index))
            {
                return defaultValue;
            }
            return reader.GetFieldValue<T>(index);
        }
        /// <summary>
        /// 获取字符串
        /// </summary>
        public static string GetString(this MySqlDataReader reader, string columnName, string defaultValue)
        {
            if (reader == null || reader.IsClosed)
            {
                return defaultValue;
            }
            int index = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(index))
            {
                return defaultValue;
            }
            return reader.GetString(index);
        }
        /// <summary>
        /// 获取bool
        /// </summary>
        public static bool GetBoolean(this MySqlDataReader reader, string columnName, bool defaultValue)
        {
            if (reader == null || reader.IsClosed)
            {
                return defaultValue;
            }
            int index = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(index))
            {
                return defaultValue;
            }
            return reader.GetBoolean(index);
        }
        /// <summary>
        /// 获取byte
        /// </summary>
        public static byte GetByte(this MySqlDataReader reader, string columnName, byte defaultValue)
        {
            if (reader == null || reader.IsClosed)
            {
                return defaultValue;
            }
            int index = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(index))
            {
                return defaultValue;
            }
            return reader.GetByte(index);
        }
        /// <summary>
        /// 获取short
        /// </summary>
        public static short GetInt16(this MySqlDataReader reader, string columnName, short defaultValue)
        {
            if (reader == null || reader.IsClosed)
            {
                return defaultValue;
            }
            int index = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(index))
            {
                return defaultValue;
            }
            return reader.GetInt16(index);
        }
        /// <summary>
        /// 获取int
        /// </summary>
        public static int GetInt32(this MySqlDataReader reader, string columnName, int defaultValue)
        {
            if (reader == null || reader.IsClosed)
            {
                return defaultValue;
            }
            int index = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(index))
            {
                return defaultValue;
            }
            return reader.GetInt32(index);
        }
        /// <summary>
        /// 获取long
        /// </summary>
        public static long GetInt64(this MySqlDataReader reader, string columnName, long defaultValue)
        {
            if (reader == null || reader.IsClosed)
            {
                return defaultValue;
            }
            int index = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(index))
            {
                return defaultValue;
            }
            return reader.GetInt64(index);
        }
        /// <summary>
        /// 获取sbyte
        /// </summary>
        public static sbyte GetSByte(this MySqlDataReader reader, string columnName, sbyte defaultValue)
        {
            if (reader == null || reader.IsClosed)
            {
                return defaultValue;
            }
            int index = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(index))
            {
                return defaultValue;
            }
            return reader.GetSByte(index);
        }
        /// <summary>
        /// 获取ushort
        /// </summary>
        public static ushort GetUInt16(this MySqlDataReader reader, string columnName, ushort defaultValue)
        {
            if (reader == null || reader.IsClosed)
            {
                return defaultValue;
            }
            int index = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(index))
            {
                return defaultValue;
            }
            return reader.GetUInt16(index);
        }
        /// <summary>
        /// 获取uint
        /// </summary>
        public static uint GetUInt32(this MySqlDataReader reader, string columnName, uint defaultValue)
        {
            if (reader == null || reader.IsClosed)
            {
                return defaultValue;
            }
            int index = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(index))
            {
                return defaultValue;
            }
            return reader.GetUInt32(index);
        }
        /// <summary>
        /// 获取ulong
        /// </summary>
        public static ulong GetUInt64(this MySqlDataReader reader, string columnName, ulong defaultValue)
        {
            if (reader == null || reader.IsClosed)
            {
                return defaultValue;
            }
            int index = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(index))
            {
                return defaultValue;
            }
            return reader.GetUInt64(index);
        }
        /// <summary>
        /// 获取float
        /// </summary>
        public static float GetFloat(this MySqlDataReader reader, string columnName, float defaultValue)
        {
            if (reader == null || reader.IsClosed)
            {
                return defaultValue;
            }
            int index = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(index))
            {
                return defaultValue;
            }
            return reader.GetFloat(index);
        }
        /// <summary>
        /// 获取double
        /// </summary>
        public static double GetDouble(this MySqlDataReader reader, string columnName, double defaultValue)
        {
            if (reader == null || reader.IsClosed)
            {
                return defaultValue;
            }
            int index = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(index))
            {
                return defaultValue;
            }
            return reader.GetDouble(index);
        }
        /// <summary>
        /// 获取DateTime
        /// </summary>
        public static DateTime GetDateTime(this MySqlDataReader reader, string columnName, DateTime defaultValue)
        {
            if (reader == null || reader.IsClosed)
            {
                return defaultValue;
            }
            int index = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(index))
            {
                return defaultValue;
            }
            return reader.GetDateTime(index);
        }
        /// <summary>
        /// 获取TimeSpan
        /// </summary>
        public static TimeSpan GetTimeSpan(this MySqlDataReader reader, string columnName, TimeSpan defaultValue)
        {
            long ticks = reader.GetInt64(columnName, defaultValue.Ticks);
            return new TimeSpan(ticks);
        }
    }
}
