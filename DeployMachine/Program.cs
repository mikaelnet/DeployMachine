using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace DeployMachine
{
    class Options
    {
        [Option('h', "hostname", HelpText = "Hostname for TeamCity")]
        public string TeamCityHost { get; set; }

        [Option('u', "username", HelpText = "Username for TeamCity authentication")]
        public string Username { get; set; }

        [Option('p', "password", HelpText = "Password for TeamCity authentication")]
        public string Password { get; set; }

        [Option(HelpText = "Job ID to start")]
        public string JobIdentity { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return "error message";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Load config
            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                var helpText = HelpText.AutoBuild(options);
                Console.WriteLine(helpText);
                Environment.Exit(1);
                return;
            }

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
