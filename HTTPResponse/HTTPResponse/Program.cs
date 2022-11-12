using System;
using System.Threading;
using System.Net;
using System.IO;

namespace HTTPResponse
{
    class HttpServer
    {
        private static object _lock = new object();
        static HttpListener listener;
        static Stream output;
        static void Main(string[] args)
        {
            Thread cmd = new Thread(CMDListen);
            cmd.Start();
            
            HttpServer.Begin();

        }
        public static void Begin()
        {
            listener = new HttpListener();
            // установка адресов прослушки
            listener.Prefixes.Add("http://localhost:8888/google/");
            listener.Start();
            Console.WriteLine("Ожидание подключений...");
            // метод GetContext блокирует текущий поток, ожидая получение запроса
            Listen();

        }
        public static void Listen()
        {
            try
            {
                while (true)
                {
                    var context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;
                    var responseString = File.ReadAllText("index.html");
                    //var responseString = new FileStream(@"C:\Users\Lenovo\source\repos\HTTPResponse\HTTPResponse\google\index.html", FileMode.Open);
                    

                    //string responseString = "<html><head><meta charset='utf8'></head><body>Привет мир!</body></html>";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Close();
            }
        }
        public static void Close()
        {
            // закрываем поток
            output?.Close();
            // останавливаем прослушивание подключений
            listener.Stop();
            listener = null;
            output = null;
            Console.WriteLine("Обработка подключений завершена");
            Console.Read();
        }

        private static void CMDListen()
        {
            while (true)
            {
                var command = Console.ReadLine();
                switch (command)
                {
                    case "close":
                        if (output != null)
                            Close();
                        break;
                    case "start":
                        if (listener == null)
                        {
                            lock (_lock)
                            {
                                if (listener == null)
                                {
                                    Thread startListener = new Thread(Begin);
                                    startListener.Start();
                                }
                            }
                        }
                        break;
                    case "restart":
                        if (output != null && listener!=null)
                        {
                            Close();
                            Thread startListener = new Thread(Begin);
                            startListener.Start();
                        }
                        else
                        {
                            Thread startListener = new Thread(Begin);
                            startListener.Start();
                        }
                        break;

                }
            }
        }
    }
}
