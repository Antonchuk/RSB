using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;

namespace RSB
{
    public partial class RSBMainForm : Form
    {
        Form param_form;
        public Form spec_form;
        public Form reserch_form;
        private int User_access_lvl=3;
        public RSBMainForm()
        {
            InitializeComponent();

        }

        public static MySqlConnection New_connection(string myhost, int myport, string mydatabase, string username, string password)
        {
            // Connection String.
            string connString = "Server=" + myhost + ";Database=" + mydatabase
                + ";port=" + myport + ";User Id=" + username + ";password=" + password;
            MySqlConnection conn = new MySqlConnection(connString);
            return conn;
        }
        private void Btn_exit_Click(object sender, EventArgs e)
        {
            if (spec_form!=null)
            {
                spec_form.Dispose();
            }
            if (reserch_form!=null)
            {
                reserch_form.Dispose();
            }
            if (btn_research.Enabled)
            {
                Properties.Settings.Default.default_username = cbox_username.Text;
                Properties.Settings.Default.default_pass = txtbox_pass.Text;                
                Properties.Settings.Default.Save();
            }
            GC.Collect();
            Close();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            //подгрузка пользователей
            cbox_username.Items.Add(Properties.Settings.Default.default_username);
            //ЗАГЛУШКА
            txtbox_pass.Text = Properties.Settings.Default.default_pass;
            cbox_username.Text = Properties.Settings.Default.default_username;
            //конект к базе 
            //запрос пользователей SELECT User FROM mys ql.user;
            //добавление в комбобокс
        }

        private void Ch_state_buttons(bool ch_state)
        {
            //кнопки открываютили закрывают доступ
            btn_reports.Enabled = ch_state;
            btn_research.Enabled = ch_state;
            btn_specimen.Enabled = ch_state;
        }

        private void Cbox_username_SelectedIndexChanged(object sender, EventArgs e)
        {
            //очистить поле пароля
            txtbox_pass.Text = "";
            //деактивация кнопок доступа
            Ch_state_buttons(false);
            lbl_status.Text = "No connection";
            //ЗАГЛУШКА
            txtbox_pass.Text = Properties.Settings.Default.default_pass;
        }
        private bool Check_log_pass(string username, string pass)
        {
            using (MySqlConnection conn = New_connection(Properties.Settings.Default.server, Properties.Settings.Default.port,
                Properties.Settings.Default.database, username, pass))
            {
                //MessageBox.Show(Properties.Settings.Default.server+ Properties.Settings.Default.port.ToString()+
                //Properties.Settings.Default.database+ username+ pass, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                try
                {
                    conn.Open();                    
                    //conect.Open();
                    string sqlcom = "SELECT access FROM test2base.producers WHERE user_name='"+username+"'";
                    User_access_lvl = 3;
                    using (MySqlCommand comand = new MySqlCommand(sqlcom, conn))
                    {
                        using (MySqlDataReader reader = comand.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (reader[0] != null)
                                    {
                                        //MessageBox.Show("мы здесь вообще появляемс?");
                                        User_access_lvl = Convert.ToInt32(reader[0].ToString());
                                    }
                                    else User_access_lvl = 3;
                                }
                                reader.Close();
                            }
                            else User_access_lvl = 3;
                        }
                        Properties.Settings.Default.user_access_lvl = User_access_lvl;
                        Properties.Settings.Default.Save();
                        //проверить выполнен ли запрос
                        //conect.Close();
                    }
                    lbl_status.Text = "Connected, access lvl = "+ Properties.Settings.Default.user_access_lvl.ToString();
                    conn.Close();
                    return true;
                }
                catch (Exception e)
                {
                    lbl_status.Text = "Error";
                    MessageBox.Show(e.ToString(), "Ошибка, попробуйте поменять последнюю цифру IP-адреса. Варинаты 1 или 2", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                    lbl_status.Text = "No connection";
                    return false;
                }
            }
        }
        private void Tbox_pass_TextChanged(object sender, EventArgs e)
        {


        }

        private void PropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            param_form = new from_conn_prop(this);
            param_form.ShowDialog();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        // посылка письма test

