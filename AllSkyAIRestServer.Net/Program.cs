using Grapevine;
using Grapevine.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using SixLabors.ImageSharp;

namespace AllSkyAIRestServer.Net
{
    public sealed class TrResource : RESTResource
    {
        Classify classify = new Classify();
        Configuration configuration = new Configuration();

        [RESTRoute(Method = HttpMethod.GET, PathInfo = @"^/classify")]
        public void HandleGetGreetRequest(HttpListenerContext context)
        {
            string result = classify.ClassifyImage(configuration.Url);
            Console.WriteLine("Classification done!");
            SendTextResponse(context, result);
        }
        
        [RESTRoute]
        public void HandleAllGetRequests(HttpListenerContext context)
        {
            SendTextResponse(context, "ROOT NODE");
        }
    }

    class Program
    {
        static Configuration configuration = new Configuration();

        static void Main(string[] args)
        {
            
            configuration.ReadConfig();
            if(!configuration.ConfigOk)
            {
                Console.WriteLine("Error in config file, please check");
                Console.ReadKey();
                Environment.Exit(0);
            }

            var exitEvent = new ManualResetEvent(false);
            //
            // As server
            //
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            try
            {
                var server = new RESTServer();

                server.Host = configuration.Host;
                server.Port = configuration.Port;

                Console.WriteLine($"AllSkyAI running: {args[0]}:{args[1]}");
                Console.WriteLine($"Make a request to endpoint: http://{args[0]}:{args[1]}/classify");
                server.Start();

                exitEvent.WaitOne();
                server.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }
        }
    }
}
