using System.Collections.Generic;

namespace software_architect.Search.Services.Lucene
{
    public class DeveloperModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string HouseNo { get; set; }
        public IList<DevelopmentLanguage> Languages { get; set; }
    }

    public class DevelopmentLanguage
    {
        public string Name { get; set; }
        public int ExperienceInYears { get; set; }
    }
}

