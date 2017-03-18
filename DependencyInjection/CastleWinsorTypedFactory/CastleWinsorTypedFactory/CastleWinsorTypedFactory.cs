using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastleWinsorTypedFactory
{
    public class CastleWinsorTypedFactoryObject
    {
        private IProcessor _processor;
        private IProcessorFactory _processorFactory;

        public CastleWinsorTypedFactoryObject(IProcessorFactory factory)
        {
            _processorFactory = factory;
        }

        public void Start(string jobType, string runTimeConfiguration)
        {
            Console.WriteLine("[CastleWinsorTypedFactoryObject] Resolving processor based on jobType: " + jobType);
            
            _processor = _processorFactory.Create(jobType);

            _processor.StartProcess(runTimeConfiguration);
            
            // object created by a factory must be released;
            _processorFactory.Release(_processor);
        }
    }
}
