using System;
using System.Collections.Generic;
using System.Linq;

namespace software_architect.Search
{
    public class SearchDocument
    {
        public IList<SearchDocumentField> Fields { get; set; }

        public string GetStringValue(string fieldName, string defaultValue = null)
        {
            var fields = Fields;
            if (fields != null && fields.Count > 0)
            {
                var field = fields.FirstOrDefault(
                    f => string.Equals(f.FieldName, fieldName, StringComparison.InvariantCultureIgnoreCase));

                if (field != null)
                    return field.Value;
            }

            return defaultValue;
        }
    }
}
