using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ParserSite;
using Vmax44Parser.library;
using ParserLibrary;

namespace ParserSite.Controllers
{
    public static class MyExtentions
    {
        public static List<ParsedData> ToParsedData(this Vmax44ParserConnectedLayer.ParsedDataCollection parsed)
        {
            List<ParsedData> result = new List<ParsedData>();
            foreach (var p in parsed)
            {
                var r = new ParsedData();
                r.Description = p.desc;
                r.Firmname = p.firmname;
                r.Original = p.orig;
                r.ParseDate = DateTime.Today;
                r.ParserType = p.parsertype;
                r.Price = p.price;
                r.SearchedArtikul = p.searchedArtikul;
                r.Statistic = p.statistic;
                r.Url = p.url;
                result.Add(r);
            }
            return result;
        }
    }

    public class ParsedDatasController : Controller
    {
        private ParserContext db = new ParserContext();

        // POST: Form to select parsers and start parse
        [HttpPost]
        public ActionResult Parse(int[] selectedParts, int OrderId)
        {
            ViewBag.SelectedParts = selectedParts;
            ViewBag.OrderId = OrderId;
            ParsersManager pm = new ParsersManager();
            return View(pm.GetAvailableParsers());
        }

        // POST: Start parse process and return parsed data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StartParse(int[] selectedParts, int OrderId, int[] selectedParsers)
        {
            ViewBag.selectedParts = selectedParts;
            ViewBag.OrderId = OrderId;
            ViewBag.parsers = selectedParsers;
            return View();
        }

        // GET: ParsedDatas
        public ActionResult Index(int OrderId)
        {
            ViewBag.OrderId = OrderId;
            return View(db.ParsedDatas.ToList());
        }

        // GET: ParsedDatas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ParsedData parsedData = db.ParsedDatas.Find(id);
            if (parsedData == null)
            {
                return HttpNotFound();
            }
            return View(parsedData);
        }

        // GET: ParsedDatas/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ParsedDatas/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ParseDate,ParserType,Original,Firmname,Description,Statistic,Price,SearchedArtikul,Url")] ParsedData parsedData)
        {
            if (ModelState.IsValid)
            {
                db.ParsedDatas.Add(parsedData);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(parsedData);
        }

        // GET: ParsedDatas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ParsedData parsedData = db.ParsedDatas.Find(id);
            if (parsedData == null)
            {
                return HttpNotFound();
            }
            return View(parsedData);
        }

        // POST: ParsedDatas/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ParseDate,ParserType,Original,Firmname,Description,Statistic,Price,SearchedArtikul,Url")] ParsedData parsedData)
        {
            if (ModelState.IsValid)
            {
                db.Entry(parsedData).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(parsedData);
        }

        // GET: ParsedDatas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ParsedData parsedData = db.ParsedDatas.Find(id);
            if (parsedData == null)
            {
                return HttpNotFound();
            }
            return View(parsedData);
        }

        // POST: ParsedDatas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ParsedData parsedData = db.ParsedDatas.Find(id);
            db.ParsedDatas.Remove(parsedData);
            db.SaveChanges();
            return RedirectToAction("Index");
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
