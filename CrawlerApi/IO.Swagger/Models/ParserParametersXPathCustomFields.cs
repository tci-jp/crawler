// <copyright file="ParserParametersXPathCustomFields.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

/*
 * Crawler API
 *
 * API for crawling web pages and searching in crowled result
 *
 * OpenAPI spec version: 1.0.0
 * Contact: zakhar_amirov@dectech.tokyo
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace CrawlerApi.Models
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using CrawlerLib.Data;
    using JetBrains.Annotations;
    using Newtonsoft.Json;

    /// <summary>
    /// Metadata name and XPath for custom parsing.
    /// </summary>
    [DataContract]
    [UsedImplicitly]
    public class ParserParametersXPathCustomFields : IEquatable<ParserParametersXPathCustomFields>, IParserParametersXPathCustomFields
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParserParametersXPathCustomFields" /> class.
        /// </summary>
        /// <param name="name">name of schema.org or decode property in URI format (required).</param>
        /// <param name="xpath">
        /// XPath expression to extract metadata field from HTML DOM. May include decads:replace function for
        /// regular expressions. (required).
        /// </param>
        public ParserParametersXPathCustomFields(string name = null, string xpath = null)
        {
            // to ensure "Name" is required (not null)
            Name = name ?? throw new InvalidDataException(
                       "Name is a required property for ParserParametersCustomFields and cannot be null");

            // to ensure "Xpath" is required (not null)
            XPath =
                xpath ?? throw new InvalidDataException(
                    "Xpath is a required property for ParserParametersCustomFields and cannot be null");
        }

        /// <summary>
        /// Gets or sets name of schema.org or decode property in URI format
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets xPath expression to extract metadata field from HTML DOM. May include fn:replace and fn:match function for regular
        /// expressions.
        /// </summary>
        [DataMember(Name = "xpath")]
        public string XPath { get; set; }

        /// <summary>Compare objects equality</summary>
        /// <param name="left">Left part of expression.</param>
        /// <param name="right">Right part of expression.</param>
        public static bool operator ==(ParserParametersXPathCustomFields left, ParserParametersXPathCustomFields right)
        {
            return Equals(left, right);
        }

        /// <summary>Compare objects unequality</summary>
        /// <param name="left">Left part of expression.</param>
        /// <param name="right">Right part of expression.</param>
        public static bool operator !=(ParserParametersXPathCustomFields left, ParserParametersXPathCustomFields right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((ParserParametersXPathCustomFields)obj);
        }

        /// <summary>
        /// Returns true if ParserParametersCustomFields instances are equal
        /// </summary>
        /// <param name="other">Instance of ParserParametersCustomFields to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ParserParametersXPathCustomFields other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return
                (
                    (Name == other.Name) ||
                    ((Name != null) &&
                     Name.Equals(other.Name))) &&
                (
                    (XPath == other.XPath) ||
                    ((XPath != null) &&
                     XPath.Equals(other.XPath)));
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            // credit: http://stackoverflow.com/a/263416/677735
            unchecked
            {
                var hash = 41;

                // Suitable nullity checks etc, of course :)
                if (Name != null)
                {
                    hash = (hash * 59) + Name.GetHashCode();
                }

                if (XPath != null)
                {
                    hash = (hash * 59) + XPath.GetHashCode();
                }

                return hash;
            }
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ParserParametersCustomFields {\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Xpath: ").Append(XPath).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}