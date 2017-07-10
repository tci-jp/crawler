namespace CrawlerLib
{
    public class RobotsTxt
    {
        private string robotstxt;

        public static readonly RobotsTxt DefaultInstance = new RobotsTxt();

        private RobotsTxt()
        {
        }

        public RobotsTxt(string robotstxt)
        {
            this.robotstxt = robotstxt;
        }
    }
}