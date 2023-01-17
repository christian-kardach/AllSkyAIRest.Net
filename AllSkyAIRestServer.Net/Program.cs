using Grapevine;
using Grapevine.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace AllSkyAIRestServer.Net
{
    public sealed class TrResource : RESTResource
    {
        Classify classify = new Classify();

        [RESTRoute(Method = HttpMethod.GET, PathInfo = @"^/classify")]
        public void HandleGetGreetRequest(HttpListenerContext context)
        {
            //var confFile = File.ReadAllLines(@".\\configuration.txt");
            //var conf = new List<string>(confFile);

            string result = classify.ClassifyImage(@"https://allsky.tristarobservatory.com/image.jpg");
            Console.WriteLine("Classification done!");
            //Console.WriteLine("URL: {0}", context.Request.RawUrl);
            SendTextResponse(context, result);
        }
        /*
        [RESTRoute(Method = HttpMethod.GET, PathInfo = @"^/endpoint?.+$")]
        [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/endpoint?.+$")]
        public void HandleEndpointRequest(HttpListenerContext context)
        {
            Console.WriteLine("URL: {0}", context.Request.RawUrl);
            Console.WriteLine("Method: {0}", context.Request.HttpMethod);

            try
            {
                foreach (string k in context.Request.QueryString)
                {
                    Console.WriteLine("{0}: {1}", k, context.Request.QueryString[k]);
                }

                if (context.Request.HttpMethod.Equals("GET"))
                {
                    SendTextResponse(context, "GET");
                }

                if (context.Request.HttpMethod.Equals("POST"))
                {
                    SendTextResponse(context, "POST");
                }
            }
            catch (Exception e)
            {
                SendTextResponse(context, e.Message + "\n" + e.StackTrace);
            }
        }
        */
        [RESTRoute]
        public void HandleAllGetRequests(HttpListenerContext context)
        {
            SendTextResponse(context, "ROOT NODE");
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
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
                server.Host = args[0];
                server.Port = args[1];
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

            /*
            else
            {
                Dictionary<string, HttpMethod> method = new Dictionary<string, HttpMethod>()
                {
                    { "GET", HttpMethod.GET },
                    { "POST", HttpMethod.POST }
                };

                //
                // As client
                //
                try
                {
                    var client = new RESTClient("http://" + options.Host + ":" + options.Port);
                    var request = new RESTRequest(options.Url);
                    request.Method = method[options.Method];

                    var response = client.Execute(request);
                    Console.WriteLine("Response: " + response.Content);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + "\n" + e.StackTrace);
                }
            }
            */

        }
    }
}
