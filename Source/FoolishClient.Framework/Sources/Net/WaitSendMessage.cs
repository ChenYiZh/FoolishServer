using System;
using System.Collections.Generic;
using System.Text;
using FoolishClient.Delegate;
using FoolishClient.Log;
using FoolishGames.Log;

namespace FoolishClient.Net
{
    internal class WaitSendMessage : IWaitSendMessage
    {
        public byte[] Data { get; private set; }

        public SendCallback Callback { get; private set; }

        internal bool Executed { get; set; } = false;

        public WaitSendMessage(byte[] data, SendCallback callback)
        {
            Data = data;
            Callback = callback;
        }

        internal void Execute(bool success, IAsyncResult result)
        {
            if (!Executed)
            {
                Executed = true;
                try
                {
                    Callback?.Invoke(success, result);
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.SOCKET, "Send callback execute error.", e);
                }
            }
        }
    }
}
