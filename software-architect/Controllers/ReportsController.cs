using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using software_architect.Reporting.Jira;

namespace software_architect.Controllers
{
    public class ReportsController : Controller
    {
        // GET: /<controller>/
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            try
            {
                byte[] buffer = new byte[file.ContentLength];
                using (var stream = file.InputStream)
                {
                    stream.Read(buffer, 0, (int)file.ContentLength);
                }

                var content = Encoding.UTF8.GetString(buffer);


                var report = new JiraReport();
                var fileContent = report.Create(content);

                return File(fileContent, "application/pdf", "tasks.pdf");
            }
            catch (Exception e)
            {
                return Content(e.ToString());
            }
        }
    }
}