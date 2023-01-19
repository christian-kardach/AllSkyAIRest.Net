using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllSkyAIRestServer.Net
{
    internal class Configuration
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string Url { get; set; }

        public string Model { get; set; }
        public bool ConfigOk { get; set; }

        public Configuration()
        {
            ReadConfig();
        }

        public void ReadConfig()
        {
            if(!File.Exists(@".\\config.cfg"))
            {
                Console.WriteLine("No config.cfg file.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            var confFile = File.ReadAllLines(@".\\config.cfg");
            var confList = new List<string>(confFile);
            foreach (var conf in confList)
            {
                if(conf.StartsWith("HOST"))
                {
                    var h = conf.Split('=').Last();
                    if(string.IsNullOrEmpty(h))
                    {
                        Console.WriteLine("Host can't be empty, check config.cfg");
                    }
                    else
                    {
                        Host= h;
                    }
                }

                else if (conf.StartsWith("PORT"))
                {
                    var h = conf.Split('=').Last();
                    if (string.IsNullOrEmpty(h))
                    {
                        Console.WriteLine("Port can't be empty, check config.cfg");
                    }
                    else
                    {
                        Port = h;
                    }
                }

                else if (conf.StartsWith("URL"))
                {
                    var h = conf.Split('=').Last();
                    if (string.IsNullOrEmpty(h))
                    {
                        Console.WriteLine("Port can't be empty, check config.cfg");
                    }
                    else
                    {
                        Url = h;
                    }
                }

                else if (conf.StartsWith("MODEL"))
                {
                    var h = conf.Split('=').Last();
                    if (string.IsNullOrEmpty(h))
                    {
                        Console.WriteLine("Model can't be empty, check config.cfg");
                    }
                    else
                    {
                        Model = h;
                    }
                }
            }

            if(string.IsNullOrEmpty(Host) && string.IsNullOrEmpty(Port) && string.IsNullOrEmpty(Url) && string.IsNullOrEmpty(Model))
            {
                ConfigOk = false;
            }
            else
            {
                ConfigOk = true;
            }
        }
    }
}
