using System;
using System.Threading;
using System.Net;
using System.IO;

namespace HTTPResponse
{
    internal class Program
    {
        static bool _appIsRunning = true;
        public static void Main()
        {
            using (var server = new HttpServer())
            {
                server.Begin();
                while (_appIsRunning)
                {
                    CMDListen(Console.ReadLine()?.ToLower(), server);
                }
            }

        }
        public static void CMDListen(string command, HttpServer server)
        {

                switch (command)
                {
                    case "close":
                        server.Stop();
                        break;
                    case "start":
                        server.Begin();
                        break;
                    case "restart":
                        server.Stop();
                        server.Begin();
                        break;
                    case "exit":
                        _appIsRunning = false;
                        break;
                }
        }
    }
}
