﻿// <copyright file="TasksForEachUrl.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerUI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class TasksForEachUrl
    {
        public Task CrawTask { get; set; }

        public Task UpdateListTask { get; set; }

        public bool Finished { get; set; }

        public int Count { get; set; }
    }
}
