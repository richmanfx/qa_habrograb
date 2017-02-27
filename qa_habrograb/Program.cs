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
        public static string grab_version = "1.0.0";                                                // Версия программы
        public static string habr_logo_file_name = "logo_habr.base64";                              // Логотип источника новостей в base64


        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;           // Цвет текста в консоли
            Console.OutputEncoding = Encoding.UTF8;                 // Кодировка в консоли
            XmlConfigurator.Configure();                            // Конфигурация логера в XML-файле
            string config_file_name = "qa_habrograb_config.json";   // Имя файла конфигурации

        // Считать настройки из файла конфигурации
        GrabConfig Config = new GrabConfig();
            Config = GetSettingsFromConfigFile(Config, config_file_name, log);
            if (Config == null)
            {
                log.Error(String.Format("Failed to load configuration settings from {0}.", config_file_name));
                Environment.Exit(1);
            }

            

            // Создать очередь запросов на граббинг
            log.Debug("Create the requests queue.");
            rq = new RequestsQueue(Config.grabber.requests_queue_size);

            // Принимать команды от Ядра в отдельном потоке
            Thread CommandReceiverThread = new Thread(delegate () { CommandReceiver(Config); });
            CommandReceiverThread.IsBackground = false;     // Основной поток
            CommandReceiverThread.Name = "Command Receiver Thread";
            CommandReceiverThread.Start();

            // Опрос очереди и запуск грабберов в отдельном потоке
            Thread GrabberRunnerThread = new Thread(delegate () { GrabberRunner(Config); });
            GrabberRunnerThread.IsBackground = true;        // Фоновый поток
            GrabberRunnerThread.Name = "Grabber Runner Thread";
            GrabberRunnerThread.Start();



            // Опрос очереди результатов и отправка результатов Ядру в отдельном потоке
            // TODO: !!!!!



        } // end Main


        /// Опрос очереди и запуск грабберов
        private static void GrabberRunner(GrabConfig config)
        {
            for (;;)
            {
                int polling_frequency = 10;         // Время в секундах между опросом очереди запросов на грабинг
                GrabResultsRequest GRR = QueuePolling(config);
                if (GRR != null)
                {
                    // Разместить GRR в очереди результатов          TODO: !!!!!
                    
                }

                Thread.Sleep(polling_frequency * 1000);
            }
        }


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
                log.Debug(String.Format("Read parameters from a Config file '{0}'.", file_name));
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
        private static GrabResultsRequest QueuePolling(GrabConfig config)
        {
            GrabRequest gr_from_queue;

            gr_from_queue = rq.GetRequest();        // Получить запрос из очереди

            if (gr_from_queue == null)              // Если нет запросов в очереди
            {
                return null;
            }

            // Запустить грабер 
            log.Debug(String.Format("{0}: Запускаем Грабер.", Thread.CurrentThread.Name));
            GrabberCore gc = new GrabberCore(gr_from_queue, config);
            GrabResultsRequest GrResReq = gc.Grabbing();

            return GrResReq;
        }



    } // end class QAHabroGrabProgram

} // end namespace qa_habrograb
