using System.Collections.Generic;
using System.Linq;

namespace software_architect.Search
{
    public class Filter
    {
        public string Name { get; set; }
        public List<FilterItem> Items { get; set; }
        public HashSet<string> CheckedValues { get; set; }

        public void Add(string id, string name)
        {
            if (Items.All(item => item.Id != id))
                Items.Add(new FilterItem { Id = id, Name = name });
        }

        public void Add(string[] ids)
        {
            if (ids == null)
                return;

            foreach (var id in ids)
            {
                if (Items.All(item => item.Id != id))
                    Items.Add(new FilterItem { Id = id });
            }
        }
    }

    public class FilterItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Checked { get; set; }
    }

    public class Row
    {
        public IList<string> Values { get; set; }
    }
}
