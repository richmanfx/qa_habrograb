using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using HtmlAgilityPack;
using WatiN.Core;
using Newtonsoft.Json;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace qa_habrograb
{
    class QAHabroGrabProgram
    {
        // Логер
        private static readonly ILog log = LogManager.GetLogger(typeof(QAHabroGrabProgram));

        // Однопоточное приложение
        // [STAThread]

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;           // Цвет текста в консоли
            Console.OutputEncoding = Encoding.UTF8;                 // Кодировка в консоли
            XmlConfigurator.Configure();                            // Конфигурация логера в XML-файле
            string config_file_name = "qa_habrograb_config.json";   // Имя файла конфигурации
            string site_page = "https://m.habrahabr.ru";

            // Считать настройки из файла конфигурации
            GrabberConfig config = new GrabberConfig();
            config = getSettings(config, config_file_name);
            if (config == null)
            {
                log.Error("Не удалось получить параметры конфигурации.");
                Environment.Exit(1);
            }
            /*
            log.Info(config.core_server.domain_name);
            log.Info(config.core_server.address);
            log.Info(config.core_server.port);
            log.Info(config.grabber.port);
            */

            // Слушать команды от сервера
            log.Debug("Запуск сервера приёма команд на порту " + config.grabber.port + ".");
            new GrabServer(Convert.ToInt32(config.grabber.port));





            log.Debug("Готово!");
            Console.ReadLine();

        } // end Main


        /// <summary>
        /// Считывает файл конфигурации. Если файла нет, то возвращает null.
        /// </summary>
        /// <param name="GrabberConfig config"></param>
        /// <param name="string file_name"></param>
        /// <returns>Конфигурацию типа GrabberConfig или null.</returns>
        private static GrabberConfig getSettings(GrabberConfig config, string file_name) {
            if (File.Exists(file_name))      // Существует ли JSON файл конфигурации
            {
                log.Debug("Считываем параметры из конфигурационного файла.");
                // Считать JSON строку из файла
                string json_string = File.ReadAllText(file_name, Encoding.UTF8);
                // Десериализация
                config = JsonConvert.DeserializeObject<GrabberConfig>(json_string);
                return config;
            } else
            {   
                log.Error("JSON файл конфигурации '" + file_name + "' не найден.");
                return null;
            }
        }
        

    } // end class QAHabroGrabProgram

    /// Класс для конфигурационных данных
    public class GrabberConfig
    {
        public Grabber grabber { get; set; }
        public CoreServer core_server { get; set; }
    }

    /// Класс для конфигурационных данных Грабера
    public class Grabber
    {
        public string port { get; set; }
    }

    /// Класс для конфигурационных данных Сервера
    public class CoreServer
    {
        public string domain_name { get; set; }
        public string address { get; set; }
        public string port { get; set; }
    }

    /// TCP сервер для приёма команд
    public class GrabServer
    {
        // Логер
        private static readonly ILog log = LogManager.GetLogger(typeof(QAHabroGrabProgram));

        HttpListener listener;           // Слушатель сокета

        /// <summary>
        /// Запускатель сервера
        /// </summary>
        /// <param name="Port"></param>
        public GrabServer(int port)
        {
            listener = new HttpListener();    
            listener.Prefixes.Add(String.Format("http://*:{0}/", port));    // Слушаем на всех интерфейсах на указанном порту
            listener.Start();       // Начинаем слушать

            // Принимаем новые соединения от клиентов
            for (;;)  // бесконечно
            {
                HttpListenerContext context = listener.GetContext();    //Ожидание входящего запроса
                HttpListenerRequest request = context.Request;          //Объект запроса
                HttpListenerResponse response = context.Response;       //Объект ответа

                // Ответ
           //    Stream inputStream = request.InputStream;
            //    Encoding encoding = request.ContentEncoding;
            //    StreamReader reader = new StreamReader(inputStream, encoding);
            //    string requestBody = reader.ReadToEnd();

                log.Debug(String.Format("{0} запрос получен: {1}", request.HttpMethod, request.Url));

                // Обработать команды 
                commandParser(request, response);

                // Послать ответ
                using (Stream stream = response.OutputStream) { }       
            }
        } // end конструктора GrabServer

        private void commandParser(HttpListenerRequest request, HttpListenerResponse response)
        {
            // string ping_template;
            string responseString;
            byte[] buffer;

            switch (request.HttpMethod)
            {
                case "GET":
                    response.StatusCode = (int)HttpStatusCode.OK;
                    responseString = @"<!DOCTYPE HTML>
                                            <html><head></head><body>
                                            <form method=""post"">
                                            <p><b>Name: </b><br>
                                            <input type=""text"" name=""myname"" size=""40""></p>
                                            <p><input type=""submit"" value=""send""></p>
                                            </form></body></html>";
                    response.ContentType = "text/html; charset=UTF-8";
                    buffer = Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    break;

                case "POST":
                    response.StatusCode = (int)HttpStatusCode.OK;
                    ShowRequestData(request);
                    responseString = @"<!DOCTYPE HTML>
                                            <html><head></head><body>
                                            <h1>Данные успешно переданы!</h1>
                                            </body></html>";
                    response.ContentType = "text/html; charset=UTF-8";
                    buffer = Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
            }
        } // end commandParser

        private void ShowRequestData(HttpListenerRequest request)
        {
                //есть данные от клиента?
                if (!request.HasEntityBody) return;

                Stream inputStream = request.InputStream;
                Encoding encoding = request.ContentEncoding;
                StreamReader reader = new StreamReader(inputStream, encoding);
                string requestBody = reader.ReadToEnd();

                log.Debug(String.Format("Данные из формы: " + requestBody));

        } 

        // Останавливаем работу сервера
        ~GrabServer()
        { if (listener != null) listener.Stop(); } // если есть живой слушатель
        
    }  // end class GrabServer

} // end namespace qa_habrograb
