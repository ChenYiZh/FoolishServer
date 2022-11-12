using FoolishGames.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.IO
{
    /// <summary>
    /// 包含通讯协议的一些内部数据
    /// </summary>
    public class MessageInfo
    {
        /// <summary>
        /// 消息的头数据大小
        /// </summary>
        public static int HeaderLength { get; private set; }

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static MessageInfo()
        {
            HeaderLength = SizeUtil.IntSize//MsgId
                     + SizeUtil.SByteSize//OpCode
                     + SizeUtil.IntSize//ActionId
                     ;
        }
    }
}
