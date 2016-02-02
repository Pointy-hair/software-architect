using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace software_architect.Search.Services.Lucene
{
    internal class LuceneSearchService : ISearchService
    {
        private readonly Directory _directory = new RAMDirectory();
        private bool _created;

        public void Save(SearchDocument doc)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));

            if (doc.Fields == null || doc.Fields.Count == 0)
                return;

            var document = new Document();
            foreach (var docField in doc.Fields)
            {
                if (string.IsNullOrWhiteSpace(docField.FieldName))
                    throw new Exception("Field name cannot be empty");

                if (string.IsNullOrWhiteSpace(docField.Value))
                    continue;

                var field = new Field(docField.FieldName, docField.Value, Field.Store.YES, Field.Index.ANALYZED);
                document.Add(field);
            }

            var writer = new IndexWriter(
                _directory,
                new StandardAnalyzer(global::Lucene.Net.Util.Version.LUCENE_30),
                !_created,
                IndexWriter.MaxFieldLength.UNLIMITED);

            using (writer)
            {
                writer.AddDocument(document);
            }

            _created = true;
        }

        public IList<SearchDocument> Search(IList<Filter> searchFilters, IList<SearchSortField> sortFields)
        {
            if (searchFilters == null || searchFilters.Count == 0)
                return new List<SearchDocument>();

            var sort = CreateSort(searchFilters, sortFields);

            var reader = IndexReader.Open(_directory, true);
            var searcher = new IndexSearcher(reader);
            var query = CreateQuery(searchFilters);
            var hits = searcher.Search(query, new QueryWrapperFilter(query), 100, sort);

            var searchResult = new List<SearchDocument>();
            foreach (var facet in hits.ScoreDocs)
            {
                var doc = searcher.Doc(facet.Doc);
                var searchDoc = new SearchDocument
                {
                    Fields = new List<SearchDocumentField>()
                };

                var docFields = doc.GetFields();
                foreach (var field in docFields)
                {
                    var value = doc.Get(field.Name);
                    searchDoc.Fields.Add(new SearchDocumentField {FieldName = field.Name, Value = value});
                }

                searchResult.Add(searchDoc);
            }
            return searchResult;
        }

        public IList<SearchFilter> GetFilter(IList<Filter> searchFilters)
        {
            var reader = IndexReader.Open(_directory, true);

            var filters = new List<SearchFilter>();
            foreach (var searchFilter in searchFilters)
            {
                var items = new List<SearchFilterItem>();

                var filter = new SearchFilter
                {
                    FilterName = searchFilter.FieldName,
                    Items = items
                };

                filters.Add(filter);

                var query = CreateQuery(searchFilters, searchFilter.FieldName);

                SimpleFacetedSearch search = new SimpleFacetedSearch(reader, searchFilter.FieldName);
                var facets = search.Search(query);

                foreach (var facet in facets.HitsPerFacet)
                {
                    var hitCount = facet.HitCount;
                    if (hitCount == 0)
                        continue;

                    string value = string.Empty;
                    foreach (var document in facet.Documents)
                    {
                        value = document.Get(searchFilter.FieldName);
                        if (!string.IsNullOrWhiteSpace(value))
                            break;
                    }

                    if (string.IsNullOrEmpty(value))
                        continue;

                    var isQueried =
                        searchFilter.Values.Any(s => string.Equals(s, value, StringComparison.InvariantCultureIgnoreCase));

                    var item = new SearchFilterItem
                    {
                        Label = value,
                        Value = value,
                        Hits = (int) facet.HitCount,
                        IsQueried = isQueried
                    };

                    filter.Items.Add(item);
                }

                items.Sort((s1, s2) => string.Compare(s1.Value, s2.Value, StringComparison.Ordinal));
            }

            return filters;
        }

        private static Query CreateQuery(IList<Filter> searchFilters, string fieldName = null)
        {
            Query query = new MatchAllDocsQuery();

            var queryBuilder = new StringBuilder();
            foreach (var searchFilter in searchFilters)
            {
                if (!string.IsNullOrWhiteSpace(fieldName) && searchFilter.FieldName == fieldName)
                    continue;

                AddQuery(queryBuilder, searchFilter.FieldName, searchFilter.Values);
            }

            var select = queryBuilder.ToString();
            if (!string.IsNullOrEmpty(select))
            {
                var parser = new QueryParser(global::Lucene.Net.Util.Version.LUCENE_30, "",
                    new StandardAnalyzer(global::Lucene.Net.Util.Version.LUCENE_30))
                {
                    AllowLeadingWildcard = true
                };

                query = parser.Parse(@select);
            }
            return query;
        }

        private static void AddQuery(StringBuilder queryBuilder, string fieldName, IEnumerable<string> names)
        {
            if (names == null)
                return;

            var subQueryBuilder = new StringBuilder();

            foreach (var value in names)
            {
                if (subQueryBuilder.Length > 0)
                {
                    subQueryBuilder.Append(" OR ");
                }
                subQueryBuilder.Append($"{fieldName}:{value}");
            }

            if (subQueryBuilder.Length > 0)
            {
                if (queryBuilder.Length > 0)
                {
                    queryBuilder.Append(" AND ");
                }

                queryBuilder.Append($"({subQueryBuilder})");
            }
        }

        private static Sort CreateSort(IList<Filter> searchFilters, IList<SearchSortField> sortFields)
        {
            SortField[] fields;
            if (sortFields != null && sortFields.Count > 0)
            {
                fields = new SortField[sortFields.Count];
                for (int i = 0; i < sortFields.Count; i++)
                {
                    var sortField = sortFields[i];
                    fields[i] = new SortField(sortField.FieldName, CultureInfo.InvariantCulture, sortField.Descending);
                }
            }
            else
            {
                var fieldName = searchFilters.First().FieldName;
                fields = new []
                {
                    new SortField(fieldName, CultureInfo.InvariantCulture)
                };
            }
            var sort = new Sort(fields);
            return sort;
        }
    }
}
