using System.Collections.Generic;
using software_architect.Search.Services.Lucene;

namespace software_architect.Search.Services
{
    public class SearchService
    {
        private readonly ISearchService _luceneSearchService;

        public SearchService()
        {
            //_luceneSearchService = new LuceneSearchService();
            _luceneSearchService = new ElasticSearchService();
            ElasticSearchService.Initialize(_luceneSearchService);
        }

        public IList<SearchFilter> GetFilters(IList<Filter> searchFilters)
        {
            var filters = _luceneSearchService.GetFilter(searchFilters);
            return filters;
        }

        public IList<SearchDocument> Search(IList<Filter> searchFilters)
        {
            return _luceneSearchService.Search<SearchDocument>(searchFilters, null);
        }
    }
}
