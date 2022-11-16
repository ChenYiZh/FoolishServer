using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Model
{
    [Serializable]
    public class Test : Entity
    {
        [EntityField]
        public long UserId { get; set; }
        [EntityField]
        public string UserName { get; set; }
        [EntityField]
        public string Password { get; set; }

        private string testStr;
        public string TestStr
        {
            get
            {
                lock (SyncRoot)
                {
                    return testStr;
                }
            }
            set
            {
                lock (SyncRoot)
                {
                    if (!object.Equals(testStr, value))
                    {
                        testStr = value;
                        NotifyPropertyModified("testStr");
                    }
                }
            }
        }
    }
}
