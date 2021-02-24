using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RSB
{
    public partial class New_producer_add : Form
    {
        //private RSBMainForm Parent_form;
        public New_producer_add()
        {
            InitializeComponent();
            //Parent_form = parent;
        }

        private void New_producer_add_Load(object sender, EventArgs e)
        {
            combox_access.Items.Add("1"); // простой
            combox_access.Items.Add("2"); // расширенный
            combox_access.Items.Add("3"); // админ
            combox_access.Text = "1";
            //txtbox_surname.Text = Properties.Settings.Default.pro_surname;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private bool Check_fields()
        {
            if (txtbox_name.Text != "" && txtbox_surname.Text != "" && combox_access.Text != "")
            {
                return true;
            }
            else return false;
        }
        private void Btn_accept_Click(object sender, EventArgs e)
        {
            if (Check_fields())
            {
                Properties.Settings.Default.pro_access = Convert.ToInt32(combox_access.Text);
                Properties.Settings.Default.pro_name = txtbox_name.Text;
                Properties.Settings.Default.pro_surname = txtbox_surname.Text;
                Properties.Settings.Default.Save();
                Close();
            }
            else MessageBox.Show("Не все поля заполнены");
        }
    }
}
