using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vmax44Parser.library;

namespace ParserLibrary
{
    public class CustomWebClient : WebClient
    {
        private CookieContainer cookieContainer = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = cookieContainer;
                (request as HttpWebRequest).AllowAutoRedirect = true;
                (request as HttpWebRequest).AutomaticDecompression = DecompressionMethods.GZip;
            }
            return request;
        }

        public CookieContainer GetCookie()
        {
            return cookieContainer;
        }

        public void SetCookie(CookieContainer cookie)
        {
            this.cookieContainer = cookie;
        }
    }

    class parserWebClient : Parser
    {
        protected CustomWebClient client = new CustomWebClient();
        protected int Error = 0;
        protected string ErrorMessage = string.Empty;
        protected List<PType> pagesType;
        protected Dictionary<string, string> StringsToSelect = new Dictionary<string, string>();
        protected string selectedString = string.Empty;
        protected bool SignedIn = false;

        public parserWebClient() : base()
        {
            client.Encoding = Encoding.UTF8;
        }

        protected void GotoPOST(string url, System.Collections.Specialized.NameValueCollection data)
        {
            client.Headers.Add(PrepareHeaders());
            client.Encoding = Encoding.UTF8;
            byte[] s = client.UploadValues(url, "POST", data);
            _goto(s);
            log("POST " + url);
        }

        protected void Goto(string url, System.Collections.Specialized.NameValueCollection data = null)
        {
            if (data != null)
            {
                client.QueryString = data;
            }
            else
            {
                client.QueryString = null;
            }
            client.Headers.Add(PrepareHeaders());
            client.Encoding = Encoding.UTF8;
            byte[] s = client.DownloadData(url);
            _goto(s);
            log("GET " + url);
        }

        /// <summary>
        /// Перекодирует полученные данные согласно кодировке в HTTP заголовках или в теге <meta http-equiv>
        /// </summary>
        /// <param name="str">полученные данные, которые необходимо перекодировать в строку</param>
        private void _goto(byte[] str)
        {
            string contentType = client.ResponseHeaders[HttpResponseHeader.ContentType];
            Regex reCharset = new Regex(@"charset=([0-9a-zA-Z-]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            Encoding responseEncoding = null;
            string charset = null;
            if (contentType != null)
            {
                Match m = reCharset.Match(contentType);
                if (m.Success) charset = m.Groups[1].Value;
            }
            if (charset == null)
            {
                // Заголовок Content-Type отсуствует или не содержит имя кодировки
                // Ищем тег <meta http-equiv="Content-Type" content="text/html; charset=NAME" />
                // TODO: Нужно чтобы поиск шел только внутри мета-тега, если он есть
                string begin = Encoding.ASCII.GetString(str, 0, Math.Min(str.Length, 4000));
                Match m = reCharset.Match(begin);
                if (m.Success) charset = m.Groups[1].Value;
            }
            if (charset != null)
            {
                try { responseEncoding = Encoding.GetEncoding(charset); }
                catch (ArgumentException) { }
            }

            responseEncoding = responseEncoding ?? Encoding.UTF8;
            string s = responseEncoding.GetString(str);
            
            SaveHtmlDocument(s);
        }

        private NameValueCollection PrepareHeaders()
        {
            NameValueCollection c = new NameValueCollection();
            c.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            c.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
            c.Add("Accept-Charset", "utf-8");
            return c;
        }

        private void SaveHtmlDocument(string s)
        {
            this.PageHtml = s;
            this.PageHtmlDocument.LoadHtml(s);
        }

        public override Vmax44ParserConnectedLayer.ParsedDataCollection detailParse(string detailCode)
        {
            throw new NotImplementedException();
        }

        protected void PutParsedData(HtmlAgilityPack.HtmlNode item, string XPath, ref string to)
        {
            string tmp = GetNodeText(item, XPath);
            if (tmp != "")
                to = tmp;
        }

        protected string GetNodeText(HtmlAgilityPack.HtmlNode item, string XPath)
        {
            var tmp = item.SelectSingleNode(XPath);
            if (tmp != null)
            {
                var s = HtmlAgilityPack.HtmlEntity.DeEntitize(tmp.InnerText);
                return s.Trim();
            } else
                return "";
        }

        protected decimal summParse(string price_s)
        {
            decimal price;
            string s;
            var separator = NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator;
            //log("Преобразование цены - " + price_s + " = ");
            s = price_s.Replace(",", separator);  //меняем все запятые на символ разделителя целой и дробной части
            s = Regex.Replace(s, @"[^0-9" + @separator + @"]", ""); //удаляем все символы кроме цифр и точек
            s = Regex.Match(s, @"[0-9" + @separator + @"]+\" + @separator + @"[0-9]+").ToString(); //удаляем все символы кроме цифр и точек
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

        protected override PTypeEnum getCurrentPageType()
        {
            PTypeEnum page = PTypeEnum.unknownPage;

            foreach (PType p in pagesType)
            {
                if (p.IsThis(this.PageHtml, this.PageHtmlDocument))
                {
                    page = p.pageType;
                    break;
                }
            }
            return page;
        }

        public override void Dispose()
        {
            client.Dispose();
        }

        public override int getError()
        {
            return this.Error;
        }

        public override string getErrorMessage()
        {
            return this.ErrorMessage;
        }    
        public override IEnumerable<string> getStringsToSelect()
        {
            return StringsToSelect.Keys.ToList();
        }

        public override void setSelectedString(string s)
        {
            this.selectedString = s;
        }

        public bool IsSignedIn()
        {
            return SignedIn;
        }
    }
}
