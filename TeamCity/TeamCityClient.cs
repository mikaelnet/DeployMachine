using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace TeamCity
{
    public class TeamCityClient
    {
        public enum JobState
        {
            Undefined,
            Queued,
            Running,
            Finished,
            Failed
        }

        public enum JobResult
        {
            Undefined,
            Success,
            Failure
        }

        public Uri BaseUri { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        private readonly CredentialCache _credentialCache;

        public string JobId { get; set; }

        public int TaskId { get; set; }
        public JobState State { get; set; }
        public JobResult Result { get; set; }
        public int PercentageComplete { get; set; }
        public int ElapsedSeconds { get; set; }
        public int EstimatedTotalSeconds { get; set; }
        public string CurrentStageText { get; set; }

        public TeamCityClient(Uri baseUri, string username, string password)
        {
            BaseUri = baseUri;
            Username = username;
            Password = password;
            _credentialCache = new CredentialCache();
            _credentialCache.Add(baseUri, "Basic", new NetworkCredential(Username, Password));
            
        }

        private HttpWebRequest CreateRequest(string url, string method = null)
        {
            var uri = new Uri(BaseUri, url);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Credentials = _credentialCache;
            request.Method = method ?? "GET";
            request.Accept = "application/xml";

            return request;
        }

        private XmlDocument GetResponse(HttpWebRequest request)
        {
            var response = (HttpWebResponse)request.GetResponse();
            var responseStream = response.GetResponseStream();
            if (responseStream == null)
                return null;
            var xdoc = new XmlDocument();
            using (var reader = new StreamReader(responseStream))
            {
                xdoc.Load(reader);
            }

            //Console.WriteLine();
            //Console.WriteLine(xdoc.OuterXml);
            //Console.WriteLine();

            return xdoc;
        }

        protected XmlDocument GetData(string url)
        {
            var request = CreateRequest(url);
            return GetResponse(request);
        }

        protected XmlDocument PostData(string uri, string postData)
        {
            var request = CreateRequest(uri, "POST");
            request.ContentType = "application/xml";

            byte[] body = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = body.Length;
            using (var putStream = request.GetRequestStream())
            {
                putStream.Write(body, 0, body.Length);
            }
            return GetResponse(request);
        }


        private JobState ParseState(string state)
        {
            JobState jobstate;
            if (!Enum.TryParse(state, true, out jobstate))
                Console.WriteLine("Unable to parse '{0}'", state);
            return jobstate;
        }

        public int StartJob(string jobid)
        {
            var buildXml = string.Format("<build><buildType id=\"{0}\"/></build>", jobid);
            var xdoc = PostData("/app/rest/buildQueue", buildXml);
            if (xdoc == null || xdoc.DocumentElement == null)
                return 0;

            var buildElement = xdoc.DocumentElement;
            int taskId;
            int.TryParse(buildElement.GetAttribute("id"), out taskId);
            TaskId = taskId;
            State = ParseState(xdoc.DocumentElement.GetAttribute("state"));
            return TaskId;
        }

        public JobState PollStatus()
        {
            var xdoc = GetData("/app/rest/buildQueue/taskId:" + TaskId);

            var buildElement = xdoc.DocumentElement;
            State = ParseState(buildElement.GetAttribute("state"));
            var ri = buildElement.SelectSingleNode("running-info") as XmlElement;
            if (ri != null)
            {
                PercentageComplete = Convert.ToInt32(ri.GetAttribute("percentageComplete"));
                ElapsedSeconds = Convert.ToInt32(ri.GetAttribute("elapsedSeconds"));
                EstimatedTotalSeconds = Convert.ToInt32(ri.GetAttribute("estimatedTotalSeconds"));
                CurrentStageText = ri.GetAttribute("currentStageText");
            }
            if (!string.IsNullOrWhiteSpace(buildElement.GetAttribute("status")))
            {
                JobResult jobResult;
                if (Enum.TryParse(buildElement.GetAttribute("status"), true, out jobResult))
                    Result = jobResult;
                else
                    Console.WriteLine("Unable to parse '{0}'", buildElement.GetAttribute("status"));
            }
            return State;
        }
    }
}
