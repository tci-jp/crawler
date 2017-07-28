namespace CrawlerUI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CrawlerLib;

    public class SearchOperators
    {
        public IList<SearchCondition.Operator> Items { get; } =
            Enum.GetValues(typeof(SearchCondition.Operator)).Cast<SearchCondition.Operator>().ToList();
    }
}