using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace DeployMachine
{
    public class TeamCityClient
    {
        public string Hostname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string TaskId { get; set; }
        public string State { get; set; }

        public void StartJob(string jobid)
        {
            var url = string.Format("https://{0}/app/rest/buildQueue");
            var buildXml = string.Format("<build><buildType id=\"{0}\"/></build>", jobid);

            var request = (HttpWebRequest) WebRequest.Create(url);
            var credentialsCache = new CredentialCache();
            credentialsCache.Add(new Uri(url), "Basic", new NetworkCredential(Username, Password));
            request.Credentials = credentialsCache;
            request.Method = "POST";
            request.ContentType = "application/xml";
            request.Accept = "application/xml";

            byte[] body = Encoding.UTF8.GetBytes(buildXml);
            request.ContentLength = body.Length;
            using (var putStream = request.GetRequestStream())
            {
                putStream.Write(body, 0, body.Length);
            }
            var response = (HttpWebResponse) request.GetResponse();
            var xdoc = new XmlDocument();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                xdoc.Load(reader);
            }

            TaskId = xdoc.DocumentElement.GetAttribute("taskId");
            State = xdoc.DocumentElement.GetAttribute("state");
        }

        public void PollStatus()
        {
            
        }
    }
}