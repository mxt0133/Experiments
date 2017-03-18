using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connector
{
    public class ConnectorClass: IConfigurableObject
    {
        private string _configuration;

        public  ConnectorClass(string configuration)
        {
            // this value is injected to the constructure by Castle via the 
            // parameter passed into IConfigurableObjectFactory.Create()
            _configuration = configuration; 
        }

        public void Connect()
        {
            Console.WriteLine("[ConnectorClass] Connecting using configuration value: " + _configuration);
        }
    }
}
