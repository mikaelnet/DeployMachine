using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace DeployMachine
{
    public class Config
    {
        public string TeamCityHostname
        {
            get { return _teamCityHostname; }
            set { _teamCityHostname = value.TrimEnd('/'); }
        }

        public string TeamCityUsername { get; set; }
        public string TeamCityPassword { get; set; }

        private static readonly object _lock = new object();
        private static Config _config = null;
        private string _teamCityHostname;

        public static Config Current
        {
            get
            {
                lock (_lock)
                {
                    if (_config == null)
                    {
                        _config = new Config();
                        _config.LoadSettings();
                    }
                    return _config;
                }
            }
        }

        private Config()
        {
        }

        private void LoadSettings()
        {
            TeamCityHostname = ConfigurationManager.AppSettings["TeamCity.Hostname"];
            TeamCityUsername = ConfigurationManager.AppSettings["TeamCity.Username"];
            TeamCityPassword = ConfigurationManager.AppSettings["TeamCity.Password"];
        }
    }

    public class TeamCityClient
    {
        public string TaskId { get; set; }
        public string State { get; set; }
        private readonly CredentialCache _credentialCache;

        public TeamCityClient()
        {
            _credentialCache = new CredentialCache();
            _credentialCache.Add(new Uri(Config.Current.TeamCityHostname), "Basic", new NetworkCredential(Config.Current.TeamCityUsername, Config.Current.TeamCityPassword));
            
        }

        private HttpWebRequest CreateRequest(string url, string method = null)
        {
            var uri = new Uri(string.Format("{0}{1}", Config.Current.TeamCityHostname, url));
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


        public void StartJob(string jobid)
        {
            var buildXml = string.Format("<build><buildType id=\"{0}\"/></build>", jobid);
            var xdoc = PostData("/app/rest/buildQueue", buildXml);
            if (xdoc == null || xdoc.DocumentElement == null)
                return;

            TaskId = xdoc.DocumentElement.GetAttribute("taskId");
            State = xdoc.DocumentElement.GetAttribute("state");
        }

        public void PollStatus()
        {
            var xdoc = GetData("/app/rest/buildQueue/taskId:" + TaskId);
            var state = xdoc.DocumentElement.GetAttribute("state");
            var ri = (XmlElement)xdoc.DocumentElement.SelectSingleNode("runningInfo");
            var percentageComplete = ri.GetAttribute("percentageComplete");
            var elapsedSeconds = ri.GetAttribute("elapsedSeconds");
            var estimatedTotalSeconds = ri.GetAttribute("estimatedTotalSeconds");
            var currentStageText = ri.GetAttribute("currentStageText");
        }
    }
}

