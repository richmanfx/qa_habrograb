using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qa_habrograb
{
    /// Информация об исключении, вызвавшем ошибку
    public class ExceptionInfo
    {
        public ExceptionInfo(string className, ExceptionInfo inner)
        {
            this.ClassName = className;
            this.Inner = inner;
            List<string> StackTrace = new List<string>();
        }

        public string ClassName { get; set; }           // Имя класса исключения.
        public string Message { get; set; }             // Сообщение исключения.
        public List<string> StackTrace { get; set; }    // Массив String - стек вызовов исключения.
        public ExceptionInfo Inner { get; set; }        // Аналогичная информация о внутреннем исключении.
    }
}
