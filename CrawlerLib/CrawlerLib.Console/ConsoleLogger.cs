// <copyright file="ConsoleLogger.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Console
{
    using System;
    using Logger;

    public class ConsoleLogger : ILogger
    {
        public void Log(LogRecord log)
        {
            Console.WriteLine(log.ToString());
        }
    }
}