using HtmlAgilityPack;
using System.Collections.Generic;
using System.Net;
using Vmax44Parser.library;
using Vmax44ParserConnectedLayer;

namespace ParserLibrary
{
    class ParserZapPost : parserWebClient
    {

        public ParserZapPost()
            : base()
        {
            this.ParserType = "zappost.ru";
            this.SignedIn = true;
            pagesType = new List<PType>
            {   
                new PType(){pageType=PTypeEnum.selectManufacturerPage,
                    DomContainsElement="//td[@class='group-brand']"},
                new PType(){pageType=PTypeEnum.dataPage, 
                    DomContainsElement="//table[starts-with(@class,'price-table')]"},
            };
        }

        public override Vmax44ParserConnectedLayer.ParsedDataCollection detailParse(string detailCode)
        {
            var values = new System.Collections.Specialized.NameValueCollection();
            values.Add("search_type", "article");
            values.Add("search", detailCode);
            Goto("http://zappost.ru/search/", values);
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
            HtmlNodeCollection names = this.PageHtmlDocument.DocumentNode.SelectNodes("//table[@class='price-table']/tr[@class]");
            string name = "";
            string path = "";
            this.StringsToSelect.Clear();
            foreach (var n in names)
            {
                name = WebUtility.HtmlDecode(n.SelectSingleNode(".//td[1]").InnerText);
                string npath = WebUtility.HtmlDecode(n.SelectSingleNode(".//td[4]/a").GetAttributeValue("href", ""));
                path = "http://zappost.ru" + npath;
                this.StringsToSelect.Add(name, path);
            }
        }

        private Vmax44ParserConnectedLayer.ParsedDataCollection ParseData()
        {
            ParsedDataCollection dataColl = new ParsedDataCollection();
            decimal price;

            var table = PageHtmlDocument.DocumentNode
                .SelectNodes("//table[starts-with(@class,'price-table')]/tr[@class]");
            foreach (var item in table)
            {
                string price_s = GetNodeText(item, ".//td[5]/span");
                price = summParse(price_s);

                ParsedData original = new ParsedData();
                original.orig = "original";
                original.firmname = GetNodeText(item,".//td[1]");
                original.art = GetNodeText(item,".//td[2]/p[1]");
                original.desc = GetNodeText(item,".//td[2]/p[2]");
                original.statistic = GetNodeText(item,".//td[4]");
                original.price = price;
                original.parsertype = GetParserType();
                original.url = "";

                dataColl.Add(original);
            }
            return dataColl;
        }
    }
}
