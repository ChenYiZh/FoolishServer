using FoolishGames.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Timer
{
    /// <summary>
    /// 时间管理类
    /// </summary>
    public static class TimeLord
    {
        /// <summary>
        /// 当前使用的计时控件
        /// </summary>
        public static IPacketWatch PacketWatch { get; private set; } = null;

        /// <summary>
        /// 锁
        /// </summary>
        private static object syncRoot = new object();

        /// <summary>
        /// 设置时间控件
        /// </summary>
        /// <param name="watch"></param>
        public static void SetPacketWatch(IPacketWatch watch)
        {
            lock (syncRoot)
            {
                PacketWatch = watch;
            }
        }

        /// <summary>
        /// 获取当前时间
        /// </summary>
        public static DateTime Now
        {
            get
            {
                try
                {
                    //lock (syncRoot)
                    //{
                    if (PacketWatch == null)
                    {
                        return DateTime.Now;
                    }
                    else
                    {
                        return PacketWatch.Now;
                    }
                    //}
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.TIME_LORD, e);
                    return DateTime.Now;
                }
            }
        }

        /// <summary>
        /// 获取当前时间
        /// </summary>
        public static DateTime UTC
        {
            get
            {
                try
                {
                    //lock (syncRoot)
                    //{
                    if (PacketWatch == null)
                    {
                        return DateTime.UtcNow;
                    }
                    else
                    {
                        return PacketWatch.UTC;
                    }
                    //}
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.TIME_LORD, e);
                    return DateTime.UtcNow;
                }
            }
        }
    }
}
