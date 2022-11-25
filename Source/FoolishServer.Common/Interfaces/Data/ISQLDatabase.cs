using FoolishServer.Config;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace FoolishServer.Data
{
    /// <summary>
    /// SQL标准数据库
    /// </summary>
    public interface ISQLDatabase : IDatabase
    {
        /// <summary>
        /// 设置配置文件，初始化时执行
        /// </summary>
        void SetSettings(IDatabaseSetting setting);

        /// <summary>
        /// 对表结构进行调整或创建
        /// </summary>
        void GenerateOrUpdateTableScheme(ITableScheme table);
    }
}
