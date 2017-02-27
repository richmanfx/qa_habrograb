using System.Collections.Generic;

namespace qa_habrograb
{
    /// Запрос Грабера к Ядру (/result) для передачи результатов поиска
    public class GrabResultsRequest
    {
        public GrabResultsRequest(int requestId, int version, GrabberInfo grabber, List<GrabResults> results)
        {
            this.RequestId = requestId;
            this.Version = version;
            this.Grabber = grabber;
            this.Results = results;
        }

        public int RequestId { get; set; }              // Идентификатор, который прислан Ядром Грабберу.
        public int Version { get; set; }                // Версия протокола, в которой граббер составлял сообщение.
        public GrabberInfo Grabber { get; set; }        // Описание граббера.
        public List<GrabResults> Results { get; set; }  // Результаты поиска по указанным Ядром ключевым словам.

        
    }
}
