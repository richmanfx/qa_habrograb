namespace qa_habrograb
{
    /// Метрики и другие характеристики процесса поиска
    public class ProcessingInfo
    {   
        public ProcessingInfo(string startTime, string FinishTime)
        {
            this.StartTime = startTime;
            this.FinishTime = FinishTime;
        }

        // Дата и время в стандартном формате JS.
        public string StartTime { get; set; }   // Дата и Время начала обработки конкретного поискового запроса.
        public string FinishTime { get; set; }  // Дата и Время завершения обработки конкретного поискового запроса.
    }
}