        private static void SendMail_test()
        {
            MailAddress from = new MailAddress("ant4uk@yandex.ru", "Anton");
            MailAddress to = new MailAddress("antchuk@gmail.com");
            MailMessage m = new MailMessage(from, to)
            {
                Subject = "Тест письмо",
                Body = "Письмо-тест 2 работы smtp-клиента"
                //IsBodyHtml = true
            };
            //SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 465) //587 - ошибка в порядке авторизации?
            SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 465)
            {
                //Host= smtpServer,
                //DeliveryMethod =SmtpDeliveryMethod.Network,
                EnableSsl = true,
                Credentials = new NetworkCredential("ant4uk@yandex.ru", "kl87bcd%1ap")                
            };
            smtp.Send(m);
            MessageBox.Show("Письмо отправлено");
            //await smtp.SendMailAsync(m);
            //Console.WriteLine("Письмо отправлено");
        }

        private void Btn_remind_pass_Click(object sender, EventArgs e)
        {
            //отправляем на почту пароль
            //проверяем выбран ли пользователь
            //если да, то запрос базе на пароль этого пользователя????
            MessageBox.Show("Пока что эта функция не работает", "Чтоб вы не переживали", MessageBoxButtons.YesNo, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            //test
            //MessageBox.Show("Начинаем тест");

            //SendMail_test();
            //MessageBox.Show("Завершаем тест");
            //test
        }


        private void Cbox_username_TextChanged(object sender, EventArgs e)
        {
            //при изменении имени пользователя - отмена доступа
            //MessageBox.Show("Проверка по изменению текста!", "test", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Ch_state_buttons(false);
            txtbox_pass.Text = "";
            lbl_status.Text = "No connection";
            //ЗАГЛУШКА
            txtbox_pass.Text = Properties.Settings.Default.default_pass;
        }
        public void Show_specimens_form(int id_spec)
        {
            //шайтан работает, никто не знает как, короче форма вообще не удаляется
            Properties.Settings.Default.main_spec_id = id_spec;
            Properties.Settings.Default.Save();
            if (spec_form != null)
            {
                spec_form.Show();
                spec_form.BringToFront();

            }
            else
            {
                GC.Collect();
                spec_form = new Form_specimens(this);
                spec_form.Show();
            }

        }
        private void Btn_specimen_Click(object sender, EventArgs e)
        {
            //блокируем изменение пароля и логина

            //форма добавления/изменения образцов
            Properties.Settings.Default.default_username = cbox_username.Text;
            Properties.Settings.Default.default_pass = txtbox_pass.Text;
            Properties.Settings.Default.Save();
            Show_specimens_form(-1);
        }



        private void Btn_connect_Click(object sender, EventArgs e)
        {
            //проверка что выбран пользователь            
            if (cbox_username.Text != "")
            {
                //проверка что пароль верен
                //заглушка
                bool ch_pass;
                ch_pass = Check_log_pass(cbox_username.Text, txtbox_pass.Text);
                //ch_pass = true;
                if (ch_pass)
                {
                    //если да, то открытие доступа кнопок доступа
                    Ch_state_buttons(true);
                }
                else
                {
                    Ch_state_buttons(false);
                    MessageBox.Show("Попробуйте поменять последнюю цифру IP адреса с 2 на 1 или наоборот.\n Если не помогло, то значит у вас более серьезные проблемы", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Ch_state_buttons(false);
                MessageBox.Show("Пользователь не выбран", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RSBMainForm_Load(object sender, EventArgs e)
        {
            //проверить не запущено ли приложение
            Process procc = Process.GetCurrentProcess();
            //MessageBox.Show("Procc name ="+procc.ProcessName.ToString());
            //MessageBox.Show("proc count ="+Process.GetProcessesByName(procc.ProcessName.ToString()).Count().ToString());
            if (Process.GetProcessesByName(procc.ProcessName.ToString()).Count()>1)
            {
                MessageBox.Show("Copies of program not allowed!");
                Close();
            }
        }
        public void Show_researches_from(int id_research)
        {
            //шайтан работает, никто не знает как, короче форма вообще не удаляется
            Properties.Settings.Default.main_res_id = id_research;
            Properties.Settings.Default.Save();
            if (reserch_form != null)
            {
                reserch_form.Show();
                reserch_form.BringToFront();
            }
            else
            {
                GC.Collect();
                reserch_form = new Researches(this);
                reserch_form.Show();
            }
        }

        private void btn_research_Click(object sender, EventArgs e)
        {
            //блокируем изменение пароля и логина

            //открыть или создать форму с исследованиями
            Properties.Settings.Default.default_username = cbox_username.Text;
            Properties.Settings.Default.default_pass = txtbox_pass.Text;
            Properties.Settings.Default.Save();
            Show_researches_from(-1);                   
        }

        private void btn_reports_Click(object sender, EventArgs e)
        {

            
        }
    }
}
