using System.Collections.Generic;

namespace software_architect.Search
{
    public class SearchFilter
    {
        public string FilterName { get; set; }
        public SearchFilterType FilterType { get; set; }
        public IList<SearchFilterItem> Items { get; set; }
    }
}
