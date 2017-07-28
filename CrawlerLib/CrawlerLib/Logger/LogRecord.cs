// <copyright file="LogRecord.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Logger
{
    using System.Text;

    /// <summary>
    /// General log record.
    /// </summary>
    public class LogRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogRecord" /> class.
        /// </summary>
        /// <param name="severity">Log record severiry.</param>
        /// <param name="obj">Object to put into log.</param>
        public LogRecord(Severity severity, object obj)
        {
            Severity = severity;
            Object = obj;
        }

        /// <summary>
        /// Gets or sets log record severity.
        /// </summary>
        public Severity Severity { get; set; }

        /// <summary>
        /// Gets or sets object to log.
        /// </summary>
        public dynamic Object { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var str = new StringBuilder();
            str.Append(Severity);

            if (Object != null)
            {
                str.Append(Object);
            }

            return str.ToString();
        }
    }
}