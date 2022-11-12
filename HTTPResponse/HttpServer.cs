using System;
using System.Threading;
using System.Net;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HTTPResponse.Controllers;
using HTTPResponse.Attributes;

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
            Console.WriteLine ("Запуск сервера...");

            new HttpServer();

            listener.Start();
            
            Console.WriteLine("Ожидание подключений...");
            Status = ServerStatus.Start;
            Listen();
        }
        public async void Listen()
        {
            while (listener.IsListening)
            {
                try
                {
                    var context = await listener.GetContextAsync();

                    if (MethodHandler(context)) continue;

                    StaticFiles(context);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }
            }
        }
        private void StaticFiles(HttpListenerContext context)
        {
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                var path = Directory.GetCurrentDirectory();
                byte[] buffer;

                if (Directory.Exists(path))

                {
                    buffer = FileFinder.GetFile(context.Request.RawUrl.Replace("%20", " "), "index.html", _path);

                    response.Headers.Set("Content-Type", "text/css");
                    response.Headers.Add("Content-Type", "text/html");


                    if (buffer == null)
                    {
                        response.Headers.Set("Content-Type", "text/plain");
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        string err = "404 - Not Found";
                        buffer = Encoding.UTF8.GetBytes(err);
                    }
                }
                else
                {

                    string err = $"{path} is not found";
                    buffer = Encoding.UTF8.GetBytes(err);

                }

                output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
                context.Response.Close();
        }
        public void Stop()
        {
            if (Status == ServerStatus.Stop) return;
            // останавливаем прослушивание подключений

            listener.Stop();

            Status = ServerStatus.Stop;
            Console.WriteLine("Обработка подключений завершена");
        }
        public void Dispose()
        {
            Stop();
        }
        private bool MethodHandler(HttpListenerContext context)
        {
            // объект запроса
            HttpListenerRequest request = context.Request;
            

            // объект ответа
            HttpListenerResponse response = context.Response;

            string controllerName = context.Request.Url.Segments[1].Replace("/", "");

            var assembly = Assembly.GetExecutingAssembly();

            var controller = assembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(HttpController))).FirstOrDefault(c => c.Name.ToLower() == controllerName.ToLower());

            if (controller == null) return false;

            var test = typeof(HttpController).Name;
            var methods = controller.GetMethods().Where(t => t.GetCustomAttributes(true)
                                                              .Any(attr => attr.GetType().Name == $"Http{context.Request.HttpMethod}"));


            if (methods == null) return false;

            string[] strParams;

            

            if (context.Request.HttpMethod == "POST")
            {
                handlePOST(context, out strParams);
            }
            else if (context.Request.HttpMethod == "GET")
            {
                handleGET(context, out strParams);
            }
            else
                return false;

            if (strParams is null) return false;

            var method = methods.Where(m => m.GetParameters().Length == strParams.Length).FirstOrDefault();

            if (method is null) return false;

            object[] queryParams = method.GetParameters()
                                .Select((p, i) => Convert.ChangeType(strParams[i], p.ParameterType))
                                .ToArray();


            // разделить код
            var ret = method.Invoke(Activator.CreateInstance(controller), queryParams);

            response.ContentType = "Application/json";

            byte[] buffer = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ret));
            response.ContentLength64 = buffer.Length;

            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            output.Close();
            return true;
        }
        private static void handleGET(HttpListenerContext context, out string[] strParams)
        {
            if (context.Request.Url.Segments.Length < 2)
            {
                strParams = null;
                return;
            }
            string controllerName = context.Request.Url.Segments[1].Replace("/", "");

            strParams = context.Request.Url
                                    .Segments
                                    .Skip(2)
                                    .Select(s => s.Replace("/", ""))
                                    .ToArray();
        }
        private static void handlePOST(HttpListenerContext context, out string[] strParams)
        {
            var body = GetPOSTBody(context.Request);

            if (body is null) 
            {
                strParams = null;
                return;
            }
            // takes values from string
            var valuesOfPost = body.Split('&');
            for (int i = 0; i < valuesOfPost.Length; i++)
            {
                valuesOfPost[i] = valuesOfPost[i].Split("=")[1];
            }
            strParams = valuesOfPost;
        }
        /// Takes body from POST request 
        /// Returns string
        private static string GetPOSTBody(HttpListenerRequest request)
        {
            if (!request.HasEntityBody || request.ContentType == null)
            {
                Console.WriteLine("No client data was sent with the request.");
                Console.WriteLine("Client data content type {0}", request.ContentType);
                return null;
            }
            Stream body = request.InputStream;
            Encoding encoding = request.ContentEncoding;
            StreamReader reader = new StreamReader(body, encoding);
            // Convert the data to a string and display it on the console.
            string s = reader.ReadToEnd();
            body.Close();
            reader.Close();
            return s; 
        }
    }
}
