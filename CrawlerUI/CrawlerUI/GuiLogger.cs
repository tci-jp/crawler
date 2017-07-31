// <copyright file="GuiLogger.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerUI
{
    using CrawlerLib.Logger;
    using Models;

    /// <inheritdoc />
    /// <summary>
    /// Using model as logger for GUI.
    /// </summary>
    public class GuiLogger : ILogger
    {
        private readonly Model model;

        /// <inheritdoc />
        public GuiLogger(Model model)
        {
            this.model = model;
            model.ResetLog();
        }

        /// <inheritdoc />
        public void Log(LogRecord log)
        {
            model.AddLogLine(log.ToString());
        }
    }
}