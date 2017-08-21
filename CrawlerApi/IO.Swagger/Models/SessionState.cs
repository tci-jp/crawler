// <copyright file="SessionState.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerApi.Models
{
    using System.Runtime.Serialization;
    using JetBrains.Annotations;

    /// <summary>
    /// Session execution state.
    /// </summary>
    public enum SessionState
    {
        /// <summary>
        /// Session is not started yet.
        /// </summary>
        [EnumMember(Value = "notStarted")]
        NotStarted = 0,

        /// <summary>
        /// Session is still in processing.
        /// </summary>
        [EnumMember(Value = "inProcess")]
        InProcess = 1,

        /// <summary>
        /// Session is finished successfully.
        /// </summary>
        [EnumMember(Value = "done")]
        Done = 3,

        /// <summary>
        /// Session processing failed because of some server error.
        /// </summary>
        [EnumMember(Value = "error")]
        Error = 4
    }
}