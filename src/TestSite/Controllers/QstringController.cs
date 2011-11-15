using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TestSite.Controllers
{
    public class QstringController : Controller
    {
        //
        // GET: /Qstring/

        public ActionResult Index(string arg1)
        {
            return Content("Querystring arg1 is: " + arg1);
        }

    }
}
