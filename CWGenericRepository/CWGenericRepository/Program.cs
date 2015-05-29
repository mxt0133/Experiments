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

            // register open generic
            container.Register(Component.For(typeof(IGeneric1<>))
             .ImplementedBy(typeof(Concrete<>)));

            // type we want to use in our open generic
            var dynamicType = Type.GetType("System.Int32");
            
            // get the type of our generic
            var generic = typeof(IGeneric1<>);

            // make the generic type with our dynamic type
            var constructed = generic.MakeGenericType(dynamicType);                       

            // resolve our open generic type 
            IGeneric1<int> genericImpl = container.Resolve(constructed) as IGeneric1<int>;

            Console.WriteLine(genericImpl.GetInfo());
            Console.Read();

        }
    }
}
