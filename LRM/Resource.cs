using System;
using System.Collections.Generic;

namespace LRM
{
	public delegate string TryNewState(Resource[] DependResources,Resource CureentResource);

	public class Resource
	{
		/**********************************************************
		 * Public Class Variables
		 * ********************************************************/
		public string Key;
		public string [] DependResourceKeys;
		public List<string> DependedResourceKeys;
		public List<State> States;

		/**********************************************************
		 * private Class Variable
		 * ********************************************************/
		private ResourceManager RM;
		private TryNewState MyTryNewState;

		/**********************************************************
		 * Constructors
		 * ********************************************************/
		public Resource()
		{
			Key = Guid.NewGuid().ToString();
			DependResourceKeys = new string[0];
			DependedResourceKeys = new List<string>();
			States = new List<State>();
		}

		public Resource(ResourceManager RM, TryNewState MyTryNewState): this()
		{
			this.RM = RM;
			this.MyTryNewState = MyTryNewState;
		}


		/**********************************************************
		 * Public Methods
		 * ********************************************************/
		public void Request(string RequestorKey)
		{
			//Add the RequestorKey into DependedResourceKeys if it is
			//not empty and is not in  DependedResourceKeys
			if (!string.IsNullOrEmpty(RequestorKey))
			{
				bool RequestorExist = false;
				foreach (string Kee in DependedResourceKeys)
				{
					if (string.Compare(Kee, RequestorKey) == 0)
						RequestorExist = true;
				}
				if (!RequestorExist)
					DependedResourceKeys.Add(RequestorKey);
			}
		}

		public void Respond(string Data)
		{
            if (!string.IsNullOrEmpty(Data))
            {
                State NewState = new State();
                NewState.Data = Data;
                States.Add(NewState);

                //Inform local resources
                foreach (string DependedResourceKey in DependedResourceKeys)
                {
                    RM.Put(DependedResourceKey);
                }
                //Inform global
                if(RM.InformGlobal!=null)
                    RM.InformGlobal(Key);
            }
            else
            {
                //1. Use MyTryNewState to Update Current Resource
                String NewData=null;
                if (MyTryNewState != null)
                {
                    //Get all depend resources
                    List<Resource> DependResources = new List<Resource>();
                    foreach (string DependResourceKey in DependResourceKeys)
                    {
                        Respond R_Resource = RM.Get(DependResourceKey, Key);
                        DependResources.Add(R_Resource.Resource);
                    }
                    NewData = MyTryNewState(DependResources.ToArray(), this);
                }

                //2. If there is a new state add it into states and inform other resources
                if (NewData!=null)
                {
                    State NewState = new State();
                    NewState.Data = NewData;
                    States.Add(NewState);
                    //Inform local resources
                    foreach (string DependedResourceKey in DependedResourceKeys)
                    {
                        RM.Put(DependedResourceKey);
                    }
                    //Inform global
                    if (RM.InformGlobal != null)
                        RM.InformGlobal(Key);
                }
            }
		}

        public void Respond(Resource Dul_Resource)
        {
            if (TryAddNewState(Dul_Resource))
            {
                //Inform local resources
                foreach (string DependedResourceKey in DependedResourceKeys)
                {
                    RM.Put(DependedResourceKey);
                }
                //Inform global
                if (RM.InformGlobal != null)
                    RM.InformGlobal(Key);
            }
        }

        /************************************************************************
        *Private Methods
        * ***********************************************************************/
        private bool TryAddNewState(Resource Dul_Resource)
        {
            bool HaveNewState = false;
            if(Dul_Resource.States.Count> States.Count)
            {
                foreach(State s in Dul_Resource.States)
                {
                    bool StateIncluded = false;
                    foreach (State ls in States)
                    {
                        if (string.Compare(s.Id,ls.Id)==0)
                        {
                            StateIncluded = true;
                            break;
                        }
                    }
                    if (!StateIncluded)
                    {
                        States.Add(s);
                        HaveNewState = true;
                    }
                }
            }
            return HaveNewState;
        }
    }

	public class State
	{
        //Id to tell the different between states
        public string Id= Guid.NewGuid().ToString();

        //Time(utc) this version is created 
        public string TimeStamp=DateTime.UtcNow.ToString("s");

		//Final data store in this version
		public string Data=""; // In Json string format
	}
}

