using CastleWinsorTypedFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = CastleWinsorTypedFactory.Utilities.Injector.Instance;

            var program = new Program();

            var jobType = program.GetJobType();
            var jobConfiguration = program.GetConfiguration();

            var typeFactory = container.Resolve<CastleWinsorTypedFactoryObject>();

            typeFactory.Start(jobType, jobConfiguration);

            Console.WriteLine("Press any key to exti.");
            Console.ReadKey(true);

            container.Release(typeFactory);
        }

        public string GetJobType()
        {
            bool invalid = true;
            var jobType = string.Empty;

            while (invalid)
            {
                Console.Clear();
                Console.Write("Please enter a job type [Query|API]:  ");
                jobType = Console.ReadLine();

                if (jobType.ToLower() == "Query".ToLower() || jobType.ToLower() == "API".ToLower())
                    invalid = false;
            }

            return jobType;
        }

        private string GetConfiguration()
        {
            Console.Write("Please enter job configuration:  ");
            return Console.ReadLine();
        }
    }
}
