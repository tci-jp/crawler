// <copyright file="Severity.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Logger
{
    /// <summary>
    /// Log Severity.
    /// </summary>
    public enum Severity
    {
        /// <summary>
        /// For debug records.
        /// </summary>
        Debug = 2,

        /// <summary>
        /// For trace records.
        /// </summary>
        Trace = 3,

        /// <summary>
        /// For error records.
        /// </summary>
        Error = 0,

        /// <summary>
        /// For warning records.
        /// </summary>
        Warning = 1
    }
}