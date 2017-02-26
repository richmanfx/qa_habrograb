using log4net;
using System;
using System.Collections.Generic;
using System.Threading;

namespace qa_habrograb
{
    class RequestsQueue
    {
        public RequestsQueue(int queue_size)
        {
            // Создать очередь запросов заданий от Ядра
            request_queue = new Queue<GrabRequest>(queue_size);
            this.QueueSize = queue_size;
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(QAHabroGrabProgram));        // Логер
        private int QueueSize { get; set; }                                                         // Максимальный размер очереди
        Queue<GrabRequest> request_queue;                                                           // Очередь запросов заданий от ядра
        string Result;


        // Добавляет в очередь запрос на грабинг
        public string AddRequest(GrabRequest gr)
        {
            Result = "";
            if (request_queue.Count >= QueueSize)
            {
                // Очередь переполнена
                Result = String.Format("{1}: Запрос не добавлен - очередь переполнена, уже {0} запросов.", QueueSize, Thread.CurrentThread.Name);
                log.Debug(Result);
                setNegativeGrabberState(Result);
            }
            else
            {
                // Проверить на повтор equestId запроса в очереди
                foreach (var old_gr in request_queue)
                {
                    if(gr.RequestId == old_gr.RequestId)
                    {
                        Result = String.Format("{1}: Запрос не добавлен - запрос c equestId = {0} уже существует в очереди.", gr.RequestId, Thread.CurrentThread.Name);
                        log.Debug(Result);
                        setNegativeGrabberState(Result);
                    }
                }
                if (Result == "")
                {
                    // Добавление в очередь запросов
                    request_queue.Enqueue(gr);
                    log.Debug(String.Format("{1}: Добавлен запрос в очередь. Запросов в очереди: {0} .", request_queue.Count, Thread.CurrentThread.Name));
                    Result = "OK";
                }
            }
            return this.Result;
        }

        /// Изменить состояние грабера на негативный
        private void setNegativeGrabberState(string result)
        {
            QAHabroGrabProgram.ping_response.Result = false;
            QAHabroGrabProgram.ping_response.error.Text = result;
            QAHabroGrabProgram.ping_response.error.Time = DateTime.Now.ToString("s");
            QAHabroGrabProgram.grab_response.Error.Exception.StackTrace.Clear();
            QAHabroGrabProgram.grab_response.Error.Exception.Message = "";
            QAHabroGrabProgram.grab_response.Error.Exception.ClassName = "";
        }

        /// Забирает из очереди запрос на грабинг
        public GrabRequest GetRequest() {
            var Result = new GrabRequest();
            if (request_queue.Count == 0)
            {
                log.Debug(String.Format("{0}: Очередь запросов заданий пуста.", Thread.CurrentThread.Name));
                Result = null;
            }
            else
            {
                Result = request_queue.Dequeue();
                log.Debug(String.Format("{1}: Изъят запрос из очереди. Осталось запросов в очереди: {0} .", request_queue.Count, Thread.CurrentThread.Name));
            }
            return Result;
        }
    }
}
