using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace DeployMachine
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var deployManager = new DeployManager();
                Console.WriteLine("Initalizing");
                deployManager.Initialize();
                Console.WriteLine("Running");
                deployManager.Run();
                Console.WriteLine("Exiting");
                deployManager.Exit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
        }
    }
}
