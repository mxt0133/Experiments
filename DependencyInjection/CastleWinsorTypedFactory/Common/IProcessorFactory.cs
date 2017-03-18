using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface IProcessorFactory
    {
        IProcessor Create(string jobType);
        void Release(IProcessor configurableObject);
    }
}
