using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WatiN.Core;
using WatiN.Core.DialogHandlers;
using System.Threading;
using System.Configuration;
using System.Data;
using System.Data.Common;


namespace Vmax44Parser
{

    public enum PTypeEnum
    {
        loginPage, basketPage, searchPage, startPage,
        selectManufacturerPage, dataPage, noResultPage,
        unknownPage
    }

    public class PType
    {
        public PTypeEnum pageType;
        public string attribute;
    }





    public class Parser : IE
    {
        public List<PType> pagesType;
        public string strEndOfPage;

        public Parser()
            : base()
        {

        }

        public virtual void Login(string filePasswords = "pass.xlsx") { }

        public virtual bool isPageType(PTypeEnum type)
        {
            string t = "";

            foreach (PType p in pagesType)
            {
                if (p.pageType == type)
                {
                    t = p.attribute;
                    break;
                }
            }
            if ((t != "") && (this.GetHTML().Contains(t)))
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
            string pageHtml = this.GetHTML();

            foreach (PType p in pagesType)
            {
                if (pageHtml.Contains(p.attribute))
                {
                    page = p.pageType;
                    break;
                }
            }
            return page;
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
