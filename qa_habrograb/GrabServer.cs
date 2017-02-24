using log4net;
using Newtonsoft.Json;
using qa_habrograb;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace qa_habrograb
{

    /// TCP сервер для приёма команд
    public class GrabServer
    {
        // Логер
        private static readonly ILog log = LogManager.GetLogger(typeof(QAHabroGrabProgram));

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

                log.Debug(String.Format("{0} request is received: {1}", request.HttpMethod, request.Url));

                // Обработать входящий запрос
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


            
            // Читать: 
            // http://xn--d1aiecikab7a.xn--p1ai/json_csharp/
            // https://msdn.microsoft.com/ru-ru/library/bb412179(v=vs.110).aspx

            // В ответ послать PingResponse
            responseString = JsonConvert.SerializeObject(QAHabroGrabProgram.ping_response);

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
            string incomingJson = CleaningRequestData(request);     // Очистка данных запроса
            if (incomingJson == null)
            {
                string negative_response = "Данных в POST запросе нет.";
                log.Debug(negative_response);
                NegativeAnswer(negative_response);      // отрицательный ответ Ядру
            }
            else
            {
                // Верификация /grab запроса GrabRequest от Ядра - Json.NET Schema (платная???)
                // ValidateGrabRequest(incomingJson);

                // Десериализовать данные запроса в объект GrabRequest
                GrabRequest gr = JsonConvert.DeserializeObject<GrabRequest>(incomingJson);

                // Добавить запрос на грабинг в очередь
                log.Debug("Добавление /grab запроса GrabRequest от Ядра в очередь.");
                string result = QAHabroGrabProgram.rq.AddRequest(gr);
                if (result == "OK")
                {
                    // Сформировать положительный ответ Ядру
                    QAHabroGrabProgram.grab_response.Result = true;
                    QAHabroGrabProgram.grab_response.Error.Text = result;
                    QAHabroGrabProgram.grab_response.Error.Time = DateTime.Now.ToString("s");
                }
                else
                {
                    NegativeAnswer(result);      // отрицательный ответ Ядру
                }
            }
            responseString = JsonConvert.SerializeObject(QAHabroGrabProgram.grab_response);
            response.ContentType = "content-type: application/json";
            buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            log.Debug(String.Format("Send answer on POST request: {0}", responseString));
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
                log.Debug(String.Format("Body: {0}", clearRequestJson));
            }
            inputStream.Close();
            reader.Close();
            return clearRequestJson;
        }

        // Останавливаем работу сервера
        ~GrabServer()
        { if (listener != null) listener.Stop(); } // если есть живой слушатель

    }  // end class GrabServer
}
