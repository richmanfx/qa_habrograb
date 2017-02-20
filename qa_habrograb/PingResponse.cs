namespace qa_habrograb
{
    /// Класс ответа Грабера на GET запрос /ping для проверки работоспособности
    public class PingResponse
    {
        public PingResponse(bool result)
        {
            this.Result = result;
            error = new ErrorInfo("");
        }

        public bool Result { get; set; }            // "true" при готовности грабера к работе.
        public ErrorInfo error { get; set; }        // Описывает проблему при неготовности грабера к работе.
    }
}
