using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vmax44ParserConnectedLayer;

namespace Vmax44Parser.library
{
    abstract public class Parser: IDisposable
    {
        /// <summary>
        /// Содержит строку-название парсера
        /// </summary>
        public string ParserType;

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
        public Func<List<string>,string> GetSelectedManufacturer; 

        //public SelectFromStringList GetSelectedManufacturer;

        /// <summary>
        /// Парсинг страницы
        /// </summary>
        /// <param name="detailCode"></param>
        /// <returns></returns>
        public abstract ParsedDataCollection detailParse(string detailCode);

        /// <summary>
        /// Получает тип последней загруженной версии страницы
        /// </summary>
        /// <returns></returns>
        protected abstract PTypeEnum getCurrentPageType();

        abstract public void Dispose();
    }
}
