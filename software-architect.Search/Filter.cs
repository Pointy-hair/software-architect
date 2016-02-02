using System.Collections.Generic;

namespace software_architect.Search
{
    public class Filter
    {
        public string FieldName { get; set; }
        public IList<string> Values { get; set; }
    }
}
