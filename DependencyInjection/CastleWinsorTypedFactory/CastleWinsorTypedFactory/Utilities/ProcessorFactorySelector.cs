using Castle.Facilities.TypedFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CastleWinsorTypedFactory.Utilities
{
    public class ProcessorFactorySelector : DefaultTypedFactoryComponentSelector
    {
        // This selector is necessary to allow Windsor to use the 
        // Name value to determine which class to use when resolving 
        // the required IProcessor object
        
        protected override string GetComponentName(MethodInfo method, object[] arguments)
        {
            return (string)arguments[0];
        }
    }
}
