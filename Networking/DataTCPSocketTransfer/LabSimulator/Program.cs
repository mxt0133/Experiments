using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);           

                using(var sender = new LabDataSender())
                {
                    sender.Start();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception occured: " + ex.Message);
                Console.ReadLine();
            }
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }
    }
}
