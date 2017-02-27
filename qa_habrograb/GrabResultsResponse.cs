namespace qa_habrograb
{
    /// Ответ от Ядра к Граберу после слива результатов скрапинга (/results)
    class GrabResultsResponse
    {
        public GrabResultsResponse()
        {
            Error = new ErrorInfo("");
        }

        public bool Result { get; set; }        // true - если результаты успешно прошли валидацию.
        public ErrorInfo Error { get; set; }    // Описание обнаруженных проблем, если результаты не прошли валидацию.
    }
}
