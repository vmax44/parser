using System.Diagnostics;
using System;
using System.Collections.Generic;
using WatiN.Core;
using System.Threading;
using Vmax44ParserConnectedLayer;
using System.Text.RegularExpressions;


namespace Vmax44Parser
{

    public enum PTypeEnum
    {
        loginPage, basketPage, searchPage, startPage,
        selectManufacturerPage, dataPage, noResultPage,noResultPage1,
        unknownPage
    }

    public class PType
    {
        public PTypeEnum pageType;
        public string attribute;
        public string DomContainsElement;
        public string DomContainsElementText;
        public string DomNotContainsElement;

        public PType()
        {
            this.pageType = PTypeEnum.unknownPage;
            this.attribute = string.Empty;
            this.DomContainsElement = "";
            this.DomContainsElementText = "";
            this.DomNotContainsElement = "";
        }
    }

    public delegate string SelectFromStringList(List<string> items);

    abstract public class Parser : IE
    {
        public List<PType> pagesType;
        public string strEndOfPage;
        public string ParserType;
        public string manufacturer { get; set; }

        public SelectFromStringList GetSelectedManufacturer;

        public Parser() : base()
        {
            this.manufacturer = "";
        }

        public abstract void Login(string filePasswords = "pass.xlsx");

        //public abstract ParsedDataCollection ParsePage();

        //public abstract void ClickManufacturer(string manufacturer);

        public abstract ParsedDataCollection detailParse(string detailCode);

        public virtual bool isPageType(PTypeEnum type)
        {
            string attribute = "";
            string DomContains = "";
            string DomNotContains = "";
            string DomContainsText = "";

            foreach (PType p in pagesType)
            {
                if (p.pageType == type)
                {
                    attribute = p.attribute;
                    DomContains = p.DomContainsElement;
                    DomContainsText = p.DomContainsElementText;
                    DomNotContains = p.DomNotContainsElement;
                    break;
                }
            }
            if ((attribute != "") && (this.GetHTML().Contains(attribute)))
                return true;
            else
                return false;
        }

        public string GetHTML()
        {
            bool wasException;
            string result = "";
            do
            {
                try
                {
                    wasException = false;
                    log("Trying to get HTML ...");
                    result = this.Html;
                    if (result == null)
                        throw new NullReferenceException();
                }
                catch (Exception e)
                {
                    wasException = true;
                    log("Getting HTML - Exception - " + e.ToString());
                }
            } while (wasException);
            log("Success getting HTML");
            return result;
        }

        public virtual PTypeEnum getCurrentPageType()
        {
            PTypeEnum page = PTypeEnum.unknownPage;
            bool result_attribute = false;
            bool result_DomContainsElement = false;
            bool result_DomNotContainsElement = false;
            bool result_DomContainsElementText = false;

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

            string pageHtml = this.GetHTML();

            doc.LoadHtml(pageHtml);

            foreach (PType p in pagesType)
            {
                //attribute
                if (p.attribute == "" || pageHtml.Contains(p.attribute)) // если атрибут не задан, или на странице содержится заданный атрибутом
                {                                                        // текст, считаем, что страница подходит
                    result_attribute = true;
                }

                if(p.DomContainsElement=="" || doc.DocumentNode.SelectNodes(p.DomContainsElement)!=null)
                {
                    result_DomContainsElement = true;
                }

                if(p.DomContainsElementText=="" && p.DomContainsElement=="")
                {
                    result_DomContainsElementText = true;
                }
                else
                {
                    var elem = doc.DocumentNode.SelectSingleNode(p.DomContainsElement);
                    if(elem!=null && elem.InnerText.Contains(p.DomContainsElementText))
                    {
                        result_DomContainsElementText = true;
                    }
                }

                if(p.DomNotContainsElement=="" || doc.DocumentNode.SelectNodes(p.DomNotContainsElement)==null)
                {
                    result_DomNotContainsElement = true;
                }

                if(result_attribute && result_DomContainsElement && result_DomContainsElementText && result_DomNotContainsElement)
                {
                    page = p.pageType;
                    break;
                }
            }
            return page;
        }

        /// <summary>
        /// Преобразует число, переданное в строке совместно с нецифровыми символами и возвращает в формате 
        /// decimal
        /// </summary>
        /// <param name="price_s">Строка, содержащая число</param>
        /// <returns>Число в формате decimal</returns>
        protected decimal summParse(string price_s)
        {
            decimal price;
            string s;
            log("Преобразование цены - " + price_s + " = ");
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

        public bool ClickAndWaitFinish(WatiN.Core.Constraints.Constraint elem)
        {
            return ClickAndWaitText(elem, this.strEndOfPage);
        }

        public bool ClickAndWaitText(WatiN.Core.Constraints.Constraint elem, string textToWait)
        {
            Button(elem).ClickNoWait();
            //WaitUntilContainsText(textToWait);
            return WaitText(textToWait);
        }

        public bool ClickAndWaitUntilText(WatiN.Core.Constraints.Constraint elem, string textToWaitUntil)
        {
            Button(elem).ClickNoWait();
            return WaitUntilText(textToWaitUntil);
        }

        static public void log(string str)
        {
            Trace.WriteLine(str);
        }

        public bool WaitFinish()
        {
            return WaitText(this.strEndOfPage);
        }

        public bool WaitUntilText(string textToWaitUntil, int timeout = 15000, int step = 100)
        {
            int i = 0;
            Thread.Sleep(step);
            while (i < timeout) //
            {
                if (!this.GetHTML().Contains(textToWaitUntil))
                    break;
                log("Ждем " + step + " мс...");
                Thread.Sleep(step);
                i += step;
            }
            if (i < timeout)
                log("Текста нет -" + textToWaitUntil);
            else
                log("таймаут " + textToWaitUntil);

            return (i < timeout); //Если текст найден, возвращаем true, если таймаут - false

        }

        public bool WaitText(string textToWait, int timeout = 15000, int step = 100)
        {
            int i = 0;
            Thread.Sleep(step);
            while (i < timeout) //
            {
                if (this.GetHTML().Contains(textToWait))
                    break;
                log("Ждем " + step + " мс...");
                Thread.Sleep(step);
                i += step;
            }
            if (i < timeout)
                log("найдено " + textToWait);
            else
                log("таймаут " + textToWait);

            return (i < timeout); //Если текст найден, возвращаем true, если таймаут - false
        }



        public bool GoToAndWaitText(string Url, string textToWait)
        {
            GoToNoWait(Url);
            return WaitText(textToWait);
        }

        public bool GoToAndWaitFinish(string Url)
        {
            GoToNoWait(Url);
            return WaitText(this.strEndOfPage);
        }

    }
}
