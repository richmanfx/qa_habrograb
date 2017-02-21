using System.Collections.Generic;

namespace qa_habrograb
{
    /// Запрос на сбор новостей от Ядра к Граберу (/grab)
    public class GrabRequest
    {
        public GrabRequest() {}

        public GrabRequest(int requestId, int version, List<string> queries, Period period)
        {
            this.RequestId = requestId;
            this.Version = version;
            this.Queries = queries;
            this.Period = period;
        }

        public int RequestId { get; set; }      // Уникальный идентификатор запроса, который используется
                                                // как связь между запросом от ядра ко грабберу и итоговым запросом 
                                                // от граббера к ядру с результатами поиска новостей.

        public int Version { get; set; }            // Версия протокола.

        public List<string> Queries { get; set; }   // Массив поисковых запросов на целевой ресурс, по каждому из 
                                                    // которых граббер должен отработать отдельно.

        public Period Period { get; set; }          // Период, за который искать статьи.
    }
}
