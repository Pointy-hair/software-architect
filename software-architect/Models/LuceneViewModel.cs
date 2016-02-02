using System.Collections.Generic;
using software_architect.Search;

namespace software_architect.Models
{
    public class LuceneViewModel
    {
        public IList<SearchFilter> Filter { get; set; }
        public IList<SearchDocument> Rows { get; set; }
    }
}