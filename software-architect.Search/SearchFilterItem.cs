namespace software_architect.Search
{
    public class SearchFilterItem
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public int Hits { get; set; }
        public bool IsQueried { get; set; }
    }
}
