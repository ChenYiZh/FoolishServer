﻿using FoolishServer.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Model
{
    public class Test2 : MinorEntity
    {
        [EntityField]
        public string TestId { get; set; }
    }
}
