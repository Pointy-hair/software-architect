using System.Collections.Generic;

namespace software_architect.Search
{
    public interface ISearchService
    {
        void Save<T>(T doc) where T : class;
        IList<SearchDocument> Search<T>(IList<Filter> searchFilters, IList<SearchSortField> sortFields) where T : class;
        IList<SearchFilter> GetFilter(IList<Filter> searchFilters);
    }
}