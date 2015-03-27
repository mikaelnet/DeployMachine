using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TeamCity.Tests
{
    [TestFixture]
    public class TeamCityClientTests
    {
        private TeamCityClient GetClient()
        {
            return new TeamCityClient(new Uri(ConfigurationManager.AppSettings["BaseUrl"]),
                ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"]);
        }

        public string JobId
        {
            get
            {
                return ConfigurationManager.AppSettings["JobId"];
            }
        }

        [Test]
        public void ConfigurationTest()
        {
            var baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
            Assert.IsFalse(string.IsNullOrWhiteSpace(baseUrl), "Please provide a config file");
            Assert.IsFalse(string.Equals(baseUrl, "https://your.teamcity.server"), "Please create a user.config and specify a BaseUrl in a user.config file");
            var uri = new Uri(baseUrl);
            Assert.IsTrue(uri.IsWellFormedOriginalString());
            Assert.IsTrue(uri.IsAbsoluteUri);

            var username = ConfigurationManager.AppSettings["Username"];
            Assert.IsFalse(string.IsNullOrWhiteSpace(username));
            Assert.IsFalse(string.Equals(username, "username"));

            var password = ConfigurationManager.AppSettings["Password"];
            Assert.IsFalse(string.IsNullOrWhiteSpace(password));
            Assert.IsFalse(string.Equals(password, "password"));

            var jobid = JobId;
            Assert.IsFalse(string.IsNullOrWhiteSpace(jobid));
            Assert.IsFalse(string.Equals(jobid, "jobid"));
        }

        [Test]
        public void StartJobTest()
        {
            var client = GetClient();
            var taskId = client.StartJob(JobId);
            Console.WriteLine("Started job id {0} as task {1}. Status {2}", JobId, taskId, client.State);
            Assert.IsTrue(taskId > 0);

            do {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                client.PollStatus();
                Console.WriteLine("{0}, {1} of {2}, {3}%", client.State, client.ElapsedSeconds,
                    client.EstimatedTotalSeconds, client.PercentageComplete);
            } while (client.State == TeamCityClient.JobState.Queued || client.State == TeamCityClient.JobState.Running);

            Console.WriteLine("Result: {0}", client.Result);
        }
    }
}
