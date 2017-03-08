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
            try
            {
                Listener.Start();       // Начать слушать
            } catch (System.Net.Sockets.SocketException) {
                log.Debug(String.Format("{0}: Сокет недоступен. Запускается второй экземпляр программы?", Thread.CurrentThread.Name));
                Environment.Exit(0);
            }
            

            // Принимать новые соединения от клиентов
            for (;;)  // бесконечно
            {
                TcpClient Client = Listener.AcceptTcpClient();                              // Принимать нового клиента
                Thread Thread = new Thread(new ParameterizedThreadStart(ClientThread));     // Создать поток
                Thread.Start(Client);                                                       // Запустить этот поток, передавая ему принятого клиента
                log.Debug(String.Format("{0}: Принят запрос: {1}", Thread.CurrentThread.Name, Client.ToString()));
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
                        HandlingGET(IncomingClient);
                        break;
                    case "POST":
                        HandlingPOST(Request, IncomingClient);
                        break;
                    case "DELETE":
                        HandlingDELETE(IncomingClient);
                        break;
                    default:
                        HandlingBadRequest(IncomingClient);
                        break;
                }
            } // end SwitchingRequests


            /// Обработчик неверного типа запроса
            private void HandlingBadRequest(TcpClient IncomingClient)
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
            private void HandlingDELETE(TcpClient IncomingClient)
            {
                PositiveAnswer("Bye!");     // положительный ответ Ядру
                string responseString = JsonConvert.SerializeObject(QAHabroGrabProgram.grab_response);
                SendResponse(IncomingClient, responseString);
                log.Debug("Работа Грабера закончена.");
                Environment.Exit(0);
            }


            /// Обработчик GET запроса от сервера-ядра
            private void HandlingGET(TcpClient IncomingClient)
            {
                QAHabroGrabProgram.ping_response.error.Time = DateTime.Now.ToString("s");
                // В ответ послать PingResponse
                SendResponse(IncomingClient, JsonConvert.SerializeObject(QAHabroGrabProgram.ping_response));
            }


            /// Обработчик POST запроса от сервера-ядра
            private void HandlingPOST(string request, TcpClient IncomingClient)
            {
                // Выделить JSON
                string incomingJson = CleaningRequestData(request);

                // Десериализовать данные запроса в объект GrabRequest
                GrabRequest gr = new GrabRequest();
                try
                {
                    gr = JsonConvert.DeserializeObject<GrabRequest>(incomingJson);
                }
                catch (Exception ex)
                {
                    // Заполнить GrabResponse при неверном JSON
                    NegativeAnswer("Ошибка в JSON запросе GrabRequest от Ядра.", ex);

                    // Послать ответ Ядру
                    SendResponse(IncomingClient, JsonConvert.SerializeObject(QAHabroGrabProgram.grab_response));

                    // Вернуться ничего не добавляя в очередь запросов на грабинг
                    return;
                }

                // Добавить запрос на грабинг в очередь
                log.Debug(String.Format("{0}: Добавление /grab запроса GrabRequest от Ядра в очередь.", Thread.CurrentThread.Name));
                string result = QAHabroGrabProgram.req_q.AddRequest(gr);

                if (result == "OK")
                    PositiveAnswer(result);             // положительный ответ Ядру при удачном добавлении в очередь
                else
                    NegativeAnswer(result, null);       // отрицательный ответ Ядру при неудачном добавлении в очередь

                // Послать ответ Ядру
                SendResponse(IncomingClient, JsonConvert.SerializeObject(QAHabroGrabProgram.grab_response));
            }

            /// Отправление ответа на запрос
            private static void SendResponse(TcpClient IncomingClient, string responseString)
            {
                string contentType = "content-type: application/json";
                UTF8Encoding encoding = new UTF8Encoding();
                string headers = "HTTP/1.1 200 OK\nContent-Type: " + contentType + "\nContent-Length: " + responseString.Length + "\n\n";
                byte[] headerBuffer = encoding.GetBytes(headers);
                byte[] responseBuffer = encoding.GetBytes(responseString);
                log.Debug(String.Format("{1}: Send answer on request: {0}", responseString, Thread.CurrentThread.Name));
                IncomingClient.GetStream().Write(headerBuffer, 0, headerBuffer.Length);
                IncomingClient.GetStream().Write(responseBuffer, 0, responseBuffer.Length);
                IncomingClient.Close();
            }

            /// Очистка данных запроса
            private string CleaningRequestData(string request)
            {
                string[] lines = request.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                int counter = lines.Length;
                string clearRequestJson = lines[counter - 1];
                log.Debug(String.Format("{1}: Body: {0}", clearRequestJson, Thread.CurrentThread.Name));
                return clearRequestJson;
            }
        }


        /// Формирование позитивного ответа Ядру
        private static void PositiveAnswer(string text)
        {
            QAHabroGrabProgram.grab_response.Result = true;
            QAHabroGrabProgram.grab_response.Error.Text = text;
            QAHabroGrabProgram.grab_response.Error.Time = DateTime.Now.ToString("s");
            QAHabroGrabProgram.grab_response.Error.Exception.StackTrace.Clear();
            QAHabroGrabProgram.grab_response.Error.Exception.Message = "";
            QAHabroGrabProgram.grab_response.Error.Exception.ClassName = "";
        }


        /// Формирование негативного ответа Ядру
        private static void NegativeAnswer(string error_text, Exception ex)
        {
            QAHabroGrabProgram.grab_response.Result = false;
            QAHabroGrabProgram.grab_response.Error.Text = error_text;
            QAHabroGrabProgram.grab_response.Error.Time = DateTime.Now.ToString("s");

            if (ex != null)
            {
                QAHabroGrabProgram.grab_response.Error.Exception.StackTrace.Add(ex.StackTrace);
                QAHabroGrabProgram.grab_response.Error.Exception.Message = ex.Message;
                QAHabroGrabProgram.grab_response.Error.Exception.ClassName = ex.Source;
            }
        }


        /// Остановка работы сервера
        ~GrabServer()
        { if (Listener != null) Listener.Stop(); } // если есть живой слушатель

    }   // end class GrabServer
}
