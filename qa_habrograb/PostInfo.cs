using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qa_habrograb
{
    public class PostInfo
    {
        public PostInfo(string url, string language, string title = "Нет заголовка", string annotation = "Нет аннотации")
        {
            this.Url = url;
            this.Language = language;
            this.Title = title;
            this.Annotation = annotation;
        }

        public string Url { get; set; }             // Адрес поста на ресурсе-источнике.

        public string Original { get; set; }        // Если пост на ресурсе является отсылкой к другой странице 
                                                    // (возможно, даже не другом сайте), то тут указывается конечная 
                                                    // ссылка на источник.

        public string Language { get; set; }        // Язык поста: "ru" или "en".

        public string Title { get; set; }           // Заголовок поста, должен быть как можно короче.

        public string Annotation { get; set; }      // Аннотация к посту. Например, первый его абзац. Желательно не более
                                                    // 1000 символов, всё равно на ядре при составлении письма придётся 
                                                    // обрезать ради экономии места.

        public int Popularity { get; set; }         // Условное значение популярности поста. В первую очередь важна 
                                                    // популярность относительно остальных постов с этого же ресурса, 
                                                    // поэтому формула, по которой считается популярность, лежит на 
                                                    // Граббере.

        public string Image { get; set; }           // Картинка, прикреплённая к посту, в формате PNG или JPG, 
                                                    // закодированная в base64, с исходным размером (до base64) не 
                                                    // более 200 кб.
    }
}
