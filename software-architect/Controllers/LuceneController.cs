using System.Collections.Generic;
using System.Web.Mvc;
using software_architect.Models;
using software_architect.Search.Services;
using Filter = software_architect.Search.Filter;

namespace software_architect.Controllers
{
    public class LuceneController : Controller
    {
        private static readonly SearchService SearchService = new SearchService();

        public ActionResult Index(string[] name, string[] city, string[] street, string[] houseNo)
        {
            IList<Filter> filters = new List<Filter>
            {
                CreateFilter("Name", name),
                CreateFilter("City", city),
                CreateFilter("Street", street),
                CreateFilter("HouseNo", houseNo),
            };

            var model = new LuceneViewModel
            {
                Filter = SearchService.GetFilters(filters),
                Rows = SearchService.Search(filters)
            };

            return View(model);
        }

        private static Filter CreateFilter(string fieldName, string[] values)
        {
            var list = new List<string>();
            if (values != null)
                list.AddRange(values);
            return new Filter {FieldName = fieldName, Values = list};
        }
    }
}