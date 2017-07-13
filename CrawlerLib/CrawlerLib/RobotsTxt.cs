// <copyright file="RobotsTxt.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    ///     Reading and parsing robots.txt
    /// </summary>
    public class RobotsTxt
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, List<string>>> _parsedValue;
        private readonly string _userAgent;
        private string _lastKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="RobotsTxt"/> class.
        /// </summary>
        /// <param name="userAgent">UserAgent name</param>
        /// <param name="robotsContent">Content of robot.txt file</param>
        public RobotsTxt(string userAgent, string robotsContent)
        {
            this._parsedValue = new ConcurrentDictionary<string, ConcurrentDictionary<string, List<string>>>();
            this._userAgent = userAgent;
            this._lastKey = null;
            Parse(robotsContent);
        }

        /// <summary>
        ///     Search if the URI is accessible for this user
        /// </summary>
        /// <param name="userAgent">UserAgent name</param>
        /// <param name="search">URI to check</param>
        /// <returns>return if the access is allow or not</returns>
        public bool IsAllowedUrl(string userAgent, string search)
        {
            bool contain = true;
            int allowLenght = 0;
            string content = search.Replace(" ", string.Empty);

            if (this._parsedValue.Count > 0)
            {
                if (this._parsedValue["*"]["Disallow"].Contains(string.Empty))
                {
                    contain = true;
                }
                else if (this._parsedValue["*"]["Disallow"].Contains("/"))
                {
                    contain = false;
                }

                if (this._parsedValue["*"]["Allow"].Count > 0)
                {
                    if (this._parsedValue["*"]["Allow"].Contains(content))
                    {
                        contain = true;
                        allowLenght = content.Length;
                    }
                    else
                    {
                        foreach (string value in this._parsedValue["*"]["Allow"])
                        {
                            if (content.Contains(value) && value.Length > 0 &&
                                content.IndexOf(value, StringComparison.Ordinal) == 0)
                            {
                                contain = true;
                                allowLenght = value.Length;
                                break;
                            }
                        }
                    }
                }

                if (this._parsedValue["*"]["Disallow"].Count > 0)
                {
                    if (this._parsedValue["*"]["Disallow"].Contains(content))
                    {
                        contain = false;
                    }
                    else
                    {
                        foreach (string value in this._parsedValue["*"]["Disallow"])
                        {
                            if (value != "/" && content.Contains(value) && value.Length > 0 &&
                                content.IndexOf(value, StringComparison.Ordinal) == 0 && value.Length > allowLenght)
                            {
                                contain = false;
                                break;
                            }
                        }
                    }
                }
            }

            if (this._parsedValue.Count > 0 && this._parsedValue.ContainsKey(userAgent))
            {
                if (this._parsedValue[userAgent]["Disallow"].Contains(string.Empty))
                {
                    contain = true;
                }
                else if (this._parsedValue[userAgent]["Disallow"].Contains("/"))
                {
                    contain = false;
                }

                if (this._parsedValue[userAgent]["Allow"].Count > 0)
                {
                    if (this._parsedValue[userAgent]["Allow"].Contains(content))
                    {
                        contain = true;
                        allowLenght = content.Length;
                    }
                    else
                    {
                        foreach (string value in this._parsedValue[userAgent]["Allow"])
                        {
                            if (content.Contains(value) && value.Length > 0 &&
                                content.IndexOf(value, StringComparison.Ordinal) == 0)
                            {
                                contain = true;
                                allowLenght = value.Length;
                                break;
                            }
                        }
                    }
                }

                if (this._parsedValue[userAgent]["Disallow"].Count > 0)
                {
                    if (this._parsedValue[userAgent]["Disallow"].Contains(content))
                    {
                        contain = false;
                    }
                    else
                    {
                        foreach (string value in this._parsedValue[userAgent]["Disallow"])
                        {
                            if (value != "/" && content.Contains(value) && value.Length > 0 &&
                                content.IndexOf(value, StringComparison.Ordinal) == 0 && value.Length > allowLenght)
                            {
                                contain = false;
                                break;
                            }
                        }
                    }
                }
            }

            return contain;
        }

        /// <summary>
        ///     Search if the URI is accessible for the user set in the constructor
        /// </summary>
        /// <param name="search">URI to check</param>
        /// <returns>return if the access is allow or not</returns>
        public bool IsAllowedUrl(string search)
        {
            bool contain = true;
            int allowLenght = 0;
            string content = search.Replace(" ", string.Empty);

            if (this._parsedValue.Count > 0)
            {
                if (this._parsedValue["*"]["Disallow"].Contains(string.Empty))
                {
                    contain = true;
                }
                else if (this._parsedValue["*"]["Disallow"].Contains("/"))
                {
                    contain = false;
                }

                if (this._parsedValue["*"]["Allow"].Count > 0)
                {
                    if (this._parsedValue["*"]["Allow"].Contains(content))
                    {
                        contain = true;
                        allowLenght = content.Length;
                    }
                    else
                    {
                        foreach (string value in this._parsedValue["*"]["Allow"])
                        {
                            if (content.Contains(value) && value.Length > 0 &&
                                content.IndexOf(value, StringComparison.Ordinal) == 0)
                            {
                                contain = true;
                                allowLenght = value.Length;
                                break;
                            }
                        }
                    }
                }

                if (this._parsedValue["*"]["Disallow"].Count > 0)
                {
                    if (this._parsedValue["*"]["Disallow"].Contains(content))
                    {
                        contain = false;
                    }
                    else
                    {
                        foreach (string value in this._parsedValue["*"]["Disallow"])
                        {

                            if (value != "/" && content.Contains(value) && value.Length > 0 &&
                                content.IndexOf(value, StringComparison.Ordinal) == 0 && value.Length > allowLenght)
                            {
                                contain = false;
                                break;
                            }
                        }
                    }
                }
            }

            if (this._parsedValue.Count > 0 && this._parsedValue.ContainsKey(this._userAgent))
            {
                if (this._parsedValue[this._userAgent]["Disallow"].Contains(string.Empty))
                {
                    contain = true;
                }
                else if (this._parsedValue[this._userAgent]["Disallow"].Contains("/"))
                {
                    contain = false;
                }

                if (this._parsedValue[this._userAgent]["Allow"].Count > 0)
                {
                    if (this._parsedValue[this._userAgent]["Allow"].Contains(content))
                    {
                        contain = true;
                        allowLenght = content.Length;
                    }
                    else
                    {
                        foreach (string value in this._parsedValue[this._userAgent]["Allow"])
                        {
                            if (content.Contains(value) && value.Length > 0 &&
                                content.IndexOf(value, StringComparison.Ordinal) == 0)
                            {
                                contain = true;
                                allowLenght = value.Length;
                                break;
                            }
                        }
                    }
                }

                if (this._parsedValue[this._userAgent]["Disallow"].Count > 0)
                {
                    if (this._parsedValue[this._userAgent]["Disallow"].Contains(content))
                    {
                        contain = false;
                    }
                    else
                    {
                        foreach (string value in this._parsedValue[this._userAgent]["Disallow"])
                        {
                            if (value != "/" && content.Contains(value) && value.Length > 0 &&
                                content.IndexOf(value, StringComparison.Ordinal) == 0 && value.Length > allowLenght)
                            {
                                contain = false;
                                break;
                            }
                        }
                    }
                }
            }

            return contain;
        }

        private void Parse(string robotsContent)
        {
            string robotParse = robotsContent.Replace(" ", string.Empty);
            robotParse = robotParse.Replace("\r", string.Empty);
            const string userAgentStart = "User-agent:";
            const string userAgentEnd = "\n";

            robotParse = StoreUserAgent(robotParse);

            string[] stringSeparators = { userAgentEnd };
            string[] lines = robotParse.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in lines)
            {
                if (item.Contains(userAgentStart))
                {
                    StoreUserAgent(item);
                }
                else
                {
                    AddValueToKey(this._lastKey, item);
                }
            }
        }

        private string StoreUserAgent(string robotsContent)
        {
            int end;
            string robotParse;
            string userAgentContent;
            const string userAgentStart = "User-agent:";
            const string userAgentEnd = "\n";

            int start = robotsContent.IndexOf(userAgentStart, StringComparison.Ordinal) + userAgentStart.Length;

            if (robotsContent.Contains(userAgentEnd))
            {
                end = robotsContent.IndexOf(userAgentEnd, start, StringComparison.Ordinal);
            }
            else
            {
                end = robotsContent.Length;
            }

            userAgentContent = robotsContent.Substring(start, end - start);

            this._parsedValue.TryAdd(userAgentContent, new ConcurrentDictionary<string, List<string>>());
            this._parsedValue[userAgentContent].TryAdd("Disallow", new List<string>());
            this._parsedValue[userAgentContent].TryAdd("Allow", new List<string>());
            this._lastKey = userAgentContent;

            if (robotsContent.Contains(userAgentEnd))
            {
                end = end + 1;
            }

            robotParse = robotsContent.Replace(robotsContent.Substring((start - userAgentStart.Length), end), string.Empty);

            return robotParse;
        }

        private void AddValueToKey(string key, string value)
        {
            const string allow = "Allow";
            const string disallow = "Disallow";

            if (value.Contains(allow))
            {
                value = value.Replace(allow + ":", string.Empty);
                this._parsedValue[key][allow].Add(value);
            }
            else if (value.Contains(disallow))
            {
                value = value.Replace(disallow + ":", string.Empty);
                this._parsedValue[key][disallow].Add(value);
            }
        }
    }
}