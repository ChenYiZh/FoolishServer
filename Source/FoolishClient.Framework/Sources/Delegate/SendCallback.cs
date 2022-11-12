using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishClient.Delegate
{
    /// <summary>
    /// 数据发送的回调
    /// </summary>
    /// <param name="success">操作是否成功，不包含结果</param>
    /// <param name="result"></param>
    public delegate void SendCallback(bool success, IAsyncResult result);
}
