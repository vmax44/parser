using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using System.IO;
using Vmax44ParserConnectedLayer;
using System.Configuration;

using Excel = Microsoft.Office.Interop.Excel;
using WinForm = System.Windows.Forms.Form;



namespace Vmax44Parser
{
    

    public partial class Form1 : WinForm
    {
        public ParsedDataCollection dataBase;

        static public void log(string str)
        {
            Trace.WriteLine(str);
        }

        public Form1()
        {
            InitializeComponent();
            dataBase = new ParsedDataCollection();
            dataCollectionBindingSource.DataSource = dataBase;
            dataGridView1.DataSource = dataCollectionBindingSource;

            OrdersDAL sqlbase=new OrdersDAL();
            sqlbase.OpenConnection(ConfigurationManager.ConnectionStrings[
                "Vmax44Parser.Properties.Settings.vmax44parserConnectionString"].ToString());
            DataTable orders=sqlbase.GetAllOrdersAsDataTable();
            foreach (DataRow row in orders.Rows)
            {
                comboBox1.Items.Add(row.Field<string>("OrderNumber"));
            }
            sqlbase.CloseConnection();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string detailCode = "";
            using (var browser = new ParserAutodoc())
            {
                //browser.Visible = false;
                browser.GetSelectedManufacturer = SelectFromStringList;
                Excel.Application codesEx = new Excel.Application();

                codesEx.Workbooks.Open(Path.Combine(Application.StartupPath, "code.xlsx"));

                var row = 1;
                
                while ((codesEx.Cells[row, 2].Value!=null)&&(codesEx.Cells[row,2].Value!=""))
                {
                    detailCode = codesEx.Cells[row, 2].Value;
                    dataBase.AddRange(browser.detailParse(detailCode));
                    row++;
                }

                codesEx.ActiveWorkbook.Close();
                codesEx.Quit();

                //browser.Close();
                var tmp=dataCollectionBindingSource.ToString();

                dataCollectionBindingSource.ResetBindings(false);
                textBox1.AppendText(dataBase.ToString());
            }
        }

        public String SelectFromStringList(List<string> items)
        {
            Form2 mDialog = new Form2(items);
            mDialog.Owner = this;
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
            gotest();
            Application.Exit();

            // TODO: This line of code loads data into the 'vmax44parserDataSet.ParsedData' table. You can move, or remove it, as needed.
            this.parsedDataTableAdapter.Fill(this.vmax44parserDataSet.ParsedData);
            // TODO: This line of code loads data into the 'vmax44parserDataSet.Orders' table. You can move, or remove it, as needed.
            this.ordersTableAdapter.Fill(this.vmax44parserDataSet.Orders);
        }

        public void gotest()
        {
            using (var browser = new ParserAutodoc())
            {
                try
                {
                    browser.GetSelectedManufacturer = SelectFromStringList;
                    var result=browser.detailParse("Z2156507");
                    MessageBox.Show("Напарсено позиций -" + result.Count + "\r\n" + result.ToString());
                }
                catch (NotImplementedException ex)
                {
                    MessageBox.Show("Функция не доделана -" + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OrdersDAL sqlbase;
            sqlbase = new OrdersDAL();
            sqlbase.OpenConnection(ConfigurationManager.ConnectionStrings[
                "Vmax44Parser.Properties.Settings.vmax44parserConnectionString"].ToString());
            int orderid = sqlbase.LookUpOrderId(comboBox1.SelectedItem.ToString());

            sqlbase.InsertParsedDataCollection(orderid, dataBase);

            sqlbase.CloseConnection();
        }

    }
}
