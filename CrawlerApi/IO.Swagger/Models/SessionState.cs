// <copyright file="SessionState.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerApi.Models
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Session execution state.
    /// </summary>
    [DataContract]
    public enum SessionState
    {
        /// <summary>
        /// Session is not started yet.
        /// </summary>
        [EnumMember(Value = "notStarted")]
        [DataMember(Name = "notStarted")]
        NotStarted = 0,

        /// <summary>
        /// Session is still in processing.
        /// </summary>
        [EnumMember(Value = "inProcess")]
        [DataMember(Name = "inProcess")]
        InProcess = 1,

        /// <summary>
        /// Session is finished successfully.
        /// </summary>
        [EnumMember(Value = "done")]
        [DataMember(Name = "done")]
        Done = 3,

        /// <summary>
        /// Session processing failed because of some server error.
        /// </summary>
        [EnumMember(Value = "error")]
        [DataMember(Name = "error")]
        Error = 4
    }
}