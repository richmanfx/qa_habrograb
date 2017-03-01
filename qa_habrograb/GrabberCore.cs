using log4net;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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

        
        /// Грабинг
        public GrabResultsRequest Grabbing()
        {
            log.Debug(String.Format("{1}: Begin grabbing, equestId = {0}.", GrabbingRequest.RequestId, Thread.CurrentThread.Name));
            string StartTime = DateTime.Now.ToString("s");          // Время начала грабинга
            string search_query = BuildingSearchQuery();            // Сборка поискового запроса

            // Требуемые даты статей
            Period p = GrabbingRequest.Period;
            DateTime from_date = ConvertGrabRequestDate(p.FromDate);
            DateTime to_date = ConvertGrabRequestDate(p.ToDate);

            RemoteWebDriver driver = null;
            driver = InitBrowser(Config, driver);

            // Пока не найдём элемент или time_wait секунд (time_wait сек - для всех, до отмены, глобально)
            int time_wait = 4;  // Время ожидания в секундах
            driver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(time_wait * 1000 * 10));  // в сотни наносекунд

            log.Debug(String.Format("{0}: Переходим на сайт.", Thread.CurrentThread.Name));
            driver.Navigate().GoToUrl(site_page);

            log.Debug(String.Format("{0}: Кликаем на 'Поиск'", Thread.CurrentThread.Name));
            driver.FindElementByXPath("//button[@id='search-form-btn']").Click();

            log.Debug(String.Format("{1}: Вводим поисковый запрос: '{0}'.", search_query, Thread.CurrentThread.Name));
            driver.FindElementById("search-form-field").SendKeys(search_query);

            log.Debug(String.Format("{0}: Нажимаем 'Enter'.", Thread.CurrentThread.Name));
            driver.FindElementById("search-form-field").Submit();
            log.Debug(String.Format("{1}: Инфо с сайта: {0}.", driver.Title, Thread.CurrentThread.Name));

            log.Debug(String.Format("{0}: Нажимаем закладку сортировки 'По времени'.", Thread.CurrentThread.Name));
            driver.FindElementByXPath("//div/ul[@class='toggle-menu']/li[a='по времени']").Click();

            // Выбрать пре-статьи в требуемом периоде дат
            List<SmallArticle> SaList = new List<SmallArticle>();       // Коллекция объектов пре-статей
            log.Debug(String.Format("{0}: Выбираем статьи в требуемый период дат.", Thread.CurrentThread.Name));
            GetSmallArticleInTrueDate(from_date, to_date, driver, SaList);

            // Грабить статьи
            GrabResultsRequest GrResReq = ArticleGrabber(driver, GrabbingRequest, search_query, SaList, StartTime);

            driver.Close();     // Браузер закрыть

            return GrResReq;
        }

        /// Сборка поискового запроса
        private string BuildingSearchQuery()
        {
            string search_query = "";
            foreach (var query in GrabbingRequest.Queries)
            {
                search_query += query;
                if (query != GrabbingRequest.Queries.Last())
                {
                    search_query += " ";
                }
            }
            return search_query;
        }

        /// Грабит статьи по ссылкам из списка пре-статей (SmallArticle), помещает в список GrabResults
        private GrabResultsRequest ArticleGrabber(RemoteWebDriver driver, GrabRequest grabbingRequest, string searchQuery,
                                    List<SmallArticle> saList, string StartTime)
        {
            // Информация об авторах
            List<AuthorInfo> AiList = new List<AuthorInfo>();
            // TODO: Заполнять в цикле про нескольких авторов - второму автору вводная  :-)
            AiList.Add(new AuthorInfo(Config.author.author_name, Config.author.author_email));

            List<GrabResults> grabResultsList = new List<GrabResults>();        // Коллекция результатов грабинга

            // Описание источника новостей
            string habr_logo = System.IO.File.ReadAllText(QAHabroGrabProgram.habr_logo_file_name, Encoding.Default);    // Логотип
            SourceInfo Si = new SourceInfo("Хабрахабр - крупнейший в Европе ресурс материалов для IT-специалистов.",
                                           site_page, habr_logo);
            GrabberInfo Gi = new GrabberInfo("Сборщик информации о статьях сайта \"www.habrahabr.ru\" по ключевым фразам.",
                                            QAHabroGrabProgram.grab_version, AiList, Si);
            GrabResultsRequest grr = new GrabResultsRequest(grabbingRequest.RequestId, grabbingRequest.Version, Gi, grabResultsList);

            // Бежим по списку пре-статей
            foreach (SmallArticle small_article in saList)
            {
                PostInfo post_info = new PostInfo(small_article.TitleLink, "");
                GrabResults GrabbingResult = new GrabResults(searchQuery, post_info, new ProcessingInfo(StartTime, ""));
                log.Debug(String.Format("{1}: Обработка статьи: {0}", small_article.TitleLink, Thread.CurrentThread.Name));

                driver.Navigate().GoToUrl(small_article.TitleLink);         // На страницу статьи
                GrabbingResult.SourceDescription.Title = driver.FindElementByXPath("//div[@class='post__header']/h1/span").Text;

                int annotationSize = 800;       // Количество символов для аннотации
                string article = driver.FindElementByXPath("//div[@class='content html_format']").Text;

                GrabbingResult.SourceDescription.Annotation = article.Remove(annotationSize);

                GrabbingResult.SourceDescription.Language = LanguageIdentifier(article);      // Определение языка статьи - "ru" или "en"

                GrabbingResult.SourceDescription.Popularity = 1;        // TODO: Пока заглушка

                // Получить изображение статьи
                string image_url = driver.FindElementByXPath("//div[@class='content html_format']/div/img").GetAttribute("src");
                WebClient web_client = new WebClient();
                log.Debug(String.Format("{0}: Downloading Image from \"{1}\" .......\n\"", Thread.CurrentThread.Name, image_url));
                byte[] image_file_as_byte = web_client.DownloadData(image_url);                             // Загрузить как массив байт
                  // В base64
                GrabbingResult.SourceDescription.Image = "data:image/jpeg;base64," + Convert.ToBase64String(image_file_as_byte);        

                GrabbingResult.Processing.FinishTime = DateTime.Now.ToString("s");      // Время окончания грабинга

                grabResultsList.Add(GrabbingResult);        // Добавить статью в список
            }

            // Собрать мусор
            // GC.WaitForPendingFinalizers();
            // GC.Collect();

            return grr;

        }


        /// Определение языка статьи - "ru" или "en"
        private string LanguageIdentifier(string text)
        {
            string Result;
            int engCount = 0;
            int rusCount = 0;

            foreach (char c in text)
            {
                if ((c > 'а' && c < 'я') || (c > 'А' && c < 'Я'))
                    rusCount++;
                else if ((c > 'a' && c < 'z') || (c > 'A' && c < 'Z'))
                    engCount++;
            }

            if (rusCount > engCount)
                Result = "ru";
            else
                Result = "en";
            log.Debug(String.Format("{0}: Язык статьи: {1}", Thread.CurrentThread.Name, Result));
            return Result;
        }


        /// Выбирает пре-статьи по требуемому пермоду дат
        private void GetSmallArticleInTrueDate(DateTime from_date, DateTime to_date, RemoteWebDriver driver, List<SmallArticle> sa_list)
        {
            do
            {
                int small_article_count = driver.FindElementsByXPath("//div[contains(@class,'post post_teaser shortcuts_item')]").Count();
                for (int i = 1; i <= small_article_count; i++)
                {
                    string date = driver.FindElementByXPath(
                        String.Format("//div[contains(@class,'post post_teaser shortcuts_item')][{0}]/div/span", i)
                    ).Text;

                    string titlelink = driver.FindElementByXPath(
                        String.Format("//div[contains(@class,'post post_teaser shortcuts_item')][{0}]/div[@class='post__header']/h2/a[@class='post__title_link']", i)
                    ).GetAttribute("href");

                    DateTime article_date = ConvertArticleDate(date);
                    SmallArticle sa = new SmallArticle(article_date, titlelink);

                    // При попадание даты статьи в требуемый диапазон - добавить статью в список
                    if (article_date > from_date && article_date < to_date)
                    {
                        // TODO: Заменить на добавление сразу в GrabResults???
                        sa_list.Add(sa);        // Добавить пре-статью в список
                    }
                }

                // Перейти на следующую страницу
                try
                {
                    if (driver.FindElementByXPath("//a[@id='next_page']").Displayed)        // Ссылка 'ТУДА ->'
                    {
                        driver.FindElementByXPath("//a[@id='next_page']").Click();
                    }
                }
                catch (NoSuchElementException)
                {
                    break;          // Если ссылки 'ТУДА ->' нет, то выходим из цикла
                }

            } while (true);
            Thread.Sleep(15000);
        }

 

        /// Конвертирует дату из формата GrabRequest в DateTime формат
        private DateTime ConvertGrabRequestDate(string date)
        {
            int year = Convert.ToUInt16(date.Split('-')[0]);
            int month = Convert.ToUInt16(date.Split('-')[1]);
            int day = Convert.ToUInt16(date.Split('-')[2]);
            return new DateTime(year, month, day);
        }


        /// Конвертирует дату из формата Хабра в DateTime формат
        private DateTime ConvertArticleDate(string date)
        {
            int day = Convert.ToUInt16(date.Split(' ')[0]);       // отделить день
            int month = ConvertMonthToInt(date.Split(' ')[1]);     // отделить месяц

            // определить год
            int year;
            if (date.Split(' ')[2].Equals("в"))
            {
                year = DateTime.Now.Year;
            }
            else
            {
                year = Convert.ToUInt16(date.Split(' ')[2]);   
            }

            return new DateTime(year, month, day);
        }

        /// Конвертирует месяц из строкового значения в целое число
        private int ConvertMonthToInt(string month)
        {
            int digital_month = 1;
            switch (month)
            {
                case "января":
                    digital_month = 1;
                    break;
                case "февраля":
                    digital_month = 2;
                    break;
                case "марта":
                    digital_month = 3;
                    break;
                case "апреля":
                    digital_month = 4;
                    break;
                case "мая":
                    digital_month = 5;
                    break;
                case "июня":
                    digital_month = 6;
                    break;
                case "июля":
                    digital_month = 7;
                    break;
                case "августа":
                    digital_month = 8;
                    break;
                case "сентября":
                    digital_month = 9;
                    break;
                case "октября":
                    digital_month = 10;
                    break;
                case "ноября":
                    digital_month = 11;
                    break;
                case "декабря":
                    digital_month = 12;
                    break;
            }
            return digital_month;
        }


        /// Инициализирует WebDriver для браузера, указанного в конфигурационном файле 
        private static RemoteWebDriver InitBrowser(GrabConfig config, RemoteWebDriver driver)
        {
            if (config.grabber.browser == "phantomjs")
                driver = new PhantomJSDriver();
            else if (config.grabber.browser == "chrome")
            {
                driver = new ChromeDriver();
                if (config.grabber.browser_size != null)
                {
                    // Выставить размер окна браузера
                    log.Debug(String.Format("{0}: Выставляем размер окна браузера.", Thread.CurrentThread.Name));
                    Char delimiter = 'x';
                    int width = Convert.ToInt32(config.grabber.browser_size.Split(delimiter)[0]);
                    int height = Convert.ToInt32(config.grabber.browser_size.Split(delimiter)[1]);
                    driver.Manage().Window.Size = new System.Drawing.Size(width, height);
                }
                // Если размера нет в конфигурационном файле, то максимальный размер окна браузера
                if (config.grabber.browser_size == null)
                    log.Debug(String.Format("{0}: Выставляем максимальный размер окна браузера.", Thread.CurrentThread.Name));
                driver.Manage().Window.Maximize();
            }
            else
            {
                log.Error(String.Format("{1}: Browser not specified in config file '{0}'.", config, Thread.CurrentThread.Name));
                Console.Read();
                Environment.Exit(1);
            }
            return driver;
        }
    }
}
