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



namespace qa_habrograb
{
    class QAHabroGrabProgram
    {
        // Логер
        private static readonly ILog log = LogManager.GetLogger(typeof(QAHabroGrabProgram));

        // Однопоточное приложение
        // [STAThread]

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;           // Цвет текста в консоли
            Console.OutputEncoding = Encoding.UTF8;                 // Кодировка в консоли
            XmlConfigurator.Configure();                            // Конфигурация логера в XML-файле
            string config_file_name = "qa_habrograb_config.json";   // Имя файла конфигурации
            string site_page = "https://habrahabr.ru";
            string search_query = "selenium";

            // Считать настройки из файла конфигурации
            GrabberConfig config = new GrabberConfig();
            config = GetSettingsFromConfigFile(config, config_file_name, log);
            if (config == null)
            {
                log.Error(String.Format("Failed to load configuration settings from {0}.", config_file_name));
                Environment.Exit(1);
            }

            /*  // Доступ к параметрам конфигурации
            log.Info(config.core_server.domain_name);
            log.Info(config.core_server.address);
            log.Info(config.core_server.port);
            log.Info(config.grabber.port);
            */

            /*
            // Слушать команды от сервера
            log.Debug(String.Format("Start accepting commands server on port '{0}'.", config.grabber.port));
            new GrabServer(Convert.ToInt32(config.grabber.port));
            */

            // Грабить habrahabr.ru
            log.Debug("Begin grabbing...");
            PhantomJSDriver driver = new PhantomJSDriver();
            // ChromeDriver driver = new ChromeDriver("web_drivers");
            driver.Navigate().GoToUrl(site_page);
            log.Debug("Инфо с сайта: " + driver.Title);

            log.Debug("Кликаем на 'Поиск'");
            driver.FindElementByXPath("//button[@id='search-form-btn']").Click();

            log.Debug("Вводим поисковый запрос: '" + search_query + "'.");
            driver.FindElementById("search-form-field").SendKeys(search_query);   //  "//input[@id='search-form-field']"

            log.Debug("Нажимаем 'Enter'.");
            driver.FindElementById("search-form-field").Submit();
            log.Debug("Инфо с сайта: " + driver.Title);



            



            Console.Read();
            driver.Close();
            Environment.Exit(0);
        } // end Main


        /// Считывает файл конфигурации. Если файла нет, то возвращает null.
        private static GrabberConfig GetSettingsFromConfigFile(GrabberConfig config, string file_name, ILog log)
        {
            if (File.Exists(file_name))      // Существует ли JSON файл конфигурации
            {
                log.Debug(String.Format("Read parameters from a config file '{0}'.", file_name));
                // Считать JSON строку из файла
                string json_string = File.ReadAllText(file_name, Encoding.UTF8);
                // Десериализация
                config = JsonConvert.DeserializeObject<GrabberConfig>(json_string);
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
