using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace LRMSHttpTesting
{
    public class HttpServer
    {
        //Private Http Server variables
        private string IP = "";
        private string SERVERPATH = "";
        private string PORT = "";

        private readonly HttpListener _listener = new HttpListener();
        private HttpResponds MyHttpResponds;

        public HttpServer(string IP, string SERVERPATH, string PORT, HttpResponds MyHttpResponds)
        {
            //Set up veriables and start server
            this.IP = IP;
            this.SERVERPATH = SERVERPATH;
            this.PORT = PORT;
            //this.MyHttpResponds = new HttpResponds();
            this.MyHttpResponds = MyHttpResponds;
            //this.MyHttpResponds.ServerBaseUrl = GetServerBaseUrl();

            if (!HttpListener.IsSupported)
                throw new NotSupportedException(
                    "HttpListener not Supported.");

            _listener.Prefixes.Add(GetServerBaseUrl());
            _listener.Start();
        }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                string rstr = Response(ctx.Request);
                                byte[] buf = Encoding.UTF8.GetBytes(rstr);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch { } // suppress any exceptions
                            finally
                            {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch { } // suppress any exceptions
            });
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }

        //private methods
        private string Response(HttpListenerRequest Request)
        {
            string SubPath = GetSubPath(Request.RawUrl);
            string BodyData = GetBodyData(Request);

            switch (Request.HttpMethod)
            {
                case "GET":
                    return MyHttpResponds.Get(SubPath, Request.QueryString);
                case "PUT":
                    return MyHttpResponds.Put(SubPath, Request.QueryString, BodyData);
                case "POST":
                    return MyHttpResponds.Post(SubPath, Request.QueryString, BodyData);
                case "DELETE":
                    return MyHttpResponds.Delete(SubPath, Request.QueryString);
                default:
                    return MyHttpResponds.Get(SubPath, Request.QueryString);
            }
        }

        //private classes
        private string GetSubPath(string RawUrl)
        {
            int index1 = RawUrl.IndexOf('?');
            if (index1 > -1)
            {
                return RawUrl.Substring(SERVERPATH.Length + 1, index1 - SERVERPATH.Length - 1);
            }
            else
            {
                return RawUrl.Substring(SERVERPATH.Length + 1);
            }
        }

        private string GetServerBaseUrl()
        {
            return string.Format("http://" + IP + ":{0}/{1}", PORT, SERVERPATH);
        }
            
        //get request post data
        public static string GetBodyData(HttpListenerRequest Request)
        {
            if (!Request.HasEntityBody)
                return "";

            using (Stream BodyData = Request.InputStream)
            {
                using (StreamReader reader = new StreamReader(BodyData))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
