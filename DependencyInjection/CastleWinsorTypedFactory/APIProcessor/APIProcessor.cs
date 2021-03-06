﻿using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIProcessor
{
    public class APIProcessorClass : IProcessor
    {
        private IConfigurableObject _object;
        private IConfigurableObjectFactory _configurationObjectFactory; 

        public APIProcessorClass(IConfigurableObjectFactory factor)
        {
            _configurationObjectFactory = factor;
        }

        public void StartProcess(string configuration)
        {
            Console.WriteLine("[APIProcessor] Using factory to instantiate with the following config:" + configuration);

            _object = _configurationObjectFactory.Create(configuration);

            Console.WriteLine("[APIProcessor] Calling connect method of IConfigurableObject");

            _object.Connect();

            // object created by a factory must be released;
            _configurationObjectFactory.Release(_object);
        }
    }
}
