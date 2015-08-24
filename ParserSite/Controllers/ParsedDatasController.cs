using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
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
                ParsedData r = new ParsedData();
                r.Description = p.desc;
                r.Firmname = p.firmname;
                r.Original = p.orig;
                r.ParseDate = DateTime.Now;
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


    public class ParseState
    {
        public int[] selectedParts;
        public int[] selectedParsers;
        public int OrderId;
        public int currentPart;
        public int currentParser;
        public Dictionary<int, List<ParsedData>> parsed;
        public Dictionary<int, string> SelectedStrings = new Dictionary<int, string>();
        public List<string> log = new List<string>();
        public List<string> parserslog = new List<string>();

        public ParseState()
        {
            parsed = new Dictionary<int, List<ParsedData>>();
            currentParser = 0;
            currentPart = 0;
        }
    }

    public class ParsedDatasController : Controller
    {
        private ParserContext db = new ParserContext();

        private void AddError(string s)
        {
            if (TempData["Errors"] == null)
            {
                TempData["Errors"] = new List<string>();
            }
            (TempData["Errors"] as List<string>).Add(s);
        }

        // POST: Form to select parsers and start parse
        [HttpPost]
        public ActionResult Parse(int[] selectedParts, int OrderId)
        {
            if (selectedParts == null)
            {
                AddError("Выберите элементы для парсинга");
                return RedirectToAction("Details", "Orders", new { Id = OrderId });
            }
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
            if (selectedParts == null)
            {
                AddError("Выберите элементы для парсинга");
                return RedirectToAction("Details", "Orders", new { Id = OrderId });
            }
            if (selectedParsers == null)
            {
                AddError("Выберите необходимые парсеры");
                return RedirectToAction("Parse", new { OrderId = OrderId });
            }
            ParseState st = new ParseState()
             {
                 selectedParsers = selectedParsers,
                 selectedParts = selectedParts,
                 OrderId = OrderId
             };
            Session.Add("state", st);
            return StartParse2(string.Empty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StartParse2(string selectedString)
        {
            ParseState st = (ParseState)Session["state"]; //достаем из сессии информацию о текущем состоянии парсинга
            if (st == null)
            {
                return HttpNotFound();
            }
            Order order = db.Orders.Find(st.OrderId);
            ParsersManager pm = new ParsersManager();
            var parsers = pm.GetParsersByArrayIds(st.selectedParsers); //получаем необходимые парсеры
            foreach (KeyValuePair<int, string> kvp in st.SelectedStrings)
            {
                parsers[kvp.Key].setSelectedString(kvp.Value); //сохраняем в парсеры ранее выбранных производителей
            }
            bool juststarted = true; //переменная для того, чтобы определить, что мы только что попали в функцию
            //и необходимо продолжить парсить тем парсером, на котором остановились ранее
            for (; st.currentPart < st.selectedParts.Count(); st.currentPart++)
            {
                var dbpart = db.Parts.Find(st.selectedParts[st.currentPart]);
                if (juststarted)
                {
                    juststarted = false;
                }
                else
                {
                    st.currentParser = 0;
                }
                for (; st.currentParser < st.selectedParsers.Count(); st.currentParser++)
                {
                    if (selectedString != string.Empty)   //если выбрана новая строка
                    {
                        st.SelectedStrings.Add(st.currentParser, selectedString);  //сохраняем ее в объект статуса
                        parsers[st.currentParser].setSelectedString(selectedString); //а также в соответствующий парсер
                        selectedString = string.Empty;
                    }
                    var parsertype = parsers[st.currentParser].GetParserType();
                    var PartNumber = from part in order.Parts
                                     where part.Id == st.selectedParts[st.currentPart]
                                     select part.PartNumber;
                    st.log.Add(string.Format(" парсим деталь {0} на сайте {1}...", PartNumber.First(), parsertype));
                    var r = parsers[st.currentParser].detailParse(PartNumber.First()).ToParsedData();
                    switch (parsers[st.currentParser].getError())
                    {
                        case 1:
                            st.log.Add("!требуется выбор производителя");
                            DisposeParsers(st, parsers);
                            ViewBag.st = st;
                            Session["state"] = st;
                            return PartialView("_SelectManufacturer", parsers[st.currentParser].getStringsToSelect());
                        case 0:
                            st.log.Add(string.Format("Ok. напарсено {0} результатов", r.Count));
                            if (st.parsed.ContainsKey(st.currentPart))
                            {
                                st.parsed[st.currentPart].AddRange(r);
                            }
                            else
                            {
                                st.parsed[st.currentPart] = r;
                            }
                            break;
                        case 2:
                            break;
                        default:
                            break;
                    }
                }
            }
            DisposeParsers(st, parsers);
            ViewBag.st = st;
            return PartialView("StartParse");
        }

        private static void DisposeParsers(ParseState st, List<Vmax44Parser.library.IParser> parsers)
        {
            foreach (var parser in parsers)
            {
                st.parserslog.AddRange(parser.GetLog());
                parser.Dispose();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveParse()
        {
            DateTime saveDate = DateTime.Now;
            ParseState st = (ParseState)Session["state"];
            if (st == null)
            {
                return HttpNotFound();
            }

            Order order = db.Orders.Find(st.OrderId);
            foreach (KeyValuePair<int, List<ParsedData>> p in st.parsed)
            {
                Part part = db.Parts.Find(st.selectedParts[p.Key]);
                foreach (ParsedData entry in p.Value)
                {
                    entry.Order = order;
                    entry.Part = part;
                    entry.ParseDate = saveDate;
                    order.ParsedDatas.Add(entry);
                }
            }
            db.SaveChanges();
            Session.Remove("state");
            return RedirectToAction("Details", "Orders", new { id = st.OrderId });
        }

        // GET: ParsedDatas
        public ActionResult Index(int OrderId)
        {
            ViewBag.OrderId = OrderId;
            return PartialView("_ParsedDataPartial", db.ParsedDatas.ToList());
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
        public ActionResult DeleteConfirmed(IEnumerable<int> selectedDatas, string OrderId)
        {
            if (selectedDatas != null)          //Если выбрана хотя бы одна строка для удаления
            {
                foreach (var id in selectedDatas)
                {
                    ParsedData parsedData = db.ParsedDatas.Find(id);
                    db.ParsedDatas.Remove(parsedData);
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index", new { OrderId = OrderId });
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
