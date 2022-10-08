using System;
using System.Threading;
using System.Net;
using System.IO;
using System.Text.Json;

namespace HTTPResponse
{
    public class HttpServer : IDisposable
    {
        static HttpListener listener;
        static Stream output;
        public ServerStatus Status = ServerStatus.Stop;
        string _path;
        private readonly int _port;

        public HttpServer()
        {
            if (File.Exists(Path.GetFullPath("Config.json")))
            {
                string fileName = "Config.json";
                string jsonString = File.ReadAllText(fileName);
                ServerSettings setting = JsonSerializer.Deserialize<ServerSettings>(jsonString);
                _port = setting.Port;
                _path = setting.Path;
            }
            else
            {
                ServerSettings setting = new ServerSettings(); 
                _port = setting.Port;
                _path = setting.Path;
            }
            listener = new HttpListener();
            // установка адресов прослушки
            listener.Prefixes.Add($"http://localhost:{_port}/");
        }

        public void Begin()
        {
            if (Status == ServerStatus.Start)
            {
                Console.WriteLine("Уже запущен");
                return;
            }
            Console.WriteLine("Запуск сервера...");
            new HttpServer();

            listener.Start();
            
            Console.WriteLine("Ожидание подключений...");
            Status = ServerStatus.Start;
            // метод GetContext блокирует текущий поток, ожидая получение запроса
            Listen();
        }
        public void Listen()
        {
            listener.BeginGetContext(new AsyncCallback(ListenCallback), listener);
        }
        public void ListenCallback(IAsyncResult result)
        {
            if (listener.IsListening)
            {
                var context = listener.EndGetContext(result);
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                var path = Directory.GetCurrentDirectory();
                byte[] buffer;

                if (Directory.Exists(path))
                
                {
                    buffer = FileFinder.GetFile(context.Request.RawUrl.Replace("%20"," "), "index.html", _path);
                   
                    response.Headers.Set("Content-Type", "text/css");
                    response.Headers.Add("Content-Type", "text/html");
                    
                   
                    if (buffer == null)
                    {
                        response.Headers.Set("Content-Type", "text/plain");
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        string err = "404 - Not Found";
                        buffer = System.Text.Encoding.UTF8.GetBytes(err);
                    }
                }
                else
                {
                    
                    string err = $"{path} is not found";
                    buffer = System.Text.Encoding.UTF8.GetBytes(err);
                    
                }
                
                output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
                Listen();
            }
        }
        public void Stop()
        {
            if (Status == ServerStatus.Stop) return;
            // останавливаем прослушивание подключений
            listener.Abort();
            Status = ServerStatus.Stop;
            listener = new HttpListener();
            Console.WriteLine("Обработка подключений завершена");
        }
        public void Dispose()
        {
            Stop();
        }
    }
}