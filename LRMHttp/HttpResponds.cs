using LRM;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Script.Serialization;

namespace LRMHttp
{
    public class HttpResponds
    {
        //private variables
        ResourceManager RM;
        JavaScriptSerializer JSC = new JavaScriptSerializer();
        Dictionary<string, string> HttpTryNewStateInfo;
        string GRM_URL = "";

        public HttpResponds()
        {
            //Set up Resource Manager
            this.RM = new ResourceManager();
            this.RM.InformGlobal = InformGlobal;
            this.HttpTryNewStateInfo = new Dictionary<string, string>();
        }

        public virtual string Get(string Path, NameValueCollection QueryString)
        {
            string Key = GetKey(Path);
            //Get all Resource keys
            if (string.Compare(Key.ToLower(), "all") == 0)
            {
                return JSC.Serialize(RM.GetAllResourceKeys());
            }
            else
            {
                return JSC.Serialize(RM.Get(Key, QueryString["RequestorKey"]));
            }
        }

        public virtual string Post(string Path, NameValueCollection QueryString, string Context)
        {
            //set GRM_URL
            if (!string.IsNullOrEmpty(QueryString["SetGRM_URL"]))
            {
                GRM_URL = QueryString["SetGRM_URL"];
                return JSC.Serialize(Respond(true, "", null));
            }

            string Key = GetKey(Path);
            Resource Dup_Resource=null;
            PostInput NewPostInput=null;
            try
            {
                Dup_Resource = JSC.Deserialize<Resource>(Context);
            }
            catch
            {
                Dup_Resource = null;
            }
            if (Dup_Resource.States.Count < 1)
                Dup_Resource = null;
            if (Dup_Resource == null)
            {
                try
                {
                    NewPostInput = JSC.Deserialize<PostInput>(Context);
                }
                catch
                {
                    return JSC.Serialize(Respond(false, "Context format not reconize", null));
                }
            }
            if (Dup_Resource != null)
            {
                return JSC.Serialize(RM.Post(Dup_Resource));
            } else if (NewPostInput != null
                && ((!string.IsNullOrEmpty(NewPostInput.Data))
                    ||(!string.IsNullOrEmpty(NewPostInput.TryNewStateCallBackLink))))
            {
                string key = "";
                if (!string.IsNullOrEmpty(NewPostInput.TryNewStateCallBackLink))
                {
                    key = Guid.NewGuid().ToString();
                    HttpTryNewStateInfo.Add(key, NewPostInput.TryNewStateCallBackLink);
                }
                Respond res = RM.Post(NewPostInput.DependResourceLinks, HttpTryNewState, NewPostInput.Data, key);
                return JSC.Serialize(res);
            }else
            {
                return JSC.Serialize(Respond(false, "Context format not reconize", null));
            }
        }

        public virtual string Put(string Path, NameValueCollection QueryString, string Context)
        {
            string Key = GetKey(Path);
            Resource Update_Resource = null;
            if (!string.IsNullOrEmpty(Context)) {
                try
                {
                    Update_Resource = JSC.Deserialize<Resource>(Context);
                }
                catch
                {
                    return JSC.Serialize(RM.Put(Key, Context));
                }
                if (Update_Resource.States.Count < 1)
                    Update_Resource = null;
            }
            if (Update_Resource!=null)
                return JSC.Serialize(RM.Put(Update_Resource));
            else
                return JSC.Serialize(RM.Put(Key,Context));
        }

        public virtual string Delete(string Path, NameValueCollection QueryString)
        {
            string Key = GetKey(Path);
            return JSC.Serialize(RM.Delete(Key));
        }


        //TryNewState Fucntionpointer
        public string HttpTryNewState(Resource[] DependResources,Resource CureentResource)
        {
            try
            {
                string BaseURL = HttpTryNewStateInfo[CureentResource.Key];
                return HttpRequest.Request(BaseURL, "POST", JSC.Serialize(new TryNewStateContext() {
                DependResources=DependResources,CureentResource=CureentResource}
                ));
            } catch
            {
                return "";
            }
        }

        //Inform Global function
        public void InformGlobal(string ResoruceKey)
        {
            ///TODO: Signal Global about the update of Resource
        }
       

        //private method*****************************************************************************
        private string GetKey(string Path)
        {
            if (Path.Length>0&&Path[0] == '/')
                Path = Path.Substring(1);
            if (Path.Length > 1 && Path[Path.Length - 1] == '/')
                Path = Path.Substring(0, Path.Length - 1);
            return Path;
        }

        private Respond Respond(bool Success, string ErrMsg, Resource NewResource)
        {
            Respond NewRespond = new Respond();
            NewRespond.Success = Success;
            NewRespond.ErrMsg = ErrMsg;
            NewRespond.Resource = NewResource;
            return NewRespond;
        }
    }

    public class PostInput
    {
        public string[] DependResourceLinks = new string[0];
        public string TryNewStateCallBackLink = "";
        public string Data = "";
    }

    public class TryNewStateContext
    {
        public Resource[] DependResources = new Resource[0];
        public Resource CureentResource = null;
    }
}
