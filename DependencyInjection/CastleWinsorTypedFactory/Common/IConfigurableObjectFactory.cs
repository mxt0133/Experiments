using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface IConfigurableObjectFactory
    {
        // This factory is meant to pass in run-time parameters to the object
        // that Castle will resolve, it is important that the parameter names in 
        // the create method matches the type's constructor parameter names
        IConfigurableObject Create(string configuration);

        void Release(IConfigurableObject configurableObject);
    }
}
