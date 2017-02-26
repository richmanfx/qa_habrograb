using System;

namespace qa_habrograb
{
    public class SmallArticle
    {

        public SmallArticle(DateTime data, string title)
        {
            this.PublicationData = data;
            this.TitleLink = title;
        }

        public DateTime PublicationData { get; set; }       // Дата публикации статьи
        public string TitleLink { get; set; }               // Заголовок-ссылка статьи
    }
}
