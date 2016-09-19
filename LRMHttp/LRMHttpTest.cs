using LRM;
using System;
using System.Threading;
using System.Web.Script.Serialization;

namespace LRMHttp
{
    class LRMHttpTest
    {
        private static HttpServer LRMHttpServer;
        private static string IP = "localhost";
        private static string SERVERPATH = "Resources/";
        private static string PORT = "8080";
  
        private static JavaScriptSerializer JSC = new JavaScriptSerializer();

        static void Main(string[] args)
        {
            LRMHttpServer = new HttpServer(IP, SERVERPATH, PORT, new HttpResponds());
            LRMHttpServer.Run();

            string BaseURL = "http://" + IP + ":" + PORT + "/" + SERVERPATH;


            //Try POST Original Resources
            Resource ResourceAB = JSC.Deserialize<Respond> (
                HttpRequest.Request(BaseURL, "POST", JSC.Serialize(CreatePostInput(null, null, "2")))
                ).Resource;

            Resource ResourceBD = JSC.Deserialize<Respond>(
                HttpRequest.Request(BaseURL, "POST", JSC.Serialize(CreatePostInput(null, null, "1")))
                ).Resource;

            ////****************************************************
            //Console.WriteLine("AB: ");
            //PrintResource(ResourceAB);

            //Console.WriteLine("BD: ");
            //PrintResource(ResourceBD);
            ////****************************************************


            //Try GET Original Resources
            Resource TryGet1 = JSC.Deserialize<Respond>(
               HttpRequest.Request(BaseURL+ResourceAB.Key+ "?RequestorKey=", "GET","")
               ).Resource;

            ////****************************************************
            //Console.WriteLine("TryGet1");
            //PrintResource(TryGet1);
            ////****************************************************


            //Try to post a resource depand on AB and BD
            Resource ResourceAD = JSC.Deserialize<Respond>(
                HttpRequest.Request(BaseURL, "POST", JSC.Serialize(CreatePostInput(new string[] { ResourceAB.Key, ResourceBD.Key }, ":9999/servername/funcname", "")))
                ).Resource;

            Thread.Sleep(1000);
            //Try GET Original Resources
            Resource TryGetAB = JSC.Deserialize<Respond>(
               HttpRequest.Request(BaseURL + ResourceAB.Key + "?RequestorKey=", "GET", "")
               ).Resource;

            //Try GET Original Resources
            Resource TryGetBD = JSC.Deserialize<Respond>(
               HttpRequest.Request(BaseURL + ResourceBD.Key + "?RequestorKey=", "GET", "")
               ).Resource;

            ////****************************************************
            //Console.WriteLine("AB: ");
            //PrintResource(TryGetAB);

            //Console.WriteLine("BD: ");
            //PrintResource(TryGetBD);

            //Console.WriteLine("AD: ");
            //PrintResource(ResourceAD);
            ////****************************************************

            //Try to update Resource AB to 3
            HttpRequest.Request(BaseURL + ResourceAB.Key, "PUT", "3");
            Resource TryGetAB2 = JSC.Deserialize<Respond>(
               HttpRequest.Request(BaseURL + ResourceAB.Key + "?RequestorKey=", "GET", "")
               ).Resource;

            ////****************************************************
            //Console.WriteLine("AB: ");
            //PrintResource(TryGetAB2);
            ////****************************************************

            HttpRequest.Request(BaseURL + ResourceAB.Key,"PUT",JSC.Serialize(TryGetAB));
            Resource TryGetAB3 = JSC.Deserialize<Respond>(
               HttpRequest.Request(BaseURL + ResourceAB.Key + "?RequestorKey=", "GET", "")
               ).Resource;

            ////****************************************************
            Console.WriteLine("AB: ");
            PrintResource(TryGetAB3);
            ////****************************************************


            Console.WriteLine("Press Any Key to Stop Server");
            Console.ReadLine();
            LRMHttpServer.Stop();  
        }

        public static PostInput CreatePostInput(string[] DependResourceLinks, string TryNewStateCallBackLink, string Data)
        {
            PostInput PI = new PostInput();
            PI.DependResourceLinks = DependResourceLinks;
            PI.TryNewStateCallBackLink = TryNewStateCallBackLink;
            PI.Data = Data;
            return PI;
        }

        private static void PrintResource(Resource r)
        {
            Console.WriteLine(JSC.Serialize(r));
        }
    }
}
