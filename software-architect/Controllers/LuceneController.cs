using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using software_architect.Models;
using software_architect.Search;

namespace software_architect.Controllers
{
    public class LuceneController : Controller
    {
        private static readonly LuceneSearch searcher = new LuceneSearch();

        public ActionResult Index(string[] name, string[] city, string[] street, string[] houseNo)
        {
            var request = this.Request;

            var model = new LuceneViewModel();
            model.Filter = searcher.GetFilter(name, city, street, houseNo);
            model.Rows = searcher.GetRows(name, city, street, houseNo);
            return View(model);
        }
    }
}