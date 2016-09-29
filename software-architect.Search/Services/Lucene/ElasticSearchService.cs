using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nest;

namespace software_architect.Search.Services.Lucene
{
    public class ElasticSearchService : ISearchService
    {
        public void Save<T>(T doc) where T : class
        {
            var connection = new ConnectionSettings(new Uri("http://localhost:9200"));
            connection.DefaultIndex("search_example");

            var client = new ElasticClient(connection);
            var index = client.GetIndex("search_example");
            if (!index.IsValid)
            {
                var response = client.CreateIndex("search_example");
            }

            client.Index(doc);

            client.Flush("search_example");
        }

        public IList<SearchDocument> Search<T>(IList<Filter> searchFilters, IList<SearchSortField> sortFields) where T : class
        {
            var connection = new ConnectionSettings(new Uri("http://localhost:9200"));
            connection.DefaultIndex("search_example");

            var filter = CreateFilter(searchFilters);

            var searchRequest = new SearchRequest<DeveloperModel>
            {
                Query = new BoolQuery {Filter = filter},
                Size = 100,
                From = 0
            };

            var client = new ElasticClient(connection);

            using (var stream = new MemoryStream())
            {
                client.Serializer.Serialize(searchRequest, stream);
                string searchJson = Encoding.UTF8.GetString(stream.ToArray());
                System.Diagnostics.Trace.WriteLine(searchJson);
            }

            var response = client.Search<DeveloperModel>(searchRequest);

            var result = new List<SearchDocument>();
            if (response.IsValid)
            {
                foreach (var model in response.Documents)
                {
                    var doc = new SearchDocument
                    {
                        Id = model.Id,
                        Fields = new List<SearchDocumentField>
                        {
                            new SearchDocumentField {FieldName = "Id", Value = model.Id},
                            new SearchDocumentField {FieldName = "City", Value = model.City},
                            new SearchDocumentField {FieldName = "HouseNo", Value = model.HouseNo},
                            new SearchDocumentField {FieldName = "Name", Value = model.Name},
                            new SearchDocumentField {FieldName = "Street", Value = model.Street}
                        }
                    };
                    result.Add(doc);
                }
            }
            return result;
        }

