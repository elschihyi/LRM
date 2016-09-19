using System;
using LRM;
using System.Web.Script.Serialization;

namespace LRMTest
{
    class LRMTest
    {
        static ResourceManager RM1 = new ResourceManager();
        static ResourceManager RM2 = new ResourceManager();
        static JavaScriptSerializer jcs = new JavaScriptSerializer();

        static void Main(string[] args)
        {
            //Try POST Original Resources
            Resource ResourceAB = RM1.Post(null, null, "1").Resource;
            Resource ResourceBA = RM1.Post(null, null, "1").Resource;
            Resource ResourceAC = RM1.Post(null, null, "2").Resource;
            Resource ResourceCA = RM1.Post(null, null, "2").Resource;
            Resource ResourceBC = RM1.Post(null, null, "2").Resource;
            Resource ResourceCB = RM1.Post(null, null, "2").Resource;
            Resource ResourceBD = RM1.Post(null, null, "1").Resource;
            Resource ResourceDB = RM1.Post(null, null, "1").Resource;
            Resource ResourceCD = RM1.Post(null, null, "2").Resource;
            Resource ResourceDC = RM1.Post(null, null, "2").Resource;

            ////****************************************************
            //Console.WriteLine("All RM1 Resource keys");
            //PrintAllResourceKey(RM1);
            ////****************************************************

            //Try to Post Duplicate Resources
            Resource DR_AB = RM2.Post(ResourceAB).Resource;
            Resource DR_BA = RM2.Post(ResourceBA).Resource;
            Resource DR_AC = RM2.Post(ResourceAC).Resource;
            Resource DR_CA = RM2.Post(ResourceCA).Resource;
            Resource DR_BC = RM2.Post(ResourceBC).Resource;
            Resource DR_CB = RM2.Post(ResourceCB).Resource;
            Resource DR_BD = RM2.Post(ResourceBD).Resource;
            Resource DR_DB = RM2.Post(ResourceDB).Resource;
            Resource DR_CD = RM2.Post(ResourceCD).Resource;
            Resource DR_DC = RM2.Post(ResourceDC).Resource;

            ////****************************************************
            //Console.WriteLine("All RM2 Resource keys");
            //PrintAllResourceKey(RM2);

            //Console.WriteLine("ResourceAB");
            //PrintResource(ResourceAB);

            //Console.WriteLine("ResourceDR_AB");
            //PrintResource(DR_AB);
            ////****************************************************

            //Try GET Original Resources
            Resource TryGet1 = RM1.Get(ResourceAB.Key, "").Resource;

            ////****************************************************
            //Console.WriteLine("TryGet1");
            //PrintResource(TryGet1);
            ////****************************************************


            //Try Get Duplicate Resources
            Resource TryGet2 = RM2.Get(DR_AB.Key, "").Resource;

            ////****************************************************
            //Console.WriteLine("TryGet2");
            //PrintResource(TryGet2);
            ////****************************************************


            //Try to post a resource depand on AB and BD
            Resource ResourceAD =RM1.Post(new string[] { ResourceAB.Key, ResourceBD.Key }, TryNewPath, "").Resource;

            ////****************************************************
            //Console.WriteLine("AB: ");
            //PrintResource(ResourceAB);

            //Console.WriteLine("BD: ");
            //PrintResource(ResourceBD);

            //Console.WriteLine("AD: ");
            //PrintResource(ResourceAD);
            ////****************************************************

            //Try to update Resource AB to 3
            RM1.Put(ResourceAB.Key, "3");

            ////****************************************************
            //Console.WriteLine("AB: ");
            //PrintResource(ResourceAB);

            //Console.WriteLine("BD: ");
            //PrintResource(ResourceBD);


            //Console.WriteLine("AD: ");
            //PrintResource(ResourceAD);
            ////****************************************************

            //Try to post a resource depand on DR_AB and DR_BD
            Resource DR_AD = RM2.Post(new string[] { DR_AB.Key, DR_BD.Key }, TryNewPath, "").Resource;

            ////****************************************************
            //Console.WriteLine("DR_AB: ");
            //PrintResource(DR_AB);

            //Console.WriteLine("DR_BD: ");
            //PrintResource(DR_BD);

            //Console.WriteLine("DR_AD: ");
            //PrintResource(DR_AD);
            ////****************************************************

            //Try to update Resource DR_AB by using ResoruceAB
            RM2.Put(ResourceAB);

            ////****************************************************
            Console.WriteLine("DR_AB: ");
            PrintResource(DR_AB);

            //Console.WriteLine("DR_BD: ");
            //PrintResource(DR_BD);

            Console.WriteLine("DR_AD: ");
            PrintResource(DR_AD);
            ////****************************************************


            Console.WriteLine("Press any key to exist");
            Console.ReadKey();
        }

        public static string TryNewPath(Resource[] DependResources, Resource CureentResource)
        {
            return (Convert.ToInt32(DependResources[0].States[DependResources[0].States.Count-1].Data) +
                Convert.ToInt32(DependResources[1].States[DependResources[1].States.Count - 1].Data)).ToString();
        }


        private static void PrintResource(Resource r)
        {
            Console.WriteLine(jcs.Serialize(r));
        } 

        private static void PrintAllResourceKey(ResourceManager RM)
        {
            string[] Allkeys = RM.GetAllResourceKeys();
            foreach(string Keys in Allkeys)
            {
                Console.WriteLine(Keys);
            }
        }

    }
}
