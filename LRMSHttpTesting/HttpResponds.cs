using LRM;
using System;
using System.Collections.Specialized;
using System.Web.Script.Serialization;

namespace LRMSHttpTesting
{
    public class HttpResponds
    {
        //private variables
        JavaScriptSerializer JSC = new JavaScriptSerializer();

        public HttpResponds()
        {
        }

        public virtual string Get(string Path, NameValueCollection QueryString)
        {
            return "";
        }

        public virtual string Post(string Path, NameValueCollection QueryString, string Context)
        {
            TryNewStateContext InputData = null;
            try
            {
                InputData = JSC.Deserialize<TryNewStateContext>(Context);
            }
            catch
            {
            }
            if (InputData != null)
            {
                if (InputData.CureentResource != null)
                {
                    string FunctionName = GetSubPath(Path);
                    switch (FunctionName)
                    {
                        case "MyFunction1":
                            return MyFunction1(InputData);
                        default:
                            return "";
                    }
                }
            }
            return "";
        }

        public virtual string Put(string Path, NameValueCollection QueryString, string Context)
        {
            return "";
        }

        public virtual string Delete(string Path, NameValueCollection QueryString)
        {
            return "";
        }

        private string GetSubPath(string Path)
        {
            if (Path.Length > 0 && Path[0] == '/')
                Path = Path.Substring(1);
            if (Path.Length > 1 && Path[Path.Length - 1] == '/')
                Path = Path.Substring(0, Path.Length - 1);
            return Path;
        }

        //*******************************************************************************************************
        private string MyFunction1(TryNewStateContext InputData)
        {
            try
            {
                var x1 = InputData.DependResources[0];
                var x2 = InputData.DependResources[1];

                return
                    (Convert.ToInt32(x1.States[x1.States.Count-1].Data)
                    + Convert.ToInt32(x2.States[x2.States.Count-1].Data)).ToString();
            }
            catch
            {
                return "";
            }
        }
    }

    public class TryNewStateContext
    {
        public Resource[] DependResources = new Resource[0];
        public Resource CureentResource = null;
    }
}
