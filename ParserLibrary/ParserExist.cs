using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Vmax44Parser.library;
using Vmax44ParserConnectedLayer;

namespace ParserLibrary
{
    class ParserExist : parserWebClient
    {
        public ParserExist()
            : base()
        {
            this.ParserType = "exist.ru";
            this.SignedIn = this.Login();
            pagesType = new List<PType>
            {   
                new PType(){pageType=PTypeEnum.loginPage, 
                    attribute="id="+'\u0022'+"login"+'\u0022',
                    DomNotContainsElement="//a[@id='hlExit']"},
                new PType(){pageType=PTypeEnum.noResultPage,
                    DomContainsElement="//span[@id='lblError']"},
                new PType(){pageType=PTypeEnum.selectManufacturerPage,
                    DomContainsElement="//tr[starts-with(@onclick,'getpr')]"},
                new PType(){pageType=PTypeEnum.dataPage, 
                    DomContainsElement="//tr[@bl]"},
            };
        }

        private bool Login()
        {
            string s = client.DownloadString("http://exist.ru");
            var values = new System.Collections.Specialized.NameValueCollection();
            values.Add("login", "login");
            values.Add("pass", "password");
            GotoPOST("http://exist.ru/Profile/Login.aspx", values);
            return PageHtmlDocument.DocumentNode.SelectSingleNode("//a[@id='hlExit']") != null;
        }

        public override Vmax44ParserConnectedLayer.ParsedDataCollection detailParse(string detailCode)
        {
            var values = new System.Collections.Specialized.NameValueCollection();
            values.Add("sr", "-4");
            values.Add("pcode", detailCode);
            Goto("http://exist.ru/price.aspx",values);
            return Commander(detailCode); 
        }

        private Vmax44ParserConnectedLayer.ParsedDataCollection Commander(string detailCode)
        {
            Vmax44ParserConnectedLayer.ParsedDataCollection res = new Vmax44ParserConnectedLayer.ParsedDataCollection();
            this.Error = 0;
            this.ErrorMessage = "";
            bool done = false;
            while (!done)
            {
                switch (getCurrentPageType())
                {
                    case PTypeEnum.loginPage:
                        this.Error = 2;
                        this.ErrorMessage = "Требуется Login";
                        done = true;
                        break;
                    case PTypeEnum.selectManufacturerPage:
                        parseManufacturers();
                        if (this.StringsToSelect.ContainsKey(this.selectedString))
                        {
                            clickManufacturer(this.StringsToSelect[this.selectedString],detailCode);
                        }
                        else
                        {
                            this.Error = 1;
                            this.ErrorMessage = "Требуется выбор производителя";
                            done = true;
                        }
                        break;
                    case PTypeEnum.dataPage:
                        res = ParseData();
                        done = true;
                        break;
                    case PTypeEnum.noResultPage:
                        done = true;
                        break;
                    default:
                        this.Error = 3;
                        this.ErrorMessage = "Неизвестная ошибка";
                        done = true;
                        break;
                }
            }
            return res;
        }

        private void clickManufacturer(string p, string detailCode)
        {
            this.Goto(p);
        }

        private void parseManufacturers()
        {
            HtmlNodeCollection names = this.PageHtmlDocument.DocumentNode.SelectNodes("//tr[@onclick]");
            string name = "";
            string path = "";
            this.StringsToSelect.Clear();
            foreach (var n in names)
            {
                var nname = n.SelectSingleNode("td[1]/div[@class='firmname']");
                name = WebUtility.HtmlDecode(nname.InnerText);
                string npath = WebUtility.HtmlDecode(n.GetAttributeValue("onclick", ""));
                int first=npath.IndexOf("'");
                int second = npath.IndexOf("'", first + 1);
                path = "";
                try
                {
                    path = "http://exist.ru/price.aspx?pid=" + WebUtility.UrlEncode(npath.Substring(first + 1, second - first - 1));
                    this.StringsToSelect.Add(name, path);
                }
                catch { }
            }
        }

        private Vmax44ParserConnectedLayer.ParsedDataCollection ParseData()
        {
            string orig = "", firmname = "", art = "", desc = "", statistic = "", price_s = "";
            decimal price;
            ParsedDataCollection res = new ParsedDataCollection(); 
            var tableselected = this.PageHtmlDocument.DocumentNode.SelectNodes("//tr[@bl]");
            if (tableselected == null)
                return res;

            var table = tableselected.ToArray();
            foreach (var item in table)
            {
                var tmp = item.GetAttributeValue("bl", "");

                if (tmp == "-10")
                    orig = "original";
                else
                    orig = "zamenitel";
                PutParsedData(item, ".//div[@class='firmname']", ref firmname);
                PutParsedData(item, ".//div[@class='art']", ref art);
                PutParsedData(item, ".//div[@class='descblock']", ref desc);
                PutParsedData(item, ".//td[@class='statis mobile_h']", ref statistic);
                PutParsedData(item, ".//td[@class='price']", ref price_s);

                price = summParse(price_s);
                ParsedData original = new ParsedData();
                original.orig = orig;
                original.firmname = firmname;
                original.art = art;
                original.desc = desc;
                original.statistic = statistic;
                original.price = price;
                original.parsertype = this.GetParserType();
                original.url = "";
                res.Add(original);
            }
            return res;
        }
    }
}
