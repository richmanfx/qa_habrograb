using System.Collections.Generic;

namespace qa_habrograb
{
    /// Запрос на сбор новостей
    public class GrabRequest
    {
        public int requestId { get; set; }      // Уникальный идентификатор запроса, который используется 
                                                // как связь между запросом от ядра ко грабберу и итоговым запросом 
                                                // от граббера к ядру с результатами поиска новостей.

        public int version { get; set; }            // Версия протокола
        public List<string> queries { get; set; }   // Массив поисковых запросов на целевой ресурс, по каждому из 
                                                    // которых граббер должен отработать отдельно.
        public Period period { get; set; }          // Период, за который искать статьи
    }

    public class Period
    {
        public string fromDate { get; set; }        // Дата начала периода, за который искать статьи
        public string toDate { get; set; }          // Дата окончания периода, за который искать статьи
    }
}

