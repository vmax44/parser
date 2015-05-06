using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vmax44Parser.library
{
    public enum PTypeEnum
    {
        loginPage, basketPage, searchPage, startPage,
        selectManufacturerPage, dataPage, noResultPage, noResultPage1,
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

        public bool IsThis(string Html, HtmlAgilityPack.HtmlDocument doc)
        {
            bool result_attribute = false;
            bool result_DomContainsElement = false;
            bool result_DomNotContainsElement = false;
            bool result_DomContainsElementText = false;
            bool result = false;

            //attribute
            if (this.attribute == "" || Html.Contains(this.attribute)) // если атрибут не задан, или на странице содержится заданный атрибутом
            {                                                        // текст, считаем, что страница подходит
                result_attribute = true;
            }

            if (this.DomContainsElement == "" || doc.DocumentNode.SelectNodes(this.DomContainsElement) != null)
            {
                result_DomContainsElement = true;
            }

            if (this.DomContainsElementText == "" && this.DomContainsElement == "")
            {
                result_DomContainsElementText = true;
            }
            else
            {
                var elem = doc.DocumentNode.SelectSingleNode(this.DomContainsElement);
                if (elem != null && elem.InnerText.Contains(this.DomContainsElementText))
                {
                    result_DomContainsElementText = true;
                }
            }

            if (this.DomNotContainsElement == "" || doc.DocumentNode.SelectNodes(this.DomNotContainsElement) == null)
            {
                result_DomNotContainsElement = true;
            }

            result = result_attribute && result_DomContainsElement && result_DomContainsElementText && result_DomNotContainsElement;
            return result;
        }
    }
}
