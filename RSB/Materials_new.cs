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
    public partial class Materials_new : Form
    {
        public Materials_new()
        {
            InitializeComponent();
        }

        private void Btn_denay_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Btn_add_new_Click(object sender, EventArgs e)
        {
            if (txtbox_composition.Text != "" && txtbox_name.Text != "")
            {
                Properties.Settings.Default.material_name = txtbox_name.Text;
                Properties.Settings.Default.material_composition = txtbox_composition.Text;
                Properties.Settings.Default.Save();
                Close();
            }
            else MessageBox.Show("Не все поля заполнены");
        }

        private void Materials_new_Load(object sender, EventArgs e)
        {
            txtbox_name.Text = Properties.Settings.Default.material_name;
        }
    }
}
