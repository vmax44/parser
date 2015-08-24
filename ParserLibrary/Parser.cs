using System;
using System.Collections.Generic;
using Vmax44ParserConnectedLayer;

namespace Vmax44Parser.library
{
    public interface IParser
    {

        /// <summary>
        /// Парсинг страницы
        /// </summary>
        /// <param name="detailCode"></param>
        /// <returns></returns>
        ParsedDataCollection detailParse(string detailCode);
        int getError();
        string getErrorMessage();
        IEnumerable<string> getStringsToSelect();
        void setSelectedString(string s);
        string GetParserType();
        List<string> GetLog();
        void Dispose();
    }

    abstract public class Parser: IDisposable, IParser
    {
        /// <summary>
        /// Содержит строку-название парсера
        /// </summary>
        protected string ParserType = "";

        protected List<string> _log = new List<string>();

        public string GetParserType()
        {
            return ParserType;
        }

        /// <summary>
        /// Содержит исходный код последней загруженной версии страницы
        /// </summary>
        protected string PageHtml { get; set; }

        /// <summary>
        /// Содержит DOM-дерево последней загруженной версии страницы
        /// </summary>
        protected HtmlAgilityPack.HtmlDocument PageHtmlDocument { get; set; }

        public Parser()
        {
            this.PageHtmlDocument = new HtmlAgilityPack.HtmlDocument();
            this.PageHtml = string.Empty;
        }

        /// <summary>
        /// Функция внешнего объекта, позволяющая выбрать строку из переданного списка
        /// </summary>
        public Func<List<string>, string> GetSelectedManufacturer;

        public abstract ParsedDataCollection detailParse(string detailCode);

        /// <summary>
        /// Получает тип последней загруженной версии страницы
        /// </summary>
        /// <returns></returns>
        protected abstract PTypeEnum getCurrentPageType();

        abstract public void Dispose();


        public abstract int getError();

        public abstract string getErrorMessage();

        public abstract IEnumerable<string> getStringsToSelect();

        public abstract void setSelectedString(string s);

        public void log(string s)
        {
            _log.Add(s);
        }

        public List<string> GetLog()
        {
            return _log;
        }

    }
}
