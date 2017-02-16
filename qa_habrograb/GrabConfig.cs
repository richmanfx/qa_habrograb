﻿namespace qa_habrograb
{
    /// Класс для конфигурационных данных
    public class GrabConfig
    {
        public Grabber grabber { get; set; }
        public CoreServer core_server { get; set; }
    }

    /// Класс для конфигурационных данных Грабера
    public class Grabber
    {
        public string port { get; set; }
        public string browser { get; set; }
        public string browser_size { get; set; }
    }

    /// Класс для конфигурационных данных Сервера
    public class CoreServer
    {
        public string domain_name { get; set; }
        public string address { get; set; }
        public string port { get; set; }
    }
}
