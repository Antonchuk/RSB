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
    public partial class from_conn_prop : Form
    {
        private RSBMainForm Parent_form;
        public from_conn_prop(RSBMainForm parent)
        {
            InitializeComponent();
            Parent_form = parent;
        }
        private bool Check_fields()
        {
            //просто проверка, что поля не пустые. ограничение на Port устанволено отдельно
            if (txtbox_database.Text != "" && txtbox_port.Text != "" && txtbox_server.Text != "")
            {
                return true;
            }
            else
            {
                return false;
            }
            
            
        }
        private void Button2_Click(object sender, EventArgs e)
        {
            //проверка на заполненность полей
            if (Check_fields())
            {
                //запись в файл настроек параметров
                Properties.Settings.Default.server = txtbox_server.Text;
                Properties.Settings.Default.database = txtbox_database.Text;
                Properties.Settings.Default.port = Convert.ToInt32(txtbox_port.Text);
                Properties.Settings.Default.Save();
            }
            else
            {
                //пишем сообщение, что у пользователя ничего не получилос и все равно закрываем всё
                MessageBox.Show("Не заполнены одно или несколько полей. Окно будет закрыто, но параметры останутся прежними", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }

            //закрытие формы
            Close();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("На form2 идекс= "+Parent_form.cbox_username.SelectedItem.ToString());
            Parent_form.Focus();
            Dispose();
            Close();
            
        }

        private void Txtbox_port_KeyPress(object sender, KeyPressEventArgs e)
        {
            //в поле только цифры вводятся и клавиша backSpace и DEL
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8 && number != 127)
            {
                e.Handled = true;
            }
        }

        private void From_conn_prop_Load(object sender, EventArgs e)
        {
            //при загрузке подгружаем данные по заполнению из файла настроек
            txtbox_database.Text = Properties.Settings.Default.database.ToString();
            txtbox_port.Text = Properties.Settings.Default.port.ToString();
            txtbox_server.Text = Properties.Settings.Default.server.ToString();
            //подсказка
            ToolTip toolTip1 = new ToolTip();

            // Set up the delays for the ToolTip.
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.SetToolTip(this.txtbox_database, "172.16.0.151");
            toolTip1.SetToolTip(this.txtbox_port, "3306");
            toolTip1.SetToolTip(this.txtbox_server, "test2base");
        }

        
    }
}
