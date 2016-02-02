using System.Collections.Generic;
using software_architect.Search.Services.Lucene;

namespace software_architect.Search.Services
{
    public class SearchService
    {
        private readonly ISearchService _luceneSearchService;

        public SearchService()
        {
            _luceneSearchService = new LuceneSearchService();
            Initialize(_luceneSearchService);
        }

        public IList<SearchFilter> GetFilters(IList<Filter> searchFilters)
        {
            var filters = _luceneSearchService.GetFilter(searchFilters);
            return filters;
        }

        public IList<SearchDocument> Search(IList<Filter> searchFilters)
        {
            return _luceneSearchService.Search(searchFilters, null);
        }

        private void Initialize(ISearchService searchService)
        {
            searchService.Save(CreateSearchDoc("1", "Draenei", "London", "BeachStr", "Manager"));
            searchService.Save(CreateSearchDoc("2", "Dwarf", "Barcelona", "BroomeStr", "Developer"));
            searchService.Save(CreateSearchDoc("3", "Draenei", "Berlin", "DelanceyStr", "Manager"));
            searchService.Save(CreateSearchDoc("4", "Dwarf", "London", "BeachStr", "Developer"));
            searchService.Save(CreateSearchDoc("5", "Draenei", "Berlin", "DelanceyStr", "Tester"));
            searchService.Save(CreateSearchDoc("6", "Gnome", "London", "BeachStr", "Director"));
            searchService.Save(CreateSearchDoc("7", "Draenei", "London", "LenoxAv", "Tester"));
            searchService.Save(CreateSearchDoc("8", "Dwarf", "Barcelona", "LenoxAv", "Administrator"));
            searchService.Save(CreateSearchDoc("9", "Draenei", "Barcelona", "BroomeStr", "Administrator"));
            searchService.Save(CreateSearchDoc("10", "Dwarf", "London", "LenoxAv", "Director"));
            searchService.Save(CreateSearchDoc("11", "Draenei", "Berlin", "LenoxAv", "Manager"));
            searchService.Save(CreateSearchDoc("12", "Draenei", "London", "BeachStr", "Developer"));
            searchService.Save(CreateSearchDoc("13", "Dwarf", "Barcelona", "BeachStr", "Tester"));
            searchService.Save(CreateSearchDoc("14", "Worgen", "Berlin", "LenoxAv", "Director"));
            searchService.Save(CreateSearchDoc("15", "Draenei", "London", "LenoxAv", "Director"));
            searchService.Save(CreateSearchDoc("16", "Dwarf", "London", "BeachStr", "Developer"));
            searchService.Save(CreateSearchDoc("17", "Draenei", "Barcelona", "BeachStr", "Manager"));
            searchService.Save(CreateSearchDoc("18", "Gnome", "Rome", "MercerStr", "Tester"));
            searchService.Save(CreateSearchDoc("19", "Orc", "Rome", "RutgersStr", "Administrator"));
            searchService.Save(CreateSearchDoc("20", "Draenei", "London", "BeachStr", "Manager"));
        }

        private static SearchDocument CreateSearchDoc(string id, string name, string city, string street, string houseNo)
        {
            var doc = new SearchDocument
            {
                Fields = new List<SearchDocumentField>
                {
                    new SearchDocumentField {FieldName = "Id", Value = id},
                    new SearchDocumentField {FieldName = "Name", Value = name},
                    new SearchDocumentField {FieldName = "City", Value = city},
                    new SearchDocumentField {FieldName = "Street", Value = street},
                    new SearchDocumentField {FieldName = "HouseNo", Value = houseNo}
                }
            };
            return doc;
        }
    }
}
