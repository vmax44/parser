using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows;

namespace WindowsFormsApplication1
{
    public partial class Form2 : Form
    {
        public Form2(List<string> items)
        {
            InitializeComponent();
            comboBox1.DataSource = items;
        }

        public string getSelected()
        {
            return (string)comboBox1.SelectedItem;
        }
        
        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }


    }
}
