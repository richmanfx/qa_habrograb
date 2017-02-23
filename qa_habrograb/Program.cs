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
        // Однопоточное приложение
        // [STAThread]

        // Логер
        private static readonly ILog log = LogManager.GetLogger(typeof(QAHabroGrabProgram));

        // Создать начальный ответ на /ping
        public static PingResponse ping_response = new PingResponse(true);

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;           // Цвет текста в консоли
            Console.OutputEncoding = Encoding.UTF8;                 // Кодировка в консоли
            XmlConfigurator.Configure();                            // Конфигурация логера в XML-файле
            string config_file_name = "qa_habrograb_config.json";   // Имя файла конфигурации
            string site_page = "https://habrahabr.ru";              // Ссылка на страницу сайта для скрапинга
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
            RequestsQueue rq = new RequestsQueue(config.grabber.requests_queue_size);

            



            // Слушать команды от сервера
            log.Debug(String.Format("Start accepting commands server on port '{0}'.", config.grabber.port));
            new GrabServer(Convert.ToInt32(config.grabber.port));

            
            

            // Запустить скрапинг сайта
            // GrabberCore(site_page, search_query, config);

            // Окончание работы - для анализа сообщений в консоли
            log.Debug("Работа закончена. Нажми 'Enter'.");
            Console.Read();
            Environment.Exit(0);
        } // end Main


        // Грабит заданный сайт по заданной строке поиска
        private static void GrabberCore(string site_page, string search_query, GrabConfig config)
        {
            
            log.Debug("Begin grabbing...");

            RemoteWebDriver driver = null;
            driver = InitBrowser(config, driver);

            driver.Navigate().GoToUrl(site_page);
            log.Debug("Инфо с сайта: " + driver.Title);

            log.Debug("Кликаем на 'Поиск'");
            driver.FindElementByXPath("//button[@id='search-form-btn']").Click();

            log.Debug("Вводим поисковый запрос: '" + search_query + "'.");
            driver.FindElementById("search-form-field").SendKeys(search_query);

            log.Debug("Нажимаем 'Enter'.");
            driver.FindElementById("search-form-field").Submit();
            log.Debug("Инфо с сайта: " + driver.Title);

            driver.Close();
            
        }


        /// Инициализирует WebDriver для браузера, указанного в конфигурационном файле 
        private static RemoteWebDriver InitBrowser(GrabConfig config, RemoteWebDriver driver)
        {
            if (config.grabber.browser == "phantomjs")
                driver = new PhantomJSDriver();
            else if (config.grabber.browser == "chrome")
            {
                driver = new ChromeDriver("web_drivers");
                if (config.grabber.browser_size != null)
                {
                    // Выставить размер окна браузера
                    log.Debug("Выставляем размер окна браузера");
                    Char delimiter = 'x';
                    int width = Convert.ToInt32(config.grabber.browser_size.Split(delimiter)[0]);
                    int height = Convert.ToInt32(config.grabber.browser_size.Split(delimiter)[1]);
                    driver.Manage().Window.Size = new System.Drawing.Size(width, height);
                }
                // Если размера нет в конфигурационном файле, то максимальный размер окна браузера
                if (config.grabber.browser_size == null)
                    log.Debug("Выставляем максимальный размер окна браузера");
                    driver.Manage().Window.Maximize();
            }
            else
            {
                log.Error(String.Format("Browser not specified in config file '{0}'", config));
                Console.Read();
                Environment.Exit(1);
            }
            return driver;
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



    } // end class QAHabroGrabProgram

} // end namespace qa_habrograb
