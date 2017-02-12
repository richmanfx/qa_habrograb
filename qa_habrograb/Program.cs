using System;
using System.Text;
using log4net;
using log4net.Config;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Threading;


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
            string site_page = "https://m.habrahabr.ru";

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

            // Слушать команды от сервера
            log.Debug(String.Format("Start accepting commands server on port '{0}'.", config.grabber.port));
            new GrabServer(Convert.ToInt32(config.grabber.port));


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
