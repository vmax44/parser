using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ParserSite;

namespace ParserSite.Controllers
{
    public class ReportsController : Controller
    {
        private ParserContext db = new ParserContext();

        // POST: Reports/Test
        [HttpPost]
        public ActionResult Test(int[] selectedparts,int OrderId)
        {
            if (selectedparts == null)
            {
                return HttpNotFound();
            }
            var order = db.Orders.Find(OrderId);
            List<ParsedData> datas = new List<ParsedData>();
            foreach (var partid in selectedparts)
            {
                var part = order.Parts.Where(p => p.Id == partid)
                    .First();
                datas.AddRange(part.ParsedDatas);
            }
            return View(datas);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
