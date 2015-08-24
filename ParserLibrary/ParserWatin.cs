using System.Diagnostics;
using System;
using System.Collections.Generic;
using WatiN.Core;
using System.Threading;
using System.Text.RegularExpressions;


namespace Vmax44Parser.library
{
    
    abstract public class ParserWatin : Parser
    {
        protected IE browser { get; set; }

        protected PTypeEnum previousPageType;

        protected List<PType> pagesType;
        protected string strEndOfPage;

        protected string manufacturer { get; set; }
        
        public ParserWatin()
            : base()
        {
            this.manufacturer = "";
            browser = new IE();
            this.previousPageType = PTypeEnum.unknownPage;
        }

        public abstract void Login(string filePasswords = "pass.xlsx");

        public virtual bool isPageType(PTypeEnum type)
        {

            bool result = false;

            foreach (PType p in pagesType)
            {
                if (p.pageType == type)
                {
                    result = p.IsThis(PageHtml, PageHtmlDocument);
                    break;
                }
            }
            return result;
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
                    //log("Trying to get HTML ...");
                    result = this.browser.Html;
                    if (result == null)
                        wasException = true;
                }
                catch (Exception e)
                {
                    wasException = true;
                    //log("Getting HTML - Exception - " + e.ToString());
                }
            } while (wasException);
            //log("Success getting HTML");
            return result;
        }

        protected override PTypeEnum getCurrentPageType()
        {
            PTypeEnum page = PTypeEnum.unknownPage;

            foreach (PType p in pagesType)
            {
                if (p.IsThis(this.PageHtml, this.PageHtmlDocument))
                {
                    page = p.pageType;
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
            //log("Преобразование цены - " + price_s + " = ");
            s = price_s.Replace(",", ".");  //меняем все запятые на точки
            s = Regex.Replace(s, @"[^0-9.]", ""); //удаляем все символы кроме цифр и точек
            s = Regex.Match(s, @"[0-9.]+\.[0-9]+").ToString(); //удаляем все символы кроме цифр и точек
            if (Decimal.TryParse(s, out price))
            {
                //log("Преобразование цены прошло успешно");
            }
            else
            {
                //log("Ошибка при преобразовании цены");
            }
            //log(price.ToString());
            return price;
        }

        /// <summary>
        /// Функция ожидает загрузку страницы в течение timeout. В процессе ожидания определяется тип страницы.
        /// </summary>
        /// <param name="timeout">Время в мс, в течение которого будет ожидаться загрузка страницы</param>
        /// <param name="step">Интервал времени, через который будут осуществляться попытки определения типа загруженной страницы</param>
        /// <returns>
        /// Тип загруженной страницы. Если не обнаружено ни одного совпадения с известными типами страниц, 
        /// или прошло время больше чем timeout, возвращается тип страницы UnknownPage
        /// </returns>
        protected PTypeEnum WaitUntilPageLoaded(int timeout = 15000, int step = 100)
        {
            int i = 0;
            PTypeEnum pageType;

            log("Предыдущая страница - " + this.previousPageType);
            do //
            {
                this.PageHtml = this.GetHTML();
                this.PageHtmlDocument.LoadHtml(PageHtml);

                pageType = getCurrentPageType();

                if ((pageType == PTypeEnum.unknownPage) || (pageType == previousPageType))
                {
                    log("   Загрузка страницы - ждем " + step + " мс...");
                    Thread.Sleep(step);
                    i += step;
                }
            } while ((i < timeout) && ((pageType == PTypeEnum.unknownPage) || (pageType == previousPageType)));
            if (i < timeout)
                log("   Страница загрузилась");
            else
            {
                log("   Загрузка страницы - таймаут. Считаем, что загрузилась неизвестная страница");
                pageType = PTypeEnum.unknownPage;
                //throw new TimeoutException("Таймаут во время загрузки страницы");
            }
            log("Осуществлен переход к странице " + pageType);
            this.previousPageType = pageType;
            return pageType;
        }

        //to delete
        public bool ClickAndWaitFinish(WatiN.Core.Constraints.Constraint elem)
        {
            return ClickAndWaitText(elem, this.strEndOfPage);
        }

        //to delete
        public bool ClickAndWaitText(WatiN.Core.Constraints.Constraint elem, string textToWait)
        {
            this.browser.Element(elem).ClickNoWait();
            //WaitUntilContainsText(textToWait);
            return WaitText(textToWait);
        }

        //to delete
        public bool WaitFinish()
        {
            return WaitText(this.strEndOfPage);
        }

        // to delete
        public bool WaitUntilText(string textToWaitUntil, int timeout = 15000, int step = 100)
        {
            int i = 0;
            Thread.Sleep(step);
            while (i < timeout) //
            {
                if (!this.GetHTML().Contains(textToWaitUntil))
                    break;
                //log("Ждем " + step + " мс...");
                Thread.Sleep(step);
                i += step;
            }
            //if (i < timeout)
            //log("Текста нет -" + textToWaitUntil);
            //else
            //log("таймаут " + textToWaitUntil);

            return (i < timeout); //Если текст найден, возвращаем true, если таймаут - false

        }

        //to delete
        public bool WaitText(string textToWait, int timeout = 15000, int step = 100)
        {
            int i = 0;
            Thread.Sleep(step);
            while (i < timeout) //
            {
                if (this.GetHTML().Contains(textToWait))
                    break;
                //log("Ждем " + step + " мс...");
                Thread.Sleep(step);
                i += step;
            }
            //if (i < timeout)
            //log("найдено " + textToWait);
            //else
            //log("таймаут " + textToWait);

            return (i < timeout); //Если текст найден, возвращаем true, если таймаут - false
        }


        //to delete
        public bool GoToAndWaitText(string Url, string textToWait)
        {
            this.browser.GoToNoWait(Url);
            return WaitText(textToWait);
        }

        //to delete
        public bool GoToAndWaitFinish(string Url)
        {
            this.browser.GoToNoWait(Url);
            return WaitText(this.strEndOfPage);
        }


        override public void Dispose()
        {
            if (this.browser != null)
            {
                this.browser.Close();
            }
        }
    }
}
