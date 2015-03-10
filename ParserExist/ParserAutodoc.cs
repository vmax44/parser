using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Vmax44ParserConnectedLayer;
using WatiN.Core;
using Excel = Microsoft.Office.Interop.Excel;

namespace Vmax44Parser
{
    public class ParserAutodoc : Parser
    {
        public ParserAutodoc(string f="pass.xlsx") : base()
        {
            this.ParserType = "Autodoc.ru";

            pagesType = new List<PType>
            {   
                new PType(){pageType=PTypeEnum.loginPage, attribute="Войти"},
                new PType(){pageType=PTypeEnum.noResultPage, attribute="Нет предложений по этому номеру"},
                new PType(){pageType=PTypeEnum.noResultPage1,DomContainsElement="//span[@id='span_zamen']",
                                      DomContainsElementText="По запрошенному Вами номеру нет предложений"},
                new PType(){pageType=PTypeEnum.dataPage, attribute="Официальные замены выделены синим цветом"},
                new PType(){pageType=PTypeEnum.selectManufacturerPage,attribute="Выберите возможного производителя для номера"},
                new PType(){pageType=PTypeEnum.startPage, attribute="Выход"},
                new PType(){pageType=PTypeEnum.searchPage, attribute="name=\"Article\" type=\"text\" value=\"Артикул детали\"/>"}
            };

            strEndOfPage = "Оригинальные каталоги автозапчастей";
            GoToAndWaitFinish("http://www.autodoc.ru/");
        }

        public override void Login(string filePasswords = "pass.xlsx")
        {
            if (isPageType(PTypeEnum.loginPage))
            {
                throw new Exception("Сайт autodoc.ru требует авторизации!");
                //Excel.Application passwords = new Excel.Application();
                //passwords.Workbooks.Open(Path.IsPathRooted(filePasswords) ? filePasswords : Path.Combine(Application.StartupPath, filePasswords));
                //string tmp = this.Html;
                ////bool tmpbool = tmp.Contains("id=\"login\"");
                //this.Element(Find.ByClass("logon")).ClickNoWait();
                
                ////WaitText("id=\"username\"");
                //var login = this.TextField(Find.ById("UserName"));
                ////login.Click();
                
                //login.TypeText(passwords.Range["B2"].Value);
                //TextField password = this.TextField(Find.ById("Password"));
                ////password.Click();
                //password.TypeText(passwords.Range["B3"].Value);
                //passwords.Quit();

                //ClickAndWaitFinish(Find.ById("submit_logon_page"));
                //WaitUntilText(">Войти</a>");

            }

        }
        
        public override ParsedDataCollection detailParse(string detailCode)
        {
            bool done = false;
            ParsedDataCollection result = new ParsedDataCollection();
            PTypeEnum previousPageType = PTypeEnum.unknownPage;
            while (!done)
            {
                PTypeEnum pageType = this.getCurrentPageType();
                if(pageType==previousPageType)
                {
                    throw new Exception("Зацикливание - страница " + pageType.ToString());
                }
                previousPageType = pageType;

                switch (pageType)
                {

                    case PTypeEnum.selectManufacturerPage:
                        if (manufacturer == "")
                            manufacturer = SelectManufacturer();
                        this.ClickManufacturer(manufacturer);
                        var timestart = System.DateTime.Now;
                        this.Element(Find.ById("gridDetails")).WaitUntilExists();
                        var timefinish = System.DateTime.Now;

                        log("выбрали и кликнули по " + manufacturer);
                        log("От клика до перехода на страницу с деталями прошло " + (timefinish - timestart).ToString());
                        break;

                    case PTypeEnum.noResultPage:
                    case PTypeEnum.noResultPage1:
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
                            parsertype = this.ParserType
                        });
                        this.GoToAndWaitFinish("http://www.autodoc.ru");
                        Thread.Sleep(100);
                        done = true;
                        break;

                    case PTypeEnum.dataPage:
                        this.Element(Find.ById("gridDetails")).WaitUntilExists();
                        result = this.ParsePage();
                        this.GoToNoWait("http://www.autodoc.ru");
                        Thread.Sleep(100);
                        //this.WaitUntilText("Запрошенный артикул");
                        done = true;
                        break;

                    case PTypeEnum.loginPage:
                        this.Login();
                        break;

                    case PTypeEnum.startPage:
                    case PTypeEnum.searchPage:
                        this.TextField(Find.ById("Article")).SetAttributeValue("value", detailCode);
                        this.ClickAndWaitFinish(Find.ById("search_btn"));
                        this.WaitText(detailCode.ToUpper());
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

            this.Element(Find.ById("gridMans")).WaitUntilExists();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            string h = this.Element(Find.ById("gridMans")).OuterHtml;
            doc.LoadHtml(h);
            HtmlNodeCollection node = doc.DocumentNode.SelectNodes("//a");
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
            string orig = "", firmname = "", art = "", desc = "", statistic = "", price_s = "";
            decimal price;

            var table_ = doc.DocumentNode.SelectSingleNode("//table[@id='gridDetails']");
            var table__ = table_.SelectNodes(".//tr[not(@class='gridHeaderStyle3')]");
            var table=table__.ToArray();
            foreach (var item in table)
            {
                orig = "original";

                
                string moredata = string.Empty;
                PutParsedData(item, ".//td[4]/a[1]", ref moredata);
                statistic = moredata.Trim();

                var tmp = item.SelectSingleNode(".//td[4]/a[1]").GetAttributeValue("href","");
                tmp=tmp.Substring(tmp.IndexOf("(")+1,tmp.IndexOf(")")-tmp.IndexOf("(")-1);
                var tmparr = tmp.Split((",").ToCharArray());
                firmname = tmparr[0].Replace("'", "");
                art = tmparr[2].Replace("'", "");
                desc = tmparr[3].Replace("'", "");

                PutParsedData(item, ".//td[1]", ref price_s);
                price = summParse(price_s);

                ParsedData original = new ParsedData();
                original.orig = orig;
                original.firmname = firmname;
                original.art = art;
                original.desc = desc;
                original.statistic = statistic;
                original.price = price;
                original.parsertype = this.ParserType;
                original.url = this.Url;

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

        private string GetNodeText(HtmlAgilityPack.HtmlNode item, string XPath)
        {
            var tmp = item.SelectSingleNode(XPath);
            if (tmp != null)
                return tmp.InnerText;
            else
                return "";
        }

        private void ClickManufacturer(string manufacturer)
        {
            this.Element(Find.ByText(manufacturer).And(Find.ByClass("l_m_A"))).ClickNoWait();
            this.WaitFinish();
            
        }
    }
}
