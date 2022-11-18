using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Config
{
    public class DatabaseSetting : IDatabaseSetting
    {
        public string ConnectKey => throw new NotImplementedException();

        public string ProviderName => throw new NotImplementedException();

        public string ConnectionString => throw new NotImplementedException();
    }
}
