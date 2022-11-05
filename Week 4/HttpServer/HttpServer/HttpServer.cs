using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    public static class HTTPServer
    {
        public static Thread server = new Thread(() => Start());
        public static bool IsRun { get; private set; } = false;
        static bool isWaiting = false;
        public static void Start()
        {
            IsRun = true;
            HttpListener listener = new HttpListener();
            // установка адресов прослушки
            listener.Prefixes.Add("http://localhost:8888/google/");
            listener.Start();
            Console.WriteLine("Ожидание подключений...");
            while (IsRun)
            {
                if (!isWaiting)
                    Processing(listener);
            }
            isWaiting = false;
            listener.Stop();
            Console.WriteLine("Обработка подключений завершена");
        }

        static async void Processing(HttpListener listener)
        {
            try
            {
                // метод GetContext блокирует текущий поток, ожидая получение запроса
                isWaiting = true;
                HttpListenerContext context = await listener.GetContextAsync();
                HttpListenerRequest request = context.Request;
                // получаем объект ответа
                HttpListenerResponse response = context.Response;
                // создаем ответ в виде кода html
                string responseStr = File.ReadAllText(Directory.GetCurrentDirectory() + "\\google.html");
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseStr);
                // получаем поток ответа и пишем в него ответ
                response.ContentLength64 = buffer.Length;
                using Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
            }
            catch (HttpListenerException)
            {
                Console.WriteLine("Ожидание запроса прервано");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Файл сайта не найден");
                Stop();
            }
            catch (ObjectDisposedException)
            {

            }
            isWaiting = false;
        }

        public static void Stop()
        {
            IsRun = false;
            server = new Thread(() => Start());
        }
    }
}
