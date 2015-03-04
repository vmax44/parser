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
    class ParserAutodoc : Parser
    {
        public ParserAutodoc(string f="pass.xlsx") : base()
        {
            this.ParserType = "Autodoc.ru";

            pagesType = new List<PType>
            {   
                new PType(){pageType=PTypeEnum.loginPage, attribute=">Войти</a>"},
                new PType(){pageType=PTypeEnum.noResultPage, attribute="Нет предложений по этому номеру"},
                new PType(){pageType=PTypeEnum.dataPage, attribute="Официальные замены выделены синим цветом"},
                new PType(){pageType=PTypeEnum.selectManufacturerPage,attribute="Выберите возможного производителя для номера"},
                new PType(){pageType=PTypeEnum.startPage, attribute="href=\"/Account/LogOut?returnUrl=%2F\">Выход</a>"},
                new PType(){pageType=PTypeEnum.searchPage, attribute="name=\"Article\" type=\"text\" value=\"Артикул детали\"/>"}
            };

            strEndOfPage = "Оригинальные каталоги автозапчастей";
            GoToAndWaitFinish("http://www.autodoc.ru/");
        }

        public override void Login(string filePasswords = "pass.xlsx")
        {
            if (isPageType(PTypeEnum.loginPage))
            {
                Excel.Application passwords = new Excel.Application();
                passwords.Workbooks.Open(Path.IsPathRooted(filePasswords) ? filePasswords : Path.Combine(Application.StartupPath, filePasswords));
                string tmp = this.Html;
                //bool tmpbool = tmp.Contains("id=\"login\"");
                this.Element(Find.ByClass("logon")).ClickNoWait();
                WaitText("id=\"username\"");
                TextField(Find.ByName("UserName")).SetAttributeValue("value", passwords.Range["B2"].Value);
                TextField(Find.ByName("Password")).SetAttributeValue("value", passwords.Range["B3"].Value);
                passwords.Quit();

                ClickAndWaitFinish(Find.ById("submit_logon_page"));
                WaitUntilText(">Войти</a>");

            }

        }
        
        public override ParsedDataCollection detailParse(string detailCode)
        {
            bool done = false;
            ParsedDataCollection result = new ParsedDataCollection();
            while (!done)
            {
                PTypeEnum pageType = this.getCurrentPageType();

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
                            parsertype = this.ParserType
                        });
                        this.GoToAndWaitFinish("http://www.autodoc.ru");
                        Thread.Sleep(100);
                        done = true;
                        break;

                    case PTypeEnum.dataPage:
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
                        this.TextField(Find.ByName("pcode")).SetAttributeValue("value", detailCode);
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
            throw new NotImplementedException();
        }
        

        private ParsedDataCollection ParsePage()
        {
            throw new NotImplementedException();
        }

        private void ClickManufacturer(string manufacturer)
        {
            throw new NotImplementedException();
        }
    }
}
