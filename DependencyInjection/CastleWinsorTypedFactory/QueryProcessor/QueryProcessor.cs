using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryProcessor
{
    public class QueryProcessorClass : IProcessor
    {
        private IConfigurableObject _object;
        private IConfigurableObjectFactory _configurationObjectFactory;

        public QueryProcessorClass(IConfigurableObjectFactory factory)
        {
            _configurationObjectFactory = factory;
        }

        public void StartProcess(string configuration)
        {
            Console.WriteLine("[QueryProcessor] Using factory to instantiate with the following config: " + configuration);

            _object = _configurationObjectFactory.Create(configuration);

            Console.WriteLine("[QueryProcessor] Calling connect method of IConfigurableObject");

            _object.Connect();

            // object created by a factory must be released;
            _configurationObjectFactory.Release(_object);
        }
    }
}
