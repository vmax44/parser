using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using WatiN.Core;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Threading;

using Excel = Microsoft.Office.Interop.Excel;
using WinForm = System.Windows.Forms.Form;
using WatForm = WatiN.Core.Form;
using WatiN.Core.UtilityClasses;



namespace Vmax44Parser
{
    

    public partial class Form1 : WinForm
    {
        public string manufacturer { get; set; }
        public dataCollection dataBase;

        static public void log(string str)
        {
            Trace.WriteLine(str);
        }

        public Form1()
        {
            InitializeComponent();
            manufacturer = "";
            dataBase = new dataCollection();
            dataCollectionBindingSource.DataSource = dataBase;
            dataGridView1.DataSource = dataCollectionBindingSource;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string detailCode = "";
            using (var browser = new ParserExist())
            {
                browser.Visible = false;
                Excel.Application codesEx = new Excel.Application();

                codesEx.Workbooks.Open(Path.Combine(Application.StartupPath, "code.xlsx"));

                var row = 1;


                while ((codesEx.Cells[row, 2].Value!=null)&&(codesEx.Cells[row,2].Value!=""))
                {
                    detailCode = codesEx.Cells[row, 2].Value;
                    dataBase.AddRange(detailParse(browser, detailCode));
                    row++;
                }

                codesEx.ActiveWorkbook.Close();
                codesEx.Quit();

                browser.Close();
                var tmp=dataCollectionBindingSource.ToString();

                dataCollectionBindingSource.ResetBindings(false);
                textBox1.AppendText(dataBase.ToString());
            }
        }

        private dataCollection detailParse(ParserExist browser, string detailCode)
        {
            bool done = false;
            dataCollection result = new dataCollection();
            while (!done)
            {
                PTypeEnum pageType = browser.getCurrentPageType();
                label1.Text = pageType.ToString();
                log("Перешли на страницу " + pageType.ToString());

                switch (pageType)
                {

                    case PTypeEnum.selectManufacturerPage:
                        if (manufacturer == "")
                            manufacturer = SelectManufacturer(browser);
                        browser.ClickManufacturer(manufacturer);
                        browser.WaitText("bl=\"-10\"");
                        log("выбрали и кликнули по " + manufacturer);
                        break;

                    case PTypeEnum.noResultPage:
                        //result = "no result;;"+detailCode+";;;";
                        result.Add(new data()
                        {
                            orig = "no result",
                            firmname = "",
                            art = detailCode,
                            desc = "",
                            statistic = "",
                            price = 0
                        });
                        browser.GoToAndWaitFinish("http://exist.ru");
                        Thread.Sleep(100);
                        done = true;
                        break;

                    case PTypeEnum.dataPage:
                        result=browser.ParsePage();
                        browser.GoToNoWait("http://exist.ru");
                        Thread.Sleep(100);
                        browser.WaitUntilText("Запрошенный артикул");
                        done = true;
                        break;

                    case PTypeEnum.loginPage:
                        browser.Login();
                        break;

                    case PTypeEnum.startPage:
                    case PTypeEnum.searchPage:
                        browser.TextField(Find.ByName("pcode")).SetAttributeValue("value", detailCode);
                        browser.ClickAndWaitFinish(Find.ByValue("Найти"));
                        browser.WaitText(detailCode);
                        break;

                }
            }
            return result;
        }

        private string SelectManufacturer(Parser browser) 
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(browser.Body.OuterHtml);
            HtmlNodeCollection node = doc.DocumentNode.SelectNodes("//div[@class='firmname']");
            List<string> items = new List<string>();
            foreach (var n in node)
            {
                items.Add(n.InnerText);
            }
            Form2 mDialog = new Form2(items);
            mDialog.Owner=this;
            mDialog.ShowDialog();
            if (mDialog.DialogResult == DialogResult.OK)
            {
                return mDialog.getSelected();
            }
            else
            {
                return "";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

    }
}
