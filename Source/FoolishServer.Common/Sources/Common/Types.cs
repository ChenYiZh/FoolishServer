using FoolishServer.Data.Entity;
using FoolishServer.Struct;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Common
{
    /// <summary>
    /// Type缓存
    /// </summary>
    internal static class Types
    {
        //主类缓存
        public static Type MajorEntity = typeof(MajorEntity);
        //子类缓存
        public static Type PropertyEntity = typeof(PropertyEntity);
    }
}
