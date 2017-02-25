using log4net;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qa_habrograb
{
    /// Грабит заданный сайт по заданной строке поиска
    public class GrabberCore
    {
        public GrabberCore(GrabRequest gr, GrabConfig config)
        {
            this.Config = config;
            this.GrabbingRequest = gr;

        }

        private static readonly ILog log = LogManager.GetLogger(typeof(QAHabroGrabProgram));    // Логер
        GrabRequest GrabbingRequest;
        
        GrabConfig Config;
        string site_page = "https://habrahabr.ru";      // Ссылка на страницу сайта для скрапинга
        
       


        public GrabResults Grabbing()
        {

            log.Debug(String.Format("Begin grabbing, equestId = {0}.", GrabbingRequest.RequestId));
            string StartTime = DateTime.Now.ToString("s");

            RemoteWebDriver driver = null;
            driver = InitBrowser(Config, driver);

            log.Debug("Переходим на сайт.");
            driver.Navigate().GoToUrl(site_page);
            

            log.Debug("Кликаем на 'Поиск'");
            driver.FindElementByXPath("//button[@id='search-form-btn']").Click();

            string search_query = "";       // Поисковый запрос
            foreach (var query in GrabbingRequest.Queries)
            {
                search_query += query;
                if (query != GrabbingRequest.Queries.Last())
                {
                    search_query += " ";
                }
            }


            log.Debug("Вводим поисковый запрос: '" + search_query + "'.");
            driver.FindElementById("search-form-field").SendKeys(search_query);

            log.Debug("Нажимаем 'Enter'.");
            driver.FindElementById("search-form-field").Submit();
            log.Debug("Инфо с сайта: " + driver.Title);

            // log.Debug("Нажимаем закладку 'По времени'.");

            string EndTime = DateTime.Now.ToString("s");

            PostInfo post_info = new PostInfo(site_page, "ru", "Заголовок");
            GrabResults GrabbingResult = new GrabResults(search_query, post_info, new ProcessingInfo(StartTime, EndTime));

            driver.Close();

            return GrabbingResult;
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
    }
}
