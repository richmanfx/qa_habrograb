using log4net;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace qa_habrograb
{

    /// TCP сервер для приёма команд
    public class GrabServer
    {
        private static ILog log = LogManager.GetLogger(typeof(QAHabroGrabProgram));        // Логер

        TcpListener Listener;           // Слушатель сокета

        /// Конструктор - Запускатель сервера
        /// <param name="Port"></param>
        public GrabServer(int port)
        {
            Listener = new TcpListener(IPAddress.Any, port);        // Слушать на указанном порту
            Listener.Start();       // Начать слушать

            // Принимать новые соединения от клиентов
            for (;;)  // бесконечно
            {
                TcpClient Client = Listener.AcceptTcpClient();                              // Принимать нового клиента
                Thread Thread = new Thread(new ParameterizedThreadStart(ClientThread));     // Создать поток
                Thread.Start(Client);                                                       // Запустить этот поток, передавая ему принятого клиента
                log.Debug(String.Format("{0}: Принят запрос: {1}", Thread.CurrentThread.Name, Client.ToString()));


                // HttpListenerContext context = Listener.GetContext();    //Ожидание входящего запроса
                // HttpListenerRequest request = context.Request;          //Объект запроса
                // HttpListenerResponse response = context.Response;       //Объект ответа

                // log.Debug(String.Format("{2}: {0} request is received: {1}", request.HttpMethod, request.Url, Thread.CurrentThread.Name));

                // Обработать входящий запрос
                // SwitchingRequests(request, response);

                // Послать ответ
                // using (Stream stream = response.OutputStream) { }
            }
        } // end конструктора GrabServer


        /// Запускатель клиента
        static void ClientThread(Object StateInfo)
        {
            // Просто создаем новый экземпляр класса Client и передаем ему приведенный к классу TcpClient объект StateInfo
            new Client((TcpClient)StateInfo);
        }

        // Обработчик клиента
        class Client
        {
            // Конструктор. Принимает клиента от TcpListener
            public Client(TcpClient IncomingClient)
            {
                string Request = "";                                // Запрос клиента
                byte[] Buffer = new byte[1024];                     // Буфер для принятых данных
                int Count;                                          // Количеств байт, принятых от клиента

                // Читать из потока клиента, пока от него поступают данные
                while ((Count = IncomingClient.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
                {
                    Request += Encoding.ASCII.GetString(Buffer, 0, Count);          // Преобразовать данные в строку и добавить к Request
                    if (Request.IndexOf("\r\n\r\n") >= 0)  // Запрос должен заканчиваться последовательностью \r\n\r\n
                    {
                        log.Debug(String.Format("{0}: Запрос: {1}", Thread.CurrentThread.Name, Request.Replace("\r\n", ", ")));
                        break;
                    }
                }

                // Обработать входящий запрос
                SwitchingRequests(Request, IncomingClient);
            }



            /// Свитч запросов от сервера-ядра
            private void SwitchingRequests(string Request, TcpClient IncomingClient)
            {
                string Method = Request.Split(' ')[0];

                switch (Method)
                {
                    case "GET":
                        HandlingGET(Request, IncomingClient);
                        break;
                        /*
                    case "POST":
                        HandlingPOST(Request);
                        break;
                        */
                    case "DELETE":
                        HandlingDELETE(Request, IncomingClient);
                        break;
                    default:
                        HandlingBadRequest(Request, IncomingClient);
                        break;
                }
            } // end SwitchingRequests

            /// Обработчик неверного типа запроса
            private void HandlingBadRequest(string request, TcpClient IncomingClient)
            {
                UTF8Encoding encoding = new UTF8Encoding();
                string responseString = "HTTP/1.1 405 Not Allowed \nContent-Type: " +
                                        "\nContent-Length: \n\n";
                byte[] responseBuffer = encoding.GetBytes(responseString);
                log.Debug(String.Format("{1}: Send answer on bad request: {0}", responseString, Thread.CurrentThread.Name));
                IncomingClient.GetStream().Write(responseBuffer, 0, responseBuffer.Length);
                IncomingClient.Close();
            }

            /// Обработчик DELETE запроса для закрытия приложения
            private void HandlingDELETE(string request, TcpClient IncomingClient)
            {

                PositiveAnswer("Bye!");     // положительный ответ Ядру
                string responseString = JsonConvert.SerializeObject(QAHabroGrabProgram.grab_response);
                string contentType = "content-type: application/json";
                UTF8Encoding encoding = new UTF8Encoding();
                string headers = "HTTP/1.1 200 OK\nContent-Type: " + contentType + "\nContent-Length: " + responseString.Length + "\n\n";
                byte[] headerBuffer = encoding.GetBytes(headers);
                byte[] responseBuffer = encoding.GetBytes(responseString);
                log.Debug(String.Format("{1}: Send answer on DELETE request: {0}", responseString, Thread.CurrentThread.Name));
                IncomingClient.GetStream().Write(headerBuffer, 0, headerBuffer.Length);
                IncomingClient.GetStream().Write(responseBuffer, 0, responseBuffer.Length);
                IncomingClient.Close();
                log.Debug("Работа Грабера закончена.");
                Environment.Exit(0);
            }


            /// Обработчик GET запроса от сервера-ядра
            private void HandlingGET(string request, TcpClient IncomingClient)
            {
                // В ответ послать PingResponse
                QAHabroGrabProgram.ping_response.error.Time = DateTime.Now.ToString("s");
                string responseString = JsonConvert.SerializeObject(QAHabroGrabProgram.ping_response);
                string contentType = "content-type: application/json";
                UTF8Encoding encoding = new UTF8Encoding();
                string headers = "HTTP/1.1 200 OK\nContent-Type: " + contentType + "\nContent-Length: " + responseString.Length + "\n\n";
                byte[] headerBuffer = encoding.GetBytes(headers);
                byte[] responseBuffer = encoding.GetBytes(responseString);
                log.Debug(String.Format("{1}: Send answer on GET request: {0}", responseString, Thread.CurrentThread.Name));
                IncomingClient.GetStream().Write(headerBuffer, 0, headerBuffer.Length);
                IncomingClient.GetStream().Write(responseBuffer, 0, responseBuffer.Length);
                IncomingClient.Close();
            }
        }


        /// Обработчик POST запроса от сервера-ядра
        private void HandlingPOST(HttpListenerRequest request, HttpListenerResponse response,
                                  out string responseString, out byte[] buffer)
        {
            response.StatusCode = (int)HttpStatusCode.OK;
            string incomingJson = CleaningRequestData(request);     // Очистка данных запроса
            if (incomingJson == null)
            {
                string negative_response = "Данных в POST запросе нет.";
                log.Debug(String.Format("{0}: {1}", Thread.CurrentThread.Name, negative_response));
                NegativeAnswer(negative_response);      // отрицательный ответ Ядру
            }
            else
            {
                // Десериализовать данные запроса в объект GrabRequest
                GrabRequest gr = new GrabRequest();
                try
                {
                    gr = JsonConvert.DeserializeObject<GrabRequest>(incomingJson);
                }
                catch (Exception ex)
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
                string result = QAHabroGrabProgram.req_q.AddRequest(gr);
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
            log.Debug(String.Format("{1}: Send answer: {0}.", responseString, Thread.CurrentThread.Name));
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        /// Формирует негативный ответ Ядру
        private static void NegativeAnswer(string error_text)
        {
            QAHabroGrabProgram.grab_response.Result = false;
            QAHabroGrabProgram.grab_response.Error.Text = error_text;
            QAHabroGrabProgram.grab_response.Error.Time = DateTime.Now.ToString("s");
        }


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
        { if (Listener != null) Listener.Stop(); } // если есть живой слушатель

    }   // end class GrabServer
}
