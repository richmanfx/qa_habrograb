namespace qa_habrograb
{
    /// Ответ от Грабера к Ядру на запрос /grab для старта сбора новостей
    public class GrabResponse
    {
        public GrabResponse(bool result)
        {
            this.Result = result;
            Error = new ErrorInfo("");
        }

        public bool Result { get; set; }        // true - если граббер взялся за работу.
        public ErrorInfo Error { get; set; }    // Описывает проблему если граббер не смог начать работу 
                                                // (значение Result = false).
    }
}
