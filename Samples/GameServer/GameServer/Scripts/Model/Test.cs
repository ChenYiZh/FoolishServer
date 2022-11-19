using FoolishServer.Collections;
using FoolishServer.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Model
{
    [Serializable]
    public class Test : MajorEntity
    {
        [EntityField(IsKey = true)]
        public long UserId { get; set; }
        [EntityField(IsKey = true)]
        public string UserName { get; set; }
        [EntityField]
        public string Password { get; set; }
        [EntityField]
        public EntityList<Test2> Tests { get; set; }

        private long testStr;
        public long TestStr
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
                        object temp = (object)testStr;
                        testStr = value;
                        NotifyPropertyModified("testStr", temp, (object)testStr);
                    }
                }
            }
        }
    }
}

