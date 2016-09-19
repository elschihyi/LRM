using LRM;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace Pipe
{
    class PipeServer
    {
        //static ResourceManager RM1 = new ResourceManager();
        static readonly string PipeSevername= "PipeServer";

        static void Main(string[] args)
        {
            StartServer();
            Thread.Sleep(2000);

            //Client
            var client = new NamedPipeClientStream(PipeSevername);
            client.Connect();
            StreamReader reader = new StreamReader(client);
            StreamWriter writer = new StreamWriter(client);

            while (true)
            {
                string input = Console.ReadLine();
                if (String.IsNullOrEmpty(input)) break;
                writer.WriteLine(input);
                writer.Flush();
                Console.WriteLine(reader.ReadLine());
            }

            reader.Close();
            writer.Close();
        }

        static void StartServer()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                NamedPipeServerStream PipeServer = new NamedPipeServerStream(PipeSevername);
                PipeServer.WaitForConnection();
                ThreadPool.QueueUserWorkItem((p) =>
                {
                    StreamReader reader = new StreamReader(PipeServer);
                    StreamWriter writer = new StreamWriter(PipeServer);
                    while (true)
                    {
                        string line = reader.ReadLine();
                        writer.WriteLine("in server"+line);
                        writer.Flush();
                    }
                });
                    
            });
        }
    }
}
