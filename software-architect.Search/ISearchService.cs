using System.Collections.Generic;

namespace software_architect.Search
{
    public interface ISearchService
    {
        void Save(SearchDocument doc);
        IList<SearchDocument> Search(IList<Filter> searchFilters, IList<SearchSortField> sortFields);
        IList<SearchFilter> GetFilter(IList<Filter> searchFilters);
    }
}