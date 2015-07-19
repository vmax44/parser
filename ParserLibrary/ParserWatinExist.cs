using System.Collections.Generic;
using System.Linq;
using WatiN.Core;
using System.IO;
//using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using Vmax44ParserConnectedLayer;
using HtmlAgilityPack;
using System.Threading;

namespace Vmax44Parser.library
{
    public class ParserWatinExist : ParserWatin
    {
        public ParserWatinExist(string f = "pass.xlsx")
            : base()
        {
            ParserType = "Exist.ru";

            pagesType = new List<PType>
            {   
                new PType(){pageType=PTypeEnum.loginPage, attribute="id="+'\u0022'+"login"+'\u0022'},
                new PType(){pageType=PTypeEnum.noResultPage, attribute="id=\"lblError\""},
                new PType(){pageType=PTypeEnum.dataPage, attribute="bl=\"-10\""},
                new PType(){pageType=PTypeEnum.selectManufacturerPage,attribute="id=\"hdnTrid\""},
                new PType(){pageType=PTypeEnum.startPage, attribute="id=\"hlExit\""},
                new PType(){pageType=PTypeEnum.searchPage, attribute="id=\"pcode\""}
            };

            strEndOfPage = "Предложить идею";
            GoToAndWaitFinish("http://exist.ru");
        }

        public override void Login(string filePasswords = "C:\\Users\\maxim\\Documents\\Visual Studio 2010\\Projects\\ParserExist\\ParserExist\\bin\\Debug\\pass.xlsx")
        {
            if (isPageType(PTypeEnum.loginPage))
            {
                Excel.Application passwords = new Excel.Application();
                //passwords.Workbooks.Open(Path.IsPathRooted(filePasswords) ? filePasswords : Path.Combine(Application.StartupPath, filePasswords));
                passwords.Workbooks.Open(filePasswords);
                string tmp = this.browser.Html;
                bool tmpbool = tmp.Contains("id=\"login\"");
                this.browser.TextField(Find.ByName("login")).SetAttributeValue("value", passwords.Range["A2"].Value);
                this.browser.TextField(Find.ByName("pass")).SetAttributeValue("value", passwords.Range["A3"].Value);
                passwords.Quit();

                ClickAndWaitFinish(Find.ById("btnLogin"));
                WaitUntilText("id=\"login\"");
                
            }
        }

        public override ParsedDataCollection detailParse(string detailCode)
        {
            bool done = false;
            ParsedDataCollection result = new ParsedDataCollection();
            while (!done)
            {
                PTypeEnum pageType = this.WaitUntilPageLoaded();

                switch (pageType)
                {

                    case PTypeEnum.selectManufacturerPage:
                        if (manufacturer == "")
                            manufacturer = SelectManufacturer();
                        this.ClickManufacturer(manufacturer);
                        this.WaitText("bl=\"-10\"");
                        log("выбрали и кликнули по " + manufacturer);
                        break;

                    case PTypeEnum.noResultPage:
                        //result = "no result;;"+detailCode+";;;";
                        result.Add(new ParsedData()
                        {
                            orig = "no result",
                            firmname = "",
                            art = detailCode,
                            desc = "",
                            statistic = "",
                            price = 0,
                            url = "",
                            parsertype = GetParserType()
                        });
                        this.GoToAndWaitFinish("http://exist.ru");
                        Thread.Sleep(100);
                        done = true;
                        break;

                    case PTypeEnum.dataPage:
                        result = this.ParsePage();
                        this.browser.GoToNoWait("http://exist.ru");
                        Thread.Sleep(100);
                        this.WaitUntilText("Запрошенный артикул");
                        done = true;
                        break;

                    case PTypeEnum.loginPage:
                        this.Login();
                        break;

                    case PTypeEnum.startPage:
                    case PTypeEnum.searchPage:
                        this.browser.TextField(Find.ByName("pcode")).SetAttributeValue("value", detailCode);
                        this.ClickAndWaitFinish(Find.ByValue("Найти"));
                        this.WaitText(detailCode);
                        break;
                }
            }
            foreach (var p in result)
            {
                p.searchedArtikul = detailCode;
            }
            return result;
        }

        private string SelectManufacturer()
        {
            string res = "";

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(this.browser.Body.OuterHtml);
            HtmlNodeCollection node = doc.DocumentNode.SelectNodes("//div[@class='firmname']");
            List<string> items = new List<string>();
            foreach (var n in node)
            {
                items.Add(n.InnerText);
            }

            res = GetSelectedManufacturer(items);
            return res;
        }

        private ParsedDataCollection ParsePage()
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            var h = this.GetHTML();
            doc.LoadHtml(h);
            ParsedDataCollection dataColl = new ParsedDataCollection();
            string orig="",firmname="", art="", desc="", statistic="",price_s="";
            decimal price;
            
            var table = doc.DocumentNode.SelectNodes("//tr[@bl]").ToArray();
            foreach (var item in table)
            {
                var tmp = item.GetAttributeValue("bl", "");

                if (tmp == "-10")
                    orig = "original";
                else
                    orig="zamenitel";
                PutParsedData(item,".//div[@class='firmname']",ref firmname);
                PutParsedData(item,".//div[@class='art']", ref art);
                PutParsedData(item,".//div[@class='descblock']",ref desc);
                PutParsedData(item,".//td[@class='statis mobile_h']",ref statistic);
                PutParsedData(item,".//td[@class='price']",ref price_s);

                price = summParse(price_s);
                ParsedData original = new ParsedData();
                original.orig = orig;
                original.firmname = firmname;
                original.art = art;
                original.desc = desc;
                original.statistic = statistic;
                original.price = price;
                original.parsertype = "Exist.ru";
                original.url = this.browser.Url;
                dataColl.Add(original);
            }
            return dataColl;
        }

        private void PutParsedData(HtmlAgilityPack.HtmlNode item, string XPath, ref string to)
        {
            string tmp = GetNodeText(item, XPath);
            tmp = HtmlAgilityPack.HtmlEntity.DeEntitize(tmp); 
            if (tmp != "")
                to = tmp;
        }

        private string GetNodeText(HtmlAgilityPack.HtmlNode item,string XPath)
        {
            var tmp = item.SelectSingleNode(XPath);
            if (tmp != null)
                return tmp.InnerText;
            else
                return "";
        }

        private void ClickManufacturer(string manufacturer)
        {
            this.browser.Div(Find.ByText(manufacturer).And(Find.ByClass("firmname"))).ClickNoWait();
            this.WaitFinish();
        }

    }
}
