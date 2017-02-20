namespace qa_habrograb
{
    /// Информация об ошибке
    public class ErrorInfo
    {
        public ErrorInfo(string text)
        {
            this.Text = text;
            Exception = new ExceptionInfo("", null);
        }

        public string Text { get; set; }                // Русскоязычное понятное и осмысленное описание ошибки.
        public string Time { get; set; }                // Дата и Время в стандартном формате JS.
        public ExceptionInfo Exception { get; set; }    // Информация об исключении, вызвавшем эту ошибку.
    }
}
