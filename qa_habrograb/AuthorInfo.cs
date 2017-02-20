namespace qa_habrograb
{
    /// Один из авторов граббера
    public class AuthorInfo
    {
        public AuthorInfo(string name, string email)
        {
            this.Name = name;
            this.Email = email;
        }

        public string Name { get; set; }        // Фамилия и имя автора на русском языке.
        public string Email { get; set; }       // Адрес электронной почты автора в домене @pflb.ru .
    }
}
