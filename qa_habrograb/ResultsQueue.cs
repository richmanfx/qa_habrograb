using log4net;
using System;
using System.Collections.Generic;
using System.Threading;

namespace qa_habrograb
{
    class ResultsQueue
    {
        public ResultsQueue (int queue_size)
        {
            // Создать очередь результатов грабинга
            results_queue = new Queue<GrabResultsRequest>(queue_size);
            this.QueueSize = queue_size;
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(QAHabroGrabProgram));        // Логер
        private int QueueSize { get; set; }                     // Максимальный размер очереди
        Queue<GrabResultsRequest> results_queue;                // Очередь результатов
        string Result;

        /// Добавляет в очередь результат грабинга
        public string AddResult(GrabResultsRequest grr)
        {
            Result = "";
            if (results_queue.Count >= QueueSize)
            {
                // Очередь переполнена
                Result = String.Format("{1}: Результат не добавлен - очередь переполнена, уже {0} результатов.",
                                       QueueSize,
                                       Thread.CurrentThread.Name);
                log.Debug(Result);
            }
            else
            {
                // Добавление результата в очередь 
                results_queue.Enqueue(grr);
                log.Debug(String.Format("{1}: Добавлен результат в очередь. Результатов в очереди: {0} .", 
                                        results_queue.Count, 
                                        Thread.CurrentThread.Name));
                Result = "OK";
            }
            return Result;
        }
    }
}
