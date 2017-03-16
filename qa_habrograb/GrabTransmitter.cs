using log4net;
using System.Net;
using System;
using System.Threading;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace qa_habrograb
{
    /// HTTP передатчик результатов грабинга Ядру
    class GrabTransmitter
    {
        private static ILog log = LogManager.GetLogger(typeof(QAHabroGrabProgram));        // Логер
        private GrabConfig Config;          // Конфиг Грабера
        private GrabResultsRequest GRR;     // Результат грабинга для отсылки Ядру

        public GrabTransmitter(GrabConfig config, GrabResultsRequest grr_from_result_queue)
        {
            this.Config = config;
            this.GRR = grr_from_result_queue;       // Результат грабинга для Ядра
            string Url = GetURL();                  // URL сервера ядра

            // Запрос Ядру
            HttpWebRequest RequestToCore = (HttpWebRequest)HttpWebRequest.Create(Url);
            RequestToCore.Method = "POST";
            RequestToCore.ContentType = "content-type: application/json";
            UTF8Encoding encoding = new UTF8Encoding();
            string PostedString = JsonConvert.SerializeObject(grr_from_result_queue);      // Данные в JSON
            byte[] bytes = encoding.GetBytes(PostedString);
            RequestToCore.ContentLength = bytes.Length;

            // Сформировать тело запроса
            try
            {
                using (Stream requestStream = RequestToCore.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, encoding.GetByteCount(PostedString));
                }
            }
            catch (WebException)
            {
                log.Debug(String.Format("{0}: Ошибка соединения с сервером-Ядром.", Thread.CurrentThread.Name));
            }

            // Отправить запрос Ядру и получить ответ
            log.Debug(String.Format("{0}: Отправка результата \"GrabResultsRequest\" Ядру.", Thread.CurrentThread.Name));
            HttpWebResponse ResponseFromCore = null;
            try
            {
                ResponseFromCore = (HttpWebResponse)RequestToCore.GetResponse();
            }
            catch (Exception)
            {
                log.Debug(String.Format("{0}: Не удалась отправка результата Ядру.", Thread.CurrentThread.Name));

            }

            if (ResponseFromCore != null)
            {
                string DataFromCore;        // Ответ от Ядра
                using (StreamReader reader = new StreamReader(ResponseFromCore.GetResponseStream(), Encoding.UTF8))
                {
                    DataFromCore = reader.ReadToEnd();
                }
                log.Debug(String.Format("{0}: От Ядра получен \"GrabResultsResponse\": {1}",
                                        Thread.CurrentThread.Name,
                                        DataFromCore));

                // Анализ ответа 'GrabResultsResponse' от ядра
                // Десериализовать данные ответа в объект GrabResultsResponse
                GrabResultsResponse GrabResResp = new GrabResultsResponse();
                try
                {
                    GrabResResp = JsonConvert.DeserializeObject<GrabResultsResponse>(DataFromCore);
                }
                catch (Exception ex)
                {
                    log.Debug(String.Format("{0}: Ответ \"GrabResultsResponse\" от Ядра имеет неверную структуру:\n{1}",
                                            Thread.CurrentThread.Name,
                                            ex));
                }

                // Если отправленный результат принят
                if (GrabResResp.Result)
                {
                    log.Debug(String.Format("{0}: Результат \"GrabResultsRequest\" принят Ядром",
                                        Thread.CurrentThread.Name));

                    // Удалить 'GrabResultsRequest' из выходной очереди
                    QAHabroGrabProgram.res_q.DeleteResult();
                }
                else
                {
                    log.Debug(String.Format("{0}: Результат \"GrabResultsRequest\" Ядром НЕ принят:\n" +
                                            "Error.Time: {1}\n" +
                                            "Error.Text: {2}\n" +
                                            "Error.Exception.Message: {3}\n" +
                                            "Error.Exception.ClassName: {4}\n" +
                                            "StackTrace: {5}\n",
                                        Thread.CurrentThread.Name,
                                        GrabResResp.Error.Time,
                                        GrabResResp.Error.Text,
                                        GrabResResp.Error.Exception.Message,
                                        GrabResResp.Error.Exception.ClassName,
                                        GrabResResp.Error.Exception.StackTrace));
                }
            }
        }
        

        /// Сборка URL сервера ядра
        private string GetURL()
        {
            string Url = "";
            try
            {
                if (Config.core_server.domain_name != null)
                {
                    log.Debug(String.Format("{0}: Результат Ядру будет отправлен на \"{1}\".",
                                            Thread.CurrentThread.Name,
                                            Config.core_server.domain_name));
                    Url = String.Format("http://{0}:{1}", Config.core_server.domain_name, Config.core_server.port);
                }
                else
                {
                    log.Debug(String.Format("{0}: Результат Ядру будет отправле по IP {1} .",
                                            Thread.CurrentThread.Name,
                                            Config.core_server.address));
                    Url = String.Format("http://{0}:{1}", Config.core_server.address, Config.core_server.port);
                }
            }
            catch (Exception ex)
            {
                log.Debug(String.Format("{0}: Не удалось получить URL сервера Ядра: {1}", Thread.CurrentThread.Name, ex));
                // TODO: Изменить состояние Грабера и вписать в это состояние Эксцепшн
            }

            return Url;
        }

    } // End class GrabTransmitter
}
