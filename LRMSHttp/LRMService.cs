using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using LRMHttp;

namespace LRMSHttp
{
    public partial class LRMService : ServiceBase
    {
        private static HttpServer LRMHttpServer;
        private static string IP = "localhost";
        private static string SERVERPATH = "Resources/";
        private static string PORT = "8080";

        public LRMService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            LRMHttpServer = new HttpServer(IP, SERVERPATH, PORT, new HttpResponds());
            LRMHttpServer.Run();
        }

        protected override void OnStop()
        {
        }
    }
}
