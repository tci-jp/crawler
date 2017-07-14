namespace WpfApp2
{
    using CrawlerLib.Logger;

    public class GuiLogger : ILogger
    {
        private Model model;

        public GuiLogger(Model model)
        {
            this.model = model;
            model.ResetLog();
        }

        public void Log(LogRecord log)
        {
            model.AddLogLine(log.ToString());
        }
    }
}