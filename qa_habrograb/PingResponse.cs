using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qa_habrograb
{
    public class PingResponse
    {
        public bool result { get; set; }
        public Error error { get; set; }
    }

    public class Inner
    {
        public string className { get; set; }
        public string message { get; set; }
        public string stackTrace { get; set; }
    }

    public class Exception
    {
        public string className { get; set; }
        public string message { get; set; }
        public string stackTrace { get; set; }
        public Inner inner { get; set; }
    }

    public class Error
    {
        public string text { get; set; }
        public string time { get; set; }
        public Exception exception { get; set; }
    }
}
