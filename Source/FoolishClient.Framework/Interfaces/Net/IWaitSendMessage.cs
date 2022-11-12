﻿using FoolishClient.Delegate;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishClient.Net
{
    /// <summary>
    /// 待发送的数据结构
    /// </summary>
    public interface IWaitSendMessage
    {
        /// <summary>
        /// 要发送的数据
        /// </summary>
        byte[] Data { get; }

        /// <summary>
        /// 发送完成的回调
        /// </summary>
        SendCallback Callback { get; }
    }
}
