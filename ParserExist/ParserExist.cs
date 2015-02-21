using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WatiN.Core;
using WatForm = WatiN.Core.Form;
using System.IO;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using System.Text.RegularExpressions;

namespace Vmax44Parser
{
    public class ParserExist : Parser
    {
        public ParserExist(string f = "pass.xlsx")
            : base()
        {

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

        public override void Login(string filePasswords = "pass.xlsx")
        {
            if (isPageType(PTypeEnum.loginPage))
            {
                Excel.Application passwords = new Excel.Application();
                passwords.Workbooks.Open(Path.IsPathRooted(filePasswords) ? filePasswords : Path.Combine(Application.StartupPath, filePasswords));
                string tmp = this.Html;
                bool tmpbool = tmp.Contains("id=\"login\"");
                TextField(Find.ByName("login")).SetAttributeValue("value", passwords.Range["A2"].Value);
                TextField(Find.ByName("pass")).SetAttributeValue("value", passwords.Range["A3"].Value);
                passwords.Quit();

                ClickAndWaitFinish(Find.ById("btnLogin"));
                WaitUntilText("id=\"login\"");
                
            }
        }

        public dataCollection ParsePage()
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            var h = this.GetHTML();
            doc.LoadHtml(h);
            dataCollection dataColl = new dataCollection();
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
                data original = new data();
                original.orig = orig;
                original.firmname = firmname;
                original.art = art;
                original.desc = desc;
                original.statistic = statistic;
                original.price = price;
                dataColl.Add(original);
            }
            return dataColl;
        }

        public decimal summParse(string price_s)
        {
            decimal price;
            string s;
            log("Преобразование цены - " + price_s+" = ");
            s = price_s.Replace(",", ".");  //меняем все запятые на точки
            s = Regex.Replace(s, @"[^0-9.]", ""); //удаляем все символы кроме цифр и точек
            s = Regex.Match(s, @"[0-9.]+\.[0-9]+").ToString(); //удаляем все символы кроме цифр и точек
            if (Decimal.TryParse(s, out price))
            {
                log("Преобразование цены прошло успешно");
            }
            else
            {
                log("Ошибка при преобразовании цены");
            }
            log(price.ToString());
            return price;
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

        public void ClickManufacturer(string manufacturer)
        {
            this.Div(Find.ByText(manufacturer).And(Find.ByClass("firmname"))).ClickNoWait();
            this.WaitFinish();
        }

    }
}
