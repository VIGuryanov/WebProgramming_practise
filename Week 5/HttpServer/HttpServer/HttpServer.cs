using System.Net;
using System.Text;
using Microsoft.AspNetCore.StaticFiles;
using System.Text.Json;

namespace HttpServer
{
    public class ServerSettings
    {
        public int Port { get; } = 8888;
        public string Path { get; } = "./server";
    }

    public class HTTPServer
    {
        Task mainProcess;
        Task ServerDefault() => new(() => Listen());
        ServerSettings serverSettings;

        public bool IsRun { get; private set; } = false;
        static bool isWaiting = false;
        HTTPServerFileIO IOSystem;

        public HTTPServer()
        {
            mainProcess = ServerDefault();
            UpdateSettings();
        }

        void UpdateSettings()
        {
            serverSettings = JsonSerializer.Deserialize<ServerSettings>(File.ReadAllText("./settings.json"));
            IOSystem = new(serverSettings.Path);
        }

        void Listen()
        {
            HttpListener listener = new();
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
                HttpListenerRequest request = context.Request;
                SendResponse(context, IOSystem.GetResponseFile(context));
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

        static void SendResponse(HttpListenerContext context, byte[] buffer)
        {
            HttpListenerResponse response = context.Response;
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
