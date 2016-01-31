using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace software_architect.Search
{
    public class LuceneSearch
    {
        RAMDirectory directory = new RAMDirectory();

        public LuceneSearch()
        {
            Initialize();
        }

        public IList<Filter> GetFilter(string[] names, string[] cities, string[] streets, string[] houseNos)
        {
            var filtersToReturn = new List<Filter>
            {
                new Filter {Name = "Name", Items = new List<FilterItem>(), CheckedValues = CreateSet(names)},
                new Filter {Name = "City", Items = new List<FilterItem>(), CheckedValues = CreateSet(cities)},
                new Filter {Name = "Street", Items = new List<FilterItem>(), CheckedValues = CreateSet(streets)},
                new Filter {Name = "HouseNo", Items = new List<FilterItem>(), CheckedValues = CreateSet(houseNos)},
            };

            var nameFilter = new Filter { Name = "Name", Items = new List<FilterItem>(), CheckedValues = CreateSet(names) };
            var cityFilter = new Filter { Name = "City", Items = new List<FilterItem>(), CheckedValues = CreateSet(cities) };
            var streetFilter = new Filter { Name = "Street", Items = new List<FilterItem>(), CheckedValues = CreateSet(streets) };
            var houseNoFilter = new Filter { Name = "HouseNo", Items = new List<FilterItem>(), CheckedValues = CreateSet(houseNos) };

            var filters = new List<Filter> { nameFilter, cityFilter, streetFilter, houseNoFilter };

            var reader = IndexReader.Open(directory, true);

            foreach (var filter in filtersToReturn)
            {
                SimpleFacetedSearch search = new SimpleFacetedSearch(reader, new[] { filter.Name });

                var query = GetQuery(names, cities, streets, houseNos, filter.Name);
                var hits = search.Search(query);
                foreach (var facet in hits.HitsPerFacet)
                {
                    var hitCount = facet.HitCount;
                    filter.Items.Add(new FilterItem
                    {
                        Id = facet.Name[0],
                        Name = $"{facet.Name[0]} ({hitCount})",
                        Checked = filter.CheckedValues.Contains(facet.Name[0])
                    });
                }
                filter.Items.Sort((s1, s2) => string.Compare(s1.Name, s2.Name, StringComparison.Ordinal));
            }

            return filtersToReturn;
        }

        private static HashSet<string> CreateSet(string[] values)
        {
            var set = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            if (values != null)
            {
                set.UnionWith(values);
            }
            return set;
        }

        public IList<Row> GetRows(string[] names, string[] cities, string[] streets, string[] houseNos)
        {
            var rows = new List<Row>();

            var reader = IndexReader.Open(directory, true);
            var query = GetQuery(names, cities, streets, houseNos, "");

            SimpleFacetedSearch search = new SimpleFacetedSearch(reader, new string[] { });
            var hits = search.Search(query);

            foreach (var facet in hits.HitsPerFacet)
            {
                foreach (Document doc in facet.Documents)
                {
                    var id = doc.GetField("Id").StringValue;
                    var name = doc.GetField("Name").StringValue;
                    var city = doc.GetField("City").StringValue;
                    var street = doc.GetField("Street").StringValue;
                    var houseNo = doc.GetField("HouseNo").StringValue;

                    var row = new Row
                    {
                        Values = new List<string> { id, name, city, street, houseNo }
                    };

                    rows.Add(row);
                }
            }

            return rows;
        }

        private static Query GetQuery(string[] names, string[] cities, string[] streets, string[] houseNos, string name)
        {
            Query query;
            if ((names != null && names.Length > 0) ||
                (cities != null && cities.Length > 0) ||
                (streets != null && streets.Length > 0) ||
                (houseNos != null && houseNos.Length > 0))
            {
                var queryBuilder = new StringBuilder();
                if (!string.Equals(name, "Name", StringComparison.InvariantCultureIgnoreCase))
                    AddQuery(queryBuilder, "Name", names);
                if (!string.Equals(name, "City", StringComparison.InvariantCultureIgnoreCase))
                    AddQuery(queryBuilder, "City", cities);
                if (!string.Equals(name, "Street", StringComparison.InvariantCultureIgnoreCase))
                    AddQuery(queryBuilder, "Street", streets);
                if (!string.Equals(name, "HouseNo", StringComparison.InvariantCultureIgnoreCase))
                    AddQuery(queryBuilder, "HouseNo", houseNos);

                var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "Id", new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
                {
                    AllowLeadingWildcard = true
                };

                var q = queryBuilder.ToString();
                if (q.Length > 0)
                    query = parser.Parse(q);
                else
                    query = new MatchAllDocsQuery();
            }
            else
            {
                query = new MatchAllDocsQuery();
            }
            return query;
        }

        private static void AddQuery(StringBuilder queryBuilder, string fieldName, string[] names)
        {
            if (names == null || names.Length == 0)
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

        private void Initialize()
        {
            IndexWriter writer = new IndexWriter(
                directory,
                new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
                true,
                IndexWriter.MaxFieldLength.UNLIMITED);

            writer.AddDocument(CreateDoc("1", "Romas", "Kaunas", "Savanorių", "Vienas"));
            writer.AddDocument(CreateDoc("2", "Simona", "Vilnius", "Ukmergės", "Du"));
            writer.AddDocument(CreateDoc("3", "Romas", "Panevėžys", "Smėlynės", "Vienas"));
            writer.AddDocument(CreateDoc("4", "Simona", "Kaunas", "Savanorių", "Du"));
            writer.AddDocument(CreateDoc("5", "Romas", "Panevėžys", "Smėlynės", "Trys"));
            writer.AddDocument(CreateDoc("6", "Rokas", "Kaunas", "Savanorių", "Keturi"));
            writer.AddDocument(CreateDoc("7", "Romas", "Kaunas", "Taikos", "Trys"));
            writer.AddDocument(CreateDoc("8", "Simona", "Vilnius", "Taikos", "Penki"));
            writer.AddDocument(CreateDoc("9", "Romas", "Vilnius", "Ukmergės", "Penki"));
            writer.AddDocument(CreateDoc("10", "Simona", "Kaunas", "Taikos", "Keturi"));
            writer.AddDocument(CreateDoc("11", "Romas", "Panevėžys", "Taikos", "Vienas"));
            writer.AddDocument(CreateDoc("12", "Romas", "Kaunas", "Savanorių", "Du"));
            writer.AddDocument(CreateDoc("13", "Simona", "Vilnius", "Savanorių", "Trys"));
            writer.AddDocument(CreateDoc("14", "Remigijus", "Panevėžys", "Taikos", "Keturi"));
            writer.AddDocument(CreateDoc("15", "Romas", "Kaunas", "Taikos", "Keturi"));
            writer.AddDocument(CreateDoc("16", "Simona", "Kaunas", "Savanorių", "Du"));
            writer.AddDocument(CreateDoc("17", "Romas", "Vilnius", "Savanorių", "Vienas"));
            writer.AddDocument(CreateDoc("18", "Rokas", "Klaipėda", "Pramonės", "Trys"));
            writer.AddDocument(CreateDoc("19", "Asta", "Klaipėda", "Vilniaus", "Penki"));
            writer.AddDocument(CreateDoc("20", "Romas", "Kaunas", "Savanorių", "Vienas"));

            writer.Dispose(true);
        }

        private static Document CreateDoc(string id, string name, string city, string street, string houseNo)
        {
            var doc = new Document();
            doc.Add(new Field("Id", id, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("Name", name, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("City", city, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("Street", street, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("HouseNo", houseNo, Field.Store.YES, Field.Index.ANALYZED));
            return doc;
        }

    }
}
