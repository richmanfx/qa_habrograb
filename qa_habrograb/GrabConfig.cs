namespace qa_habrograb
{
    /// Класс для конфигурационных данных
    public class GrabConfig
    {
        public Grabber grabber { get; set; }                // Данные Грабера 
        public CoreServer core_server { get; set; }         // Данные Ядра
    }

    /// Класс для конфигурационных данных Грабера
    public class Grabber
    {
        public int port { get; set; }                       // TCP порт Грабера для команд
        public string browser { get; set; }                 // Браузер - phantomjs или chrome
        public string browser_size { get; set; }            // Размер окна браузера, например 1800x800
        public int requests_queue_size { get; set; }         // Размер очереди запросов на грабинг
    }

    /// Класс для конфигурационных данных Сервера
    public class CoreServer
    {
        public string domain_name { get; set; }             // Доменное имя Ядра
        public string address { get; set; }                 // IP адрес Ядра
        public string port { get; set; }                    // TCP порт Ядра для отправки статей
    }
}
