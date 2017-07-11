namespace CrawlerLib.Logger
{
    using System;
    using System.Text;

    public class LogRecord
    {
        public LogRecord(Severity severity, string message)
        {
            Severity = severity;
            Object = message;
        }

        public LogRecord(Severity severity, string message, Exception exception)
        {
            Severity = severity;
            Object = exception;
            Message = message;
        }

        public string Message { get; set; }

        public Severity Severity { get; set; }

        public object Object { get; set; }

        public override string ToString()
        {
            var str = new StringBuilder();
            str.Append(Severity);

            if (Message != null)
            {
                str.Append(" : ").Append(Message);
            }

            if (Object != null)
            {
                str.Append(Message != null ? "\r\n" : " : ").Append(Object);
            }

            return str.ToString();
        }
    }
}