using Castle.MicroKernel.Registration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWGenericRepository
{
    class Program
    {
        static void Main(string[] args)
        {

            var container = new WindsorContainer();

            container.Register(Component.For(typeof(IGeneric1<>))
             .ImplementedBy(typeof(Concrete<>)));

            IGeneric1<int> generic = container.Resolve(typeof(IGeneric1<int>)) as IGeneric1<int>;


            Console.WriteLine(generic.GetInfo());
            Console.Read();

        }
    }
}
