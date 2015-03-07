using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vmax44Parser;

namespace WindowsFormsApplication1
{
    public partial class Test : Form
    {
        //ParserExist browser;

        public Test()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            go();
        }

        public void go()
        {
            using (var browser = new ParserAutodoc())
            {
                browser.detailParse("dfklj");
            }
        }
    }

    
}
