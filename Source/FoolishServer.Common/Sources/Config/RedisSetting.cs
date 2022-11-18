using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Config
{
    public class RedisSetting : IRedisSetting
    {
        public string Host => throw new NotImplementedException();

        public int Port => throw new NotImplementedException();

        public string Password => throw new NotImplementedException();

        public int DbIndex => throw new NotImplementedException();
    }
}
