using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.RPC.Sockets
{
    internal struct SummaryStatus
    {
        /// <summary>
        /// 
        /// </summary>
        internal long TotalConnectCount;
        /// <summary>
        /// 
        /// </summary>
        internal long CurrentConnectCount;
        /// <summary>
        /// 
        /// </summary>
        internal long RejectedConnectCount;
        /// <summary>
        /// 
        /// </summary>
        internal long CloseConnectCount;
    }
}
