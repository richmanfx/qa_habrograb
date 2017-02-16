namespace qa_habrograb
{
    /// Класс ответа Грабера на GET запрос проверки работоспособности (/ping)
    public class PingResponse
    {
        public bool result { get; set; }        // "true" при готовности грабера к работе
        public Error error { get; set; }        // описывает проблему при неготовности грабера к работе
    }

    /// Информация о внутреннем исключении
    public class Inner
    {
        public string className { get; set; }       // Класс исключения
        public string message { get; set; }         // Сообщение исключения
        public string stackTrace { get; set; }      // Стек вызовов исключения
    }

    public class Exception
    {
        public string className { get; set; }       // Класс исключения
        public string message { get; set; }         // Сообщение исключения
        public string stackTrace { get; set; }      // Стек вызовов исключения
        public Inner inner { get; set; }            // Аналогичная информация о внутреннем исключении
    }

    public class Error
    {
        public string text { get; set; }            // Русскоязычное понятное и осмысленное описание ошибки
        public string time { get; set; }            // Дата и время регистрации (отлова) ошибки
        public Exception exception { get; set; }    // Информация об исключении, вызвавшем эту ошибку
    }
}
