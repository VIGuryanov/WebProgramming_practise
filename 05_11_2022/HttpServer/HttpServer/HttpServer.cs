using System.Net;
using System.Text;
using Microsoft.AspNetCore.StaticFiles;
using System.Text.Json;
using System.Reflection;
using HttpServer.Attributes;
using HttpServer.Models;

namespace HttpServer
{
    public class ServerSettings
    {
        public int Port { get; }
        public string Path { get; }

        public ServerSettings(int port, string path)
        {
            Port = port;
            Path = path;
        }
    }

    public class HTTPServer
    {
        Task mainProcess;
        Task ServerDefault() => new(() => Listen());
        ServerSettings serverSettings;
        HTTPServerFileIO IOSystem;

        HttpListener listener = new();
        public bool IsRun { get; private set; } = false;
        static bool isWaiting = false;        

        public HTTPServer()
        {
            mainProcess = ServerDefault();
            UpdateSettings();
        }

        void UpdateSettings()
        {
            var settingsDeserialized = JsonSerializer.Deserialize<ServerSettings>(File.ReadAllText("./settings.json"));
            if(settingsDeserialized == null)
                throw new JsonException("Returned null after settings deserialization");
            serverSettings = settingsDeserialized;
            IOSystem = new(serverSettings.Path);
        }

        void Listen()
        {
            listener.Start();
            listener.Prefixes.Add($"http://localhost:{serverSettings.Port}/");
            if (IsRun)
            {
                listener.Start();
                Console.WriteLine("Waiting for requests...");
                while (IsRun)
                {
                    if (!isWaiting)
                        Processing(listener);
                }
                isWaiting = false;
                listener.Stop();
                Console.WriteLine("Processing of connections ended");
            }
        }

        async void Processing(HttpListener listener)
        {
            try
            {
                isWaiting = true;
                HttpListenerContext context = await listener.GetContextAsync();
                Console.WriteLine("Got request");
                if (!MethodHandler(context))
                {
                    HttpListenerRequest request = context.Request;
                    SendResponse(IOSystem.GetResponseFile(context), context.Response);
                }
                Console.WriteLine("Sent response");
            }
            catch (HttpListenerException)
            {
                Console.WriteLine("Request awaiting interrupted");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Файл сайта не найден");
                ServerCommands.Stop(this);
            }
            catch (ObjectDisposedException)
            {

            }
            isWaiting = false;
        }

        private bool MethodHandler(HttpListenerContext httpContext)
        {
            // объект запроса
            HttpListenerRequest request = httpContext.Request;
            
            httpContext.Response.ContentType = "Application/json";

            if (httpContext.Request.Url.Segments.Length < 2) return false;

            string[] pathParts = httpContext.Request.Url
                                    .Segments
                                    .Select(s => s.Replace("/", ""))
                                    .ToArray();

            Type? controller = AttributesRecognize.GetClassByControllerAttribute(pathParts[1].Replace("/", ""));

            if (controller == null) return false;

            MethodInfo? method = AttributesRecognize.GetMethodByHttpAttribute(httpContext, pathParts[2], controller);

            if (method == null) return false;

            string text;
            using (var reader = new StreamReader(request.InputStream,
                                     request.ContentEncoding))
            {
                text = reader.ReadToEnd();
            }

            object[] methodParams = null;

            try
            {
                switch (request.HttpMethod)
                {
                    case "GET":
                        methodParams = (request.Url.Query == "") ? Array.Empty<object>() :
                            new object[] { new string(request.Url.Query.Skip(1).ToArray()) };
                        break;
                    case "POST":
                        methodParams = (text == "") ? Array.Empty<object>() : text.Split(new[] { '&', '=' });
                        methodParams = methodParams.Where((x, i) => i % 2 == 1).ToArray();
                        break;
                    default:
                        throw new NotImplementedException("Logic for another methods not implemented yet");
                }

                if (method.GetParameters().Where(x => x.ParameterType.Name == "HttpListenerContext").Count() == 1)
                    methodParams = methodParams.Append(httpContext).ToArray();

                methodParams = method.GetParameters()
                                    .Select((p, i) => Convert.ChangeType(methodParams[i], p.ParameterType))
                                    .ToArray();

                var ret = method.Invoke(Activator.CreateInstance(controller), methodParams);

                SendResponse(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ret)), httpContext.Response);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        static void SendResponse(byte[] buffer, HttpListenerResponse response)
        {
            response.ContentLength64 = buffer.Length;
            using Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
        }

        public class ServerCommands
        {
            static readonly object Locker = new();

            public static void Start(HTTPServer server)
            {
                if (!server.IsRun)
                    lock (Locker)
                    {
                        if (!server.IsRun)
                        {
                            server.UpdateSettings();
                            server.IsRun = true;
                            server.mainProcess.Start();
                        }
                        else
                            Console.WriteLine("Double run unsupported");
                    }
                else
                    Console.WriteLine("Double run unsupported");
            }

            public static void Stop(HTTPServer server)
            {
                if (server.IsRun)
                    lock (Locker)
                    {
                        if (server.IsRun)
                        {
                            server.IsRun = false;
                            server.listener.Stop();
                            while (!server.mainProcess.IsCompleted) ;
                            server.mainProcess = server.ServerDefault();
                            server.UpdateSettings();
                            Console.WriteLine("Server stopped");
                        }
                        else
                            Console.WriteLine("Server not running now");
                    }
                else
                    Console.WriteLine("Server not running now");
            }
        }
    }

    public class HTTPServerFileIO
    {
        public readonly string Path = "./server";

        public HTTPServerFileIO(string serverPath)
        {
            Path = serverPath;
        }

        public byte[] GetResponseFile(HttpListenerContext context)
        {
            byte[]? buffer;

            if (Directory.Exists(Path))
            {
                buffer = GetFileBytes(context.Request.RawUrl.Replace("%20", " "), out string? extension);
                if (buffer == null)
                {
                    context.Response.Headers.Set("Content-type", "text/plain");
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Encoding.UTF8.GetBytes("404 - not found");
                }

                if (!new FileExtensionContentTypeProvider().TryGetContentType(extension, out string contentType))
                    contentType = "text/plain";
                context.Response.Headers.Set("Content-type", contentType);
                return buffer;
            }
            return Encoding.UTF8.GetBytes($"Directory '{Path}' not found");
        }

        byte[]? GetFileBytes(string rawUrl, out string? extension)
        {
            var path = Path + rawUrl;

            if (Directory.Exists(path))
                path += "index.html";
            if (File.Exists(path))
            {
                extension = "." + path.Split('.').Last();
                return File.ReadAllBytes(path);
            }
            extension = null;
            return null;
        }
    }
}
