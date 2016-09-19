using LRM;
using System;
using System.Threading;
using System.Web.Script.Serialization;

namespace LRMSHttpTesting
{
    class LRMSHttpTest
    {
        //Local Host for call back function
        static HttpServer LocalHttpServer;
        private static string LocalIP = "localhost";
        private static string LocalSERVERPATH = "TestServer/";
        private static string LocalPORT = "8083";

        //LRMSHttp Info
        //static HttpServer LRMHttpServer;
        private static string IP = "localhost";
        private static string SERVERPATH = "Resources/";
        private static string PORT = "8080";

        //Tools
        private static JavaScriptSerializer JSC = new JavaScriptSerializer();

        static void Main(string[] args)
        {
            /***********************Create LRM Server******************/
            //LRMHttpServer = new HttpServer(IP, SERVERPATH, PORT, new RMSHttpResponds());
            //LRMHttpServer.Run();
            /***********************************************************/


            /***Create a Local server**********************************/
            LocalHttpServer = new HttpServer(LocalIP, LocalSERVERPATH, LocalPORT, new HttpResponds());
            LocalHttpServer.Run();
            /***********************************************************/

            

            string BaseURL = "http://" + IP + ":" + PORT + "/" + SERVERPATH;
            string LocalbaseUrl= "http://" + LocalIP + ":" + LocalPORT + "/" + LocalSERVERPATH;

            //*************************************************************************************************
            //0.Try to get all resources
            string[] AllResources = JSC.Deserialize<string[]>(
               HttpRequest.Request(BaseURL + "/all", "GET", JSC.Serialize(CreatePostInput(null, null, "1")))
               );

            //Console.WriteLine("Print All Resoruces");
            foreach(var x in AllResources)
            {
                //Console.WriteLine(x);
            }


            //*************************************************************************************************
            //0. Delete All Resoruces(Cleaning)
            foreach(var key in AllResources)
            {
                var x=HttpRequest.Request(BaseURL + "/" + key, "DELETE", "");
            }

            AllResources = JSC.Deserialize<string[]>(
               HttpRequest.Request(BaseURL + "/all", "GET", JSC.Serialize(CreatePostInput(null, null, "1")))
               );

            //Console.WriteLine("Print All Resoruces after delete all");
            foreach(var x in AllResources)
            {
                //Console.WriteLine(x);
            }


            //*************************************************************************************************
            //1. Try POST Original Resources
            Resource ResourceAB = JSC.Deserialize<Respond>(
                HttpRequest.Request(BaseURL, "POST", JSC.Serialize(CreatePostInput(null, null, "2")))
                ).Resource;

            Resource ResourceBD = JSC.Deserialize<Respond>(
                HttpRequest.Request(BaseURL, "POST", JSC.Serialize(CreatePostInput(null, null, "1")))
                ).Resource;

            //Console.WriteLine("AB: ");
            //PrintResource(ResourceAB);
            //Console.WriteLine("BD: ");
            //PrintResource(ResourceBD);

            
            

            //*************************************************************************************************
            //2. Try GET a resoruce
            Resource TryGet = JSC.Deserialize<Respond>(
                HttpRequest.Request(BaseURL +"/" +ResourceAB.Key + "?RequestorKey=", "GET", "")
                ).Resource;

            //Console.WriteLine("TryGet");
            //PrintResource(TryGet);
        
            //*************************************************************************************************
            //3. Try to post a resource depand on AB and BD (with call back function MyFunction1)
            Resource ResourceAD = JSC.Deserialize<Respond>(
                HttpRequest.Request(BaseURL, "POST", 
                JSC.Serialize(CreatePostInput(new string[] { ResourceAB.Key, ResourceBD.Key}, LocalbaseUrl+"/MyFunction1", "")))
                ).Resource;

            //Console.WriteLine("AD: ");
            //PrintResource(ResourceAD);

            TryGet = JSC.Deserialize<Respond>(
                HttpRequest.Request(BaseURL + "/" + ResourceAB.Key + "?RequestorKey=", "GET", "")
                ).Resource;

            //Console.WriteLine("AB: ");
            //PrintResource(TryGet);

            TryGet = JSC.Deserialize<Respond>(
                HttpRequest.Request(BaseURL + "/" + ResourceBD.Key + "?RequestorKey=", "GET", "")
                ).Resource;

            //Console.WriteLine("BD: ");
            //PrintResource(TryGet);


            //*************************************************************************************************
            //4. Try to update a resource AB (Resource AD should update automatically)
            Respond res=JSC.Deserialize<Respond>(HttpRequest.Request(BaseURL + ResourceAB.Key, "PUT", "3"));

            TryGet = JSC.Deserialize<Respond>(
               HttpRequest.Request(BaseURL + "/" + ResourceAB.Key + "?RequestorKey=", "GET", "")
               ).Resource;

            //Console.WriteLine("AB: ");
            //PrintResource(TryGet);

            TryGet = JSC.Deserialize<Respond>(
               HttpRequest.Request(BaseURL + "/" + ResourceAD.Key + "?RequestorKey=", "GET", "")
               ).Resource;

            //Console.WriteLine("AD: ");
            //PrintResource(TryGet);


            //**************************************************************************************
            //5. Try to Post with a Resoruce
            Resource ResourceAC = new Resource();
            ResourceAC.DependedResourceKeys.Add("sadasdasdasdqwddadadasdas");
            ResourceAC.States.Add(new State() { Data = "22" });

            Resource NewResource = JSC.Deserialize<Respond>(
                HttpRequest.Request(BaseURL, "POST", JSC.Serialize(ResourceAC))
                ).Resource;

            TryGet = JSC.Deserialize<Respond>(
               HttpRequest.Request(BaseURL + "/" + NewResource.Key + "?RequestorKey=", "GET", "")
               ).Resource;

            //Console.WriteLine("AC: ");
            //PrintResource(ResourceAC);

            //Console.WriteLine("NewResource: ");
            //PrintResource(NewResource);

            //Console.WriteLine("TryGetNewResoruce: ");
            //PrintResource(TryGet);

            //**************************************************************************************
            //6. Try to update with Resoruce
            ResourceAC.States.Add(new State() { Data = "23" });

            Resource UpdateResourceAC = JSC.Deserialize<Respond>(
                HttpRequest.Request(BaseURL, "PUT", JSC.Serialize(ResourceAC))
                ).Resource;

            TryGet = JSC.Deserialize<Respond>(
               HttpRequest.Request(BaseURL + "/" + NewResource.Key + "?RequestorKey=", "GET", "")
               ).Resource;

            //Console.WriteLine("AC: ");
            //PrintResource(ResourceAC);

            //Console.WriteLine("NewResource: ");
            //PrintResource(NewResource);

            //Console.WriteLine("TryGetNewResoruce: ");
            //PrintResource(TryGet);


            Console.WriteLine("Press Any Key to Stop Server");
            Console.ReadLine();
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

    public class PostInput
    {
        public string[] DependResourceLinks = new string[0];
        public string TryNewStateCallBackLink = "";
        public string Data = "";
    }
}
