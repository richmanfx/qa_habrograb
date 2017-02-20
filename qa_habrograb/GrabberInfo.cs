using System.Collections.Generic;

namespace qa_habrograb
{
    /// Полная информация по Граберу.
    public class GrabberInfo
    {
        public GrabberInfo(string name, string version, List<AuthorInfo> autors, SourceInfo source)
        {
            this.Name = name;
            this.Version = version;
            this.Authors = autors;
            this.Source = source;
        }

        public string Name { get; set; }                // Человеческое название грабера.
        public string Version { get; set; }             // Версия грабера в формате, принятом в рамках проекта.
        public List<AuthorInfo> Authors { get; set; }   // Перечисление авторов Грабера.
        public SourceInfo Source { get; set; }          // Описание источника новостей, с которым работает Грабер.
    }
}
