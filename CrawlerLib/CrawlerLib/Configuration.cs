namespace CrawlerLib
{
    using System;

    public class Configuration
    {
        public string UserAgent { get; set; } = "Mozilla/5.0 (Linux; Android 4.0.4; Galaxy Nexus Build/IMM76B) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1025.133 Mobile Safari/535.19";

        public ICrawlerStorage Storage { get; set; } = new DummyStorage();

        public int Depth { get; set; } = 3;

        public int HostDepth { get; set; } = 0;

        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public int RetriesNumber { get; set; } = 3;

        public TimeSpan RequestErrorRetryDelay { get; set; } = TimeSpan.FromSeconds(10);
    }
}