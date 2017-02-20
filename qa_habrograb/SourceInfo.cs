namespace qa_habrograb
{
    /// Описание источника новостей, с которым работает Грабер.
    public class SourceInfo
    {
        public SourceInfo(string title, string url, string logo="")
        {
            this.Title = title;
            this.Url = url;
            this.Logo = logo;
        }

        public string Title { get; set; }       // Краткое человеческое наименование ресурса, 
                                                // желательно русскоязычная версия.
        public string Url { get; set; }         // Адрес главной страницы веб-сайта источника новостей.
        public string Logo { get; set; }        // Логотип источника новостей в формате PNG или JPG, закодированный 
                                                // в base64, с исходным размером (до base64) не более 100 кб.
    }
}
