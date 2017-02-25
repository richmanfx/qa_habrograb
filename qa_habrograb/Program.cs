using System;
using System.Text;
using log4net;
using log4net.Config;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System.Collections.Generic;
using System.Collections;



namespace qa_habrograb
{
    class QAHabroGrabProgram
    {
        // [STAThread]          // Однопоточное приложение

        private static readonly ILog log = LogManager.GetLogger(typeof(QAHabroGrabProgram));        // Логер
        public static PingResponse ping_response = new PingResponse(true);                          // Ответ на /ping
        public static GrabResponse grab_response = new GrabResponse(true);                          // Ответ на /grab
        public static RequestsQueue rq;                                                             // Очередь запросов на граббинг
        

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;           // Цвет текста в консоли
            Console.OutputEncoding = Encoding.UTF8;                 // Кодировка в консоли
            XmlConfigurator.Configure();                            // Конфигурация логера в XML-файле
            string config_file_name = "qa_habrograb_config.json";   // Имя файла конфигурации

            string search_query = "selenium";                       // Поисковые фразы для скрапинга
            string logo_base64 = "pictures/logo_habr.base64";       // Логотип источника новостей в base64


            // Считать настройки из файла конфигурации
            GrabConfig config = new GrabConfig();
            config = GetSettingsFromConfigFile(config, config_file_name, log);
            if (config == null)
            {
                log.Error(String.Format("Failed to load configuration settings from {0}.", config_file_name));
                Environment.Exit(1);
            }

            // Создать очередь запросов на граббинг
            log.Debug("Create the requests queue.");
            rq = new RequestsQueue(config.grabber.requests_queue_size);

            // Принимать команды от Ядра в отдельном потоке
            Thread CommandReceiverThread = new Thread(delegate () { CommandReceiver(config); });
            CommandReceiverThread.IsBackground = true;
            CommandReceiverThread.Name = "Command Receiver Thread";
            CommandReceiverThread.Start();


            // Опрос очереди и запуск грабберов
            for (;;)
            {
                GrabResults grab_result = QueuePolling(config);
                Thread.Sleep(10000);
            }

            // Разместить результат в очереди результатов
            //


            // Окончание работы - для анализа сообщений в консоли
            log.Debug("Работа закончена. Нажми 'Enter'.");
            Console.Read();
            Environment.Exit(0);
        } // end Main


        /// Принимать команды от Ядра
        private static void CommandReceiver(GrabConfig config)
        {
            log.Debug(String.Format("Start accepting commands server on port '{0}'.", config.grabber.port));
            new GrabServer(Convert.ToInt32(config.grabber.port));
        }


        /// Считывает файл конфигурации. Если файла нет, то возвращает null.
        private static GrabConfig GetSettingsFromConfigFile(GrabConfig config, string file_name, ILog log)
        {
            if (File.Exists(file_name))      // Существует ли JSON файл конфигурации
            {
                log.Debug(String.Format("Read parameters from a config file '{0}'.", file_name));
                // Считать JSON строку из файла
                string json_string = File.ReadAllText(file_name, Encoding.UTF8);
                // Десериализация
                config = JsonConvert.DeserializeObject<GrabConfig>(json_string);
                return config;
            }
            else
            {
                log.Error(String.Format("Config file '{0}' not found.", file_name));
                return null;
            }
        }

        /// Опрос очереди запросов и запуск грабберов
        private static GrabResults QueuePolling(GrabConfig config)
        {
            GrabRequest gr_from_queue;

            gr_from_queue = rq.GetRequest();        // Получить запрос из очереди

            if (gr_from_queue == null)          // Если нет запросов в очереди
            {
                return null;
            }

            // Запустить грабер 
            GrabberCore gc = new GrabberCore(gr_from_queue, config);
            return gc.Grabbing();

            
        }



    } // end class QAHabroGrabProgram

} // end namespace qa_habrograb
