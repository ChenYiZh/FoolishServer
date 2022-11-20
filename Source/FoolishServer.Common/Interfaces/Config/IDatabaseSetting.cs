﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Config
{
    /// <summary>
    /// 数据库设置
    /// </summary>
    public interface IDatabaseSetting
    {
        /// <summary>
        /// 数据库映射名称
        /// </summary>
        string ConnectKey { get; }

        /// <summary>
        /// 数据库连接名称
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        string ConnectionString { get; }
    }
}