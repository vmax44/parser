using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Vmax44Parser.library;
using Vmax44ParserConnectedLayer;

namespace ParserLibrary
{
    class ParserAutodoc : parserWebClient
    {

        public ParserAutodoc()
            : base()
        {
            this.ParserType = "autodoc.ru";
            this.SignedIn = this.Login();
            pagesType = new List<PType>
            {   
                new PType(){pageType=PTypeEnum.loginPage, 
                    DomContainsElement="//a[@class='logon']"},
                new PType(){pageType=PTypeEnum.selectManufacturerPage,
                    DomContainsElement="//table[@id='gridMans']"},
                new PType(){pageType=PTypeEnum.dataPage, 
                    DomContainsElement="//table[@id='gridDetails']"},
            };
        }

        private bool Login()
        {
            Goto("http://www.autodoc.ru");
            var values = new System.Collections.Specialized.NameValueCollection();
            values.Add("UserName", "login");
            values.Add("Password", "password");
            values.Add("RememberMe", "false");
            values.Add("returnUrl", "/");
            GotoPOST("http://www.autodoc.ru/Account/LogOn", values);
            return PageHtmlDocument.DocumentNode.SelectSingleNode("//a[@class='log-out']") != null;
        }

        public override Vmax44ParserConnectedLayer.ParsedDataCollection detailParse(string detailCode)
        {
            var values = new System.Collections.Specialized.NameValueCollection();
            values.Add("Article", detailCode);
            values.Add("analog", "false");
            Goto("http://www.autodoc.ru/Price/Index", values);
            //this.selectedString = "Mitsubishi";
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
                    default:
                        this.Error = 3;
                        this.ErrorMessage = "Неизвестная страница";
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
            HtmlNodeCollection names = this.PageHtmlDocument.DocumentNode.SelectNodes("//table[@id='gridMans']/tbody/tr/td[1]/a");
            string name = "";
            string path = "";
            this.StringsToSelect.Clear();
            foreach (var n in names)
            {
                name = WebUtility.HtmlDecode(n.InnerText);
                string npath = WebUtility.HtmlDecode(n.GetAttributeValue("href", ""));
                path = "http://www.autodoc.ru" + npath;
                this.StringsToSelect.Add(name, path);
            }
        }

        private Vmax44ParserConnectedLayer.ParsedDataCollection ParseData()
        {
            ParsedDataCollection dataColl = new ParsedDataCollection();
            string orig = "", firmname = "", art = "", desc = "", statistic = "", price_s = "";
            decimal price;

            var table = PageHtmlDocument.DocumentNode
                .SelectNodes("//table[@id='gridDetails']/tr[not(@class='gridHeaderStyle3')]");
            foreach (var item in table)
            {
                orig = "original";
                string moredata = string.Empty;
                PutParsedData(item, ".//td[4]/a[1]", ref moredata);
                statistic = moredata.Trim();

                var tmp = item.SelectSingleNode(".//td[4]/a[1]").GetAttributeValue("href", "");
                tmp = tmp.Substring(tmp.IndexOf("(") + 1, tmp.IndexOf(")") - tmp.IndexOf("(") - 1);
                tmp = WebUtility.HtmlDecode(tmp);
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
                original.parsertype = GetParserType();
                original.url = "";

                dataColl.Add(original);
            }
            return dataColl;
        }


    }
}
