using System.Collections.Generic;
using software_architect.Search;

namespace software_architect.Models
{
    public class LuceneViewModel
    {
        public IList<Filter> Filter { get; set; }
        public IList<Row> Rows { get; set; }
    }
}