using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Sources.Common
{
    /// <summary>
    /// 随机数管理
    /// </summary>
    public static class RandomUtil
    {
        /// <summary>
        /// 种子
        /// </summary>
        public static Random Seed { get; set; }

        static RandomUtil()
        {
            Seed = new Random((int)(DateTime.Now.Ticks % int.MaxValue));
        }

        /// <summary>
        /// 随机一个字节
        /// </summary>
        /// <returns></returns>
        public static byte RandomByte()
        {
            return (byte)Seed.Next(byte.MinValue, byte.MaxValue);
        }

        /// <summary>
        /// 范围随机
        /// </summary>
        public static int RandomRange(int min, int max)
        {
            return Seed.Next(min, max);
        }

        /// <summary>
        /// 范围随机
        /// </summary>
        public static float RandomRange(float min, float max)
        {
            return (float)((max - min) * Random() + min);
        }

        /// <summary>
        /// 范围随机
        /// </summary>
        public static double RandomRange(double min, double max)
        {
            return (max - min) * Random() + min;
        }

        /// <summary>
        /// 随机数
        /// </summary>
        public static double Random()
        {
            return Seed.NextDouble();
        }
    }
}
