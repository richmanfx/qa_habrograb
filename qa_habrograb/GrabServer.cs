using log4net;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace qa_habrograb
{

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
            // listener.Prefixes.Add(String.Format("http://0.0.0.0:{0}/", port));    // Слушаем на всех интерфейсах на указанном порту
            listener.Prefixes.Add("http://*:7777/");
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

                log.Debug(String.Format("{0} request is received: {1}", request.HttpMethod, request.Url));

                // Обработать команды 
                SwitchingRequests(request, response);

                // Послать ответ
                using (Stream stream = response.OutputStream) { }
            }
        } // end конструктора GrabServer


        // Парсер GET и POST запросов от сервера-ядра
        private void SwitchingRequests(HttpListenerRequest request, HttpListenerResponse response)
        {
            // string ping_template;
            string responseString;
            byte[] buffer;

            switch (request.HttpMethod)
            {
                case "GET":
                    HendlingGET(response, out responseString, out buffer);
                    break;

                case "POST":
                    HendlingPOST(request, response, out responseString, out buffer);
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
            }
        } // end SwitchingRequests


        // Обработчик GET запроса от сервера-ядра
        private static void HendlingGET(HttpListenerResponse response, 
                                       out string responseString, 
                                       out byte[] buffer)
        {
            DateTime currentDateTime = DateTime.Now;
            response.StatusCode = (int)HttpStatusCode.OK;

            //    responseString = String.Format("{'result':'true','error':{'text':'Русскоязычное понятное осмысленное описание ошибки.','time':'{0}','exception':{'className':'ИмяКласса','message':'Сообщение исключения','stackTrace':'Стек вызовов исключения','inner':{'className':'ИмяКласса','message':'Сообщение исключения','stackTrace':'Стек вызовов исключения'}}}}", currentDateTime.ToString("s"));

            // Читать: http://andrey.moveax.ru/post/tools-visualstudio-paste-as-json-or-xml
            // https://metanit.com/sharp/tutorial/6.5.php
            // http://xn--d1aiecikab7a.xn--p1ai/json_csharp/
            // https://msdn.microsoft.com/ru-ru/library/bb412179(v=vs.110).aspx
            responseString = String.Format("Фыва:  'dadфывыфыв' : '{0}'", currentDateTime.ToString("s"));

            response.ContentType = "content-type: application/json";
            buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            log.Debug(String.Format("Send answer on GET request: {0}", responseString));
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }


        // Обработчик POST запроса от сервера-ядра
        private void HendlingPOST(HttpListenerRequest request, HttpListenerResponse response, 
                                 out string responseString, out byte[] buffer)
        {
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
        }

        private void ShowRequestData(HttpListenerRequest request)
        {
            //есть данные от клиента?
            if (!request.HasEntityBody) return;

            Stream inputStream = request.InputStream;
            Encoding encoding = request.ContentEncoding;
            StreamReader reader = new StreamReader(inputStream, Encoding.Default);
            string requestBody = reader.ReadLine();

            log.Debug(String.Format("Data: {0}", requestBody));

        }

        // Останавливаем работу сервера
        ~GrabServer()
        { if (listener != null) listener.Stop(); } // если есть живой слушатель

    }  // end class GrabServer
}