        public IList<SearchFilter> GetFilter(IList<Filter> searchFilters)
        {
            var connection = new ConnectionSettings(new Uri("http://localhost:9200"));
            connection.DefaultIndex("search_example");

            var searchRequest = new SearchRequest<DeveloperModel>
            {
                Query = new BoolQuery { Filter = CreateFilter(searchFilters) },
                Size = 100,
                From = 0,
                Aggregations = new AggregationDictionary(new Dictionary<string, AggregationContainer>
                {
                    {
                        "facets", new AggregationContainer
                        {
                            Global = new GlobalAggregation("global"),
                            Aggregations = new AggregationDictionary(new Dictionary<string, AggregationContainer>
                            {
                                {
                                    "name", new AggregationContainer
                                    {
                                        Filter = new FilterAggregation("name_filter")
                                        {
                                            Filter = new BoolQuery {Filter = CreateFilter(searchFilters, "name")}
                                        },
                                        Aggregations = new AggregationDictionary(new Dictionary<string, AggregationContainer>
                                        {
                                            { "name_count", new AggregationContainer
                                            {
                                                Terms = new TermsAggregation("terms")
                                                {
                                                    Field = "name"
                                                }
                                            } }
                                        })
                                    }
                                },
                                {
                                    "city", new AggregationContainer
                                    {
                                        Filter = new FilterAggregation("city_filter")
                                        {
                                            Filter = new BoolQuery {Filter = CreateFilter(searchFilters, "city")}
                                        },
                                        Aggregations = new AggregationDictionary(new Dictionary<string, AggregationContainer>
                                        {
                                            { "city_count", new AggregationContainer
                                            {
                                                Terms = new TermsAggregation("terms")
                                                {
                                                    Field = "city"
                                                }
                                            } }
                                        })
                                    }
                                },
                                {
                                    "street", new AggregationContainer
                                    {
                                        Filter = new FilterAggregation("street_filter")
                                        {
                                            Filter = new BoolQuery {Filter = CreateFilter(searchFilters, "street")}
                                        },
                                        Aggregations = new AggregationDictionary(new Dictionary<string, AggregationContainer>
                                        {
                                            { "street_count", new AggregationContainer
                                            {
                                                Terms = new TermsAggregation("terms")
                                                {
                                                    Field = "street"
                                                }
                                            } }
                                        })
                                    }
                                },
                                {
                                    "houseNo", new AggregationContainer
                                    {
                                        Filter = new FilterAggregation("houseNo_filter")
                                        {
                                            Filter = new BoolQuery {Filter = CreateFilter(searchFilters, "name")}
                                        },
                                        Aggregations = new AggregationDictionary(new Dictionary<string, AggregationContainer>
                                        {
                                            { "houseNo_count", new AggregationContainer
                                            {
                                                Terms = new TermsAggregation("terms")
                                                {
                                                    Field = "houseNo"
                                                }
                                            } }
                                        })
                                    }
                                },
                            })
                        }
                    }
                })
            };

            var client = new ElasticClient(connection);
            var response = client.Search<DeveloperModel>(searchRequest);

            using (var stream = new MemoryStream())
            {
                client.Serializer.Serialize(searchRequest, stream);
                string searchJson = Encoding.UTF8.GetString(stream.ToArray());
                System.Diagnostics.Trace.WriteLine(searchJson);
            }

            var result = new List<SearchFilter>();
            if (response.IsValid)
            {
                var devs = (SingleBucketAggregate)response.Aggregations["facets"];
                foreach (var category in devs.Aggregations)
                {
                    var fieldName = category.Key;

                    var items = new List<SearchFilterItem>();
                    var filter = new SearchFilter
                    {
                        FilterName = fieldName,
                        Items = items
                    };
                    result.Add(filter);

                    var searchFilter = searchFilters.FirstOrDefault(f => f.FieldName == fieldName);

                    var agg = (SingleBucketAggregate) category.Value;
                    foreach (var values in agg.Aggregations)
                    {
                        var bucket = (BucketAggregate) values.Value;
                        foreach (var bucketItem in bucket.Items)
                        {
                            var facetBucket = bucketItem as KeyedBucket;
                            if (facetBucket != null)
                            {
                                var isQueried =
                                    searchFilter?.Values.Any(
                                        s =>
                                            string.Equals(s, facetBucket.Key,
                                                StringComparison.InvariantCultureIgnoreCase)) ?? false;

                                items.Add(new SearchFilterItem
                                {
                                    Value = facetBucket.Key,
                                    Label = facetBucket.Key,
                                    Hits = (int) facetBucket.DocCount.GetValueOrDefault(),
                                    IsQueried = isQueried
                                });
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static List<QueryContainer> CreateFilter(IList<Filter> searchFilters, string exceptFieldName = null)
        {
            var filter = new List<QueryContainer>();

            foreach (var searchFilter in searchFilters)
            {
                if ((exceptFieldName == null || exceptFieldName != searchFilter.FieldName) &&
                    searchFilter.Values != null && searchFilter.Values.Count > 0)
                {
                    filter.Add(new TermsQuery
                    {
                        Field = searchFilter.FieldName,
                        Terms = searchFilter.Values
                    });
                }
            }

            if (filter.Count == 0)
                filter.Add(new MatchAllQuery());

            return filter;
        }

        private static Random _random = new Random();

        public static void Initialize(ISearchService searchService)
        {
            searchService.Save(CreateDeveloperModel("1", "Draenei", "London", "BeachStr", "Manager"));
            searchService.Save(CreateDeveloperModel("2", "Dwarf", "Barcelona", "BroomeStr", "Developer"));
            searchService.Save(CreateDeveloperModel("3", "Draenei", "Berlin", "DelanceyStr", "Manager"));
            searchService.Save(CreateDeveloperModel("4", "Dwarf", "London", "BeachStr", "Developer"));
            searchService.Save(CreateDeveloperModel("5", "Draenei", "Berlin", "DelanceyStr", "Tester"));
            searchService.Save(CreateDeveloperModel("6", "Gnome", "London", "BeachStr", "Director"));
            searchService.Save(CreateDeveloperModel("7", "Draenei", "London", "LenoxAv", "Tester"));
            searchService.Save(CreateDeveloperModel("8", "Dwarf", "Barcelona", "LenoxAv", "Administrator"));
            searchService.Save(CreateDeveloperModel("9", "Draenei", "Barcelona", "BroomeStr", "Administrator"));
            searchService.Save(CreateDeveloperModel("10", "Dwarf", "London", "LenoxAv", "Director"));
            searchService.Save(CreateDeveloperModel("11", "Draenei", "Berlin", "LenoxAv", "Manager"));
            searchService.Save(CreateDeveloperModel("12", "Draenei", "London", "BeachStr", "Developer"));
            searchService.Save(CreateDeveloperModel("13", "Dwarf", "Barcelona", "BeachStr", "Tester"));
            searchService.Save(CreateDeveloperModel("14", "Worgen", "Berlin", "LenoxAv", "Director"));
            searchService.Save(CreateDeveloperModel("15", "Draenei", "London", "LenoxAv", "Director"));
            searchService.Save(CreateDeveloperModel("16", "Dwarf", "London", "BeachStr", "Developer"));
            searchService.Save(CreateDeveloperModel("17", "Draenei", "Barcelona", "BeachStr", "Manager"));
            searchService.Save(CreateDeveloperModel("18", "Gnome", "Rome", "MercerStr", "Tester"));
            searchService.Save(CreateDeveloperModel("19", "Orc", "Rome", "RutgersStr", "Administrator"));
            searchService.Save(CreateDeveloperModel("20", "Draenei", "London", "BeachStr", "Manager"));
        }

        private static DeveloperModel CreateDeveloperModel(string id, string name, string city, string street, string houseNo)
        {
            var languages = new[] {"C#", "Java", "Perl", "PHP", "GO", "Erlang", "Javascript", "C++", "Delphi", "Pascal"};

            var doc = new DeveloperModel
            {
                Id = id,
                Name = name,
                City = city,
                Street = street,
                HouseNo = houseNo,
                Languages = new List<DevelopmentLanguage>()
            };

            var languageCount = _random.Next(1, 5);
            var set = new HashSet<string>();
            for (int i = 0; i < languageCount; ++i)
            {
                set.Add(languages[_random.Next(0, languages.Length - 1)]);
            }

            foreach (var language in set)
            {
                doc.Languages.Add(new DevelopmentLanguage
                {
                    Name = language,
                    ExperienceInYears = _random.Next(1, 30)
                });
            }

            return doc;
        }
    }
}
