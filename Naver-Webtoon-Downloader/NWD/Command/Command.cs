﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRforest.NWD.DataType;
using WRforest.NWD.Key;

namespace WRforest.NWD.Command
{
    abstract class Command
    {
        protected Config config;
        public Command(Config config)
        {
            this.config = config;
        }
        public abstract void Start(params string[] args);
    }
}
