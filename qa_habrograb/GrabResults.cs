namespace qa_habrograb
{
    /// Результат поиска по указанным Ядром ключевым словам.
    public class GrabResults
    {
        public GrabResults(string query, PostInfo posts, ProcessingInfo processing)
        {
            this.Query = query;
            this.Posts = posts;
            this.Processing = processing;
        }

        public string Query { get; set; }               // Поисковый запрос, соответствующий указанным результатам.
        public PostInfo Posts { get; set; }             // Описание источника новостей, с которым работает граббер.
        public ErrorInfo Errors { get; set; }           // Описания ошибок, произошедших в процессе обработки этого 
                                                        // поискового запроса.
        public ProcessingInfo Processing { get; set; }  // Метрики и другие характеристики процесса поиска.
    }
}
