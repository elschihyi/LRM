using System.Collections.Generic;

namespace LRM
{
    public delegate void SignalGlobal(string ResoruceKey);

    public class ResourceManager
	{
        public SignalGlobal InformGlobal;

        /**********************************************************
		 * private Class Variable
		 * ********************************************************/
        private Dictionary<string,Resource> Resources ;

		/**********************************************************
		 * Constructors
		 * ********************************************************/
		public ResourceManager()
		{
			Resources = new Dictionary<string,Resource>();
		}

        /**********************************************************
		 * Public Methods
		 * ********************************************************/
        public string [] GetAllResourceKeys()
        {
            string[] AllResourceKeys= new string[Resources.Count];
            Resources.Keys.CopyTo(AllResourceKeys,0);
            return AllResourceKeys;
        }


        public Respond Post(string[] DependResourceKeys, TryNewState MyTryNewState, string Data ,string DefaulKey= "")
        {
            //Check MyTryNewState and Data
            if (MyTryNewState == null && string.IsNullOrEmpty(Data))
                return Respond(false, "TryNewState and Data can not both be null or empty", null);

            if (DependResourceKeys == null)
                DependResourceKeys = new string[0];

            //Check DependResourceKeys
            foreach (string DependResourceKey in DependResourceKeys)
            {
                Resource TheResource = null;
                Resources.TryGetValue(DependResourceKey, out TheResource);
                if (TheResource == null)
                    return Respond(false, "Resource Key(" + DependResourceKey + ") not found", null);
            }

            //create new resource
            Resource NewResource = new Resource(this, MyTryNewState);
            NewResource.DependResourceKeys = DependResourceKeys;
            if (!string.IsNullOrEmpty(DefaulKey))
                NewResource.Key = DefaulKey;
            NewResource.Respond(Data);

            //Add to Resources
            if (NewResource.States.Count > 0)
            {
                Resources.Add(NewResource.Key, NewResource);
                return Respond(true, "", NewResource);
            }else
            {
                return Respond(false, "No initial New State",null);
            }
        }

        public Respond Post(Resource Dup_Resource)
        {
            if (Dup_Resource == null)
            {
                return Respond(false, "Dup_resource can not be null", null);
            }
            Resource TheResource = null;
            Resources.TryGetValue(Dup_Resource.Key, out TheResource);
            if (TheResource != null)
            {
                return Respond(false, "Dup_Resource already exist", null);
            }
            if (Dup_Resource.States == null || Dup_Resource.States.Count == 0)
            {
                return Respond(false, "Dup_resource state number can not be 0", null);
            }
            //create duplicate resource
            Resource NewResource = new Resource(this,null);
            NewResource.Key = Dup_Resource.Key;
            NewResource.DependResourceKeys = new string[0];
            foreach(State aState in Dup_Resource.States)
            {
                State NewState = new State();
                NewState.Id = aState.Id;
                NewState.TimeStamp = aState.TimeStamp;
                NewState.Data = aState.Data;
                NewResource.States.Add(NewState);
            }

            //Add to Resources
            Resources.Add(NewResource.Key, NewResource);

            return Respond(true, "", NewResource);
        }

		public Respond Get(string ResourceKey,string RequestorKey)
		{
			Resource TheResource =null;
			Resources.TryGetValue(ResourceKey, out TheResource);
            if (TheResource != null)
            {
                TheResource.Request(RequestorKey);
                return Respond(true,"",TheResource);
            }else
            {
                return Respond(false,"Resource Key does not exist",null);
            }
		}

        public Respond Put(string ResourceKey, string Data="")
        {
            Resource TheResource = null;
            Resources.TryGetValue(ResourceKey, out TheResource);
            if (TheResource != null)
            {
                TheResource.Respond(Data);
                return Respond(true, "", null);
            }
            else
            {
                return Respond(false, "Resource Key does not exist", null);
            }
        }

        public Respond Put(Resource Dul_Resource)
        {
            if(Dul_Resource == null)
                return Respond(false, "Dul_Resource can not be null", null);

            Resource TheResource = null;
            Resources.TryGetValue(Dul_Resource.Key, out TheResource);
            if (TheResource != null)
            {
                TheResource.Respond(Dul_Resource);
                return Respond(true, "", null);
            }
            else
            {
                return Respond(false, "Resource does not exist", null);
            }
        }

        public Respond Delete(string ResourceKey)
        {
            Resource TheResource = null;
            Resources.TryGetValue(ResourceKey, out TheResource);
            if (TheResource != null)
            {
                if (TheResource.DependedResourceKeys.Count == 0)
                {
                    Resources.Remove(ResourceKey);
                    foreach(var Kee in TheResource.DependResourceKeys)
                    {
                        Resource DependResource = null;
                        Resources.TryGetValue(Kee, out DependResource);
                        if (DependResource != null)
                        {
                            if (DependResource.DependedResourceKeys.Contains(ResourceKey))
                                DependResource.DependedResourceKeys.Remove(ResourceKey);
                        }
                    }
                    return Respond(true, "", null);
                }else
                {
                    return Respond(false, "Still have depended Resources.", null);
                }
            }
            else
            {
                return Respond(false, "Resource does not exist.", null);
            }
        }
        

        /**********************************************************
		 * Private Methods
		 * ********************************************************/
        private Respond Respond(bool Success, string ErrMsg, Resource NewResource)
		{
            Respond NewRespond = new Respond();
			NewRespond.Success = Success;
			NewRespond.ErrMsg = ErrMsg;
			NewRespond.Resource = NewResource;
			return NewRespond;
		}
	}

	public class Respond
	{
		public bool Success = false;
		public string ErrMsg = "";
		public Resource Resource = null;
	}
}