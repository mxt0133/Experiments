using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWGenericRepository
{
    class Concrete<T> : IGeneric1<T>
    {
        public string GetInfo()
        {
            return typeof(T).FullName;
        }
    }
}
