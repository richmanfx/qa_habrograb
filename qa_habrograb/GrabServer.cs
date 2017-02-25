using log4net;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace qa_habrograb
{

    /// TCP сервер для приёма команд
    public class GrabServer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(QAHabroGrabProgram));        // Логер

        HttpListener listener;           // Слушатель сокета

        /// Запускатель сервера
        /// <param name="Port"></param>
        public GrabServer(int port)
        {
            listener = new HttpListener();
            listener.Prefixes.Add(String.Format("http://*:{0}/", port));    // Слушаем на указанном порту
            listener.Start();       // Начинаем слушать

            // Принимаем новые соединения от клиентов
            for (;;)  // бесконечно
            {
                HttpListenerContext context = listener.GetContext();    //Ожидание входящего запроса
                HttpListenerRequest request = context.Request;          //Объект запроса
                HttpListenerResponse response = context.Response;       //Объект ответа

                log.Debug(String.Format("{2}: {0} request is received: {1}", request.HttpMethod, request.Url, Thread.CurrentThread.Name));

                // Обработать входящий запрос
                SwitchingRequests(request, response);

                // Послать ответ
                using (Stream stream = response.OutputStream) { }
            }
        } // end конструктора GrabServer


        /// Парсер GET и POST запросов от сервера-ядра
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

                case "DELETE":
                    HendlingDELETE(request, response, out responseString, out buffer);
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
            }
        } // end SwitchingRequests


        /// Обработчик DELETE запроса для закрытия приложения
        private void HendlingDELETE(HttpListenerRequest request, HttpListenerResponse response, 
                                    out string responseString, out byte[] buffer)
        {
            response.StatusCode = (int)HttpStatusCode.OK;

            PositiveAnswer("Bye!");     // положительный ответ Ядру

            SendResponse(response, out responseString, out buffer);
            log.Debug("Работа Грабера закончена.");
            Environment.Exit(0);
        }


        /// Обработчик POST запроса от сервера-ядра
        private void HendlingPOST(HttpListenerRequest request, HttpListenerResponse response, 
                                  out string responseString, out byte[] buffer)
        {
            response.StatusCode = (int)HttpStatusCode.OK;
            string incomingJson = CleaningRequestData(request);     // Очистка данных запроса
            if (incomingJson == null)
            {
                string negative_response = "Данных в POST запросе нет.";
                log.Debug(String.Format("{0}: {1}",Thread.CurrentThread.Name, negative_response));
                NegativeAnswer(negative_response);      // отрицательный ответ Ядру
            }
            else
            {
                // TODO: Верификация /grab запроса GrabRequest от Ядра - Json.NET Schema (платная???)
                // ValidateGrabRequest(incomingJson);

                // Десериализовать данные запроса в объект GrabRequest
                GrabRequest gr = new GrabRequest();
                try
                {
                    gr = JsonConvert.DeserializeObject<GrabRequest>(incomingJson);
                }
                catch (Newtonsoft.Json.JsonReaderException ex)
                {
                    QAHabroGrabProgram.grab_response.Result = false;
                    QAHabroGrabProgram.grab_response.Error.Text = "Ошибка в запросе GrabRequest от Ядра.";
                    QAHabroGrabProgram.grab_response.Error.Time = DateTime.Now.ToString("s");
                    QAHabroGrabProgram.grab_response.Error.Exception.StackTrace.Add(ex.StackTrace);
                    QAHabroGrabProgram.grab_response.Error.Exception.Message = ex.Message;
                    QAHabroGrabProgram.grab_response.Error.Exception.ClassName = ex.Source;

                    SendResponse(response, out responseString, out buffer);
                    return;
                }

                // Добавить запрос на грабинг в очередь
                log.Debug(String.Format("{0}: Добавление /grab запроса GrabRequest от Ядра в очередь.", Thread.CurrentThread.Name));
                string result = QAHabroGrabProgram.rq.AddRequest(gr);
                if (result == "OK")
                {
                    PositiveAnswer(result);     // положительный ответ Ядру
                }
                else
                {
                    NegativeAnswer(result);     // отрицательный ответ Ядру
                }
            }
            SendResponse(response, out responseString, out buffer);
        }


        /// Обработчик GET запроса от сервера-ядра
        private static void HendlingGET(HttpListenerResponse response,
                                       out string responseString,
                                       out byte[] buffer)
        {
            DateTime currentDateTime = DateTime.Now;
            response.StatusCode = (int)HttpStatusCode.OK;

            // В ответ послать PingResponse
            responseString = JsonConvert.SerializeObject(QAHabroGrabProgram.ping_response);

            response.ContentType = "content-type: application/json";
            buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            log.Debug(String.Format("{1}: Send answer on GET request: {0}", responseString, Thread.CurrentThread.Name));
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }


        /// Сформировать положительный ответ Ядру
        private static void PositiveAnswer(string text)
        {
            QAHabroGrabProgram.grab_response.Result = true;
            QAHabroGrabProgram.grab_response.Error.Text = text;
            QAHabroGrabProgram.grab_response.Error.Time = DateTime.Now.ToString("s");
            QAHabroGrabProgram.grab_response.Error.Exception.StackTrace.Clear();
            QAHabroGrabProgram.grab_response.Error.Exception.Message = "";
            QAHabroGrabProgram.grab_response.Error.Exception.ClassName = "";
        }

        /// Отправление ответа на запрос
        private static void SendResponse(HttpListenerResponse response, out string responseString, out byte[] buffer)
        {
            responseString = JsonConvert.SerializeObject(QAHabroGrabProgram.grab_response);
            response.ContentType = "content-type: application/json";
            buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            log.Debug(String.Format("{1}: Send answer on POST request: {0}.", responseString, Thread.CurrentThread.Name));
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        /// Формирует негативный ответ Ядру
        private static void NegativeAnswer(string error_text)
        {
            QAHabroGrabProgram.grab_response.Result = false;
            QAHabroGrabProgram.grab_response.Error.Text = error_text;
            QAHabroGrabProgram.grab_response.Error.Time = DateTime.Now.ToString("s");
        }

        /*
        /// Верификация /grab запроса GrabRequest от Ядра - Json.NET Schema: http://www.newtonsoft.com/jsonschema
        private void ValidateGrabRequest(string incomingJson)
        {
            
        }
        */


        /// Очистка данных запроса
        private string CleaningRequestData(HttpListenerRequest request)
        {
            string clearRequestJson;
            Stream inputStream = request.InputStream;
            StreamReader reader = new StreamReader(inputStream, request.ContentEncoding);

            // Есть данные от клиента?
            if (!request.HasEntityBody)
            {
                clearRequestJson = null;
            }
            else
            {
                string requestBody = reader.ReadToEnd().Replace("\n", "").Trim();   // Убрать переводы строк, начальные и конечные пробелы
                clearRequestJson = Regex.Replace(requestBody, "[ ]+", " ");         // Убрать повторяющиеся пробелы
                log.Debug(String.Format("{1}: Body: {0}", clearRequestJson, Thread.CurrentThread.Name));
            }
            inputStream.Close();
            reader.Close();
            return clearRequestJson;
        }

        /// Останавливаем работу сервера
        ~GrabServer()
        { if (listener != null) listener.Stop(); } // если есть живой слушатель

    }  // end class GrabServer
}
