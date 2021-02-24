using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Word = Microsoft.Office.Interop.Word;

namespace RSB
{
    public partial class Researches : Form
    {
        private readonly RSBMainForm Parent_form;
        private string conn_str;
        private bool on_load = false;
        private static List<string> data_filters = new List<string>();
        private static List<string> serv_data_filters = new List<string>();
        //private List<string> data_filters;        
        private int show_only_spec;

        public Researches(RSBMainForm parent)
        {
            InitializeComponent();
            Parent_form = parent;
        }
        public static MySqlConnection New_connection(string connString)
        {
            // Connection String.
            MySqlConnection conn = new MySqlConnection(connString);
            return conn;
        }
        public static bool Is_created()
        {
            bool ans = true;
            //
            return ans;
        }
        private string Get_conn_string(string myhost, int myport, string mydatabase, string username, string password)
        {
            string conn_str_loc;
            conn_str_loc = "Server=" + myhost + ";Database=" + mydatabase
                + ";port=" + myport + ";User Id=" + username + ";password=" + password;
            return conn_str_loc;
        }
        private int Get_index_datagrid(string id_row, int pos)
        {
            //поиск индекса строки с ID=id row в ячейке под номером pos
            int ans = -1;
            for (int i = 0; i < datagrid_researches.Rows.Count; i++)
            {
                if (datagrid_researches.Rows[i].Cells[3].Value != null)
                {
                    if (id_row == datagrid_researches.Rows[i].Cells[pos].Value.ToString())
                    {
                        ans = i;
                    }
                }
            }
            return ans;
        }        
        private void Refresh_data_researches()
        {
            datagrid_researches.Rows.Clear();
            conn_str = Get_conn_string(Properties.Settings.Default.server, Properties.Settings.Default.port,
               Properties.Settings.Default.database, Parent_form.cbox_username.Text, Parent_form.txtbox_pass.Text);
            if (chbox_show_serv.CheckState != CheckState.Checked)
            {
                using (MySqlConnection conn = New_connection(conn_str))
                {
                    try
                    {
                        conn.Open();
                        //dateTimePicker_ref_end.Value();
                        //dateTimePicker_stat_end
                        string sort_dec_asc = "";
                        string sort_sql = "";
                        //DateTime.TryParse(dateTimePicker_ref_start.Text, out DateTime temp_dat_start);
                        //DateTime.TryParse(dateTimePicker_ref_end.Text, out DateTime temp_dat_end);
                        //фильтры для SQL запроса
                        string sql_filtres = do_filtres_for_SQL(data_filters);
                        string sqlcom = "SELECT id_research, researches.res_date, materials.name, projects.name, type.name, producers.surname, " +
                            "researches.success, researches.temperature, researches.power_laser, setups.name " +
                        "FROM test2base.researches " +
                        "LEFT OUTER JOIN test2base.specimens ON test2base.researches.id_specimen=test2base.specimens.idspecimens " +
                        "LEFT OUTER JOIN test2base.materials ON test2base.specimens.id_material=test2base.materials.id_material " +
                        "LEFT OUTER JOIN test2base.projects ON test2base.specimens.id_project=test2base.projects.id_project " +
                        "LEFT OUTER JOIN test2base.producers ON test2base.specimens.id_respon=test2base.producers.id_producer " +
                        "LEFT OUTER JOIN test2base.type ON test2base.specimens.id_treat_type=test2base.type.id_type " +
                        "LEFT OUTER JOIN test2base.setups ON test2base.researches.id_setup=test2base.setups.id_setups " +
                        " WHERE (res_date >= '" + dateTimePicker_ref_start.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                        " AND res_date <= '" + dateTimePicker_ref_end.Value.ToString("yyyy-MM-dd HH:mm:ss") + "') " +
                        sql_filtres +
                        " ORDER BY researches.res_date DESC" +
                        sort_sql + sort_dec_asc;

                        using (MySqlCommand comand = new MySqlCommand(sqlcom, conn))
                        {
                            using (MySqlDataReader reader = comand.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    int num = 0;
                                    while (num < show_only_spec)
                                    {
                                        if (reader.Read())
                                        {
                                            //зачем это делать?                                    
                                            string srt;
                                            if (DateTime.TryParse(reader[1].ToString(), out DateTime temp_dat))
                                            {
                                                srt = temp_dat.ToString("yyyy.MM.dd");
                                            }
                                            else
                                            {
                                                srt = reader[4].ToString();
                                            }
                                            datagrid_researches.Rows.Add(reader[0].ToString(), srt, reader[2].ToString(), reader[3].ToString(),
                                                    reader[4].ToString(), reader[5].ToString(), reader[6].ToString(), reader[7].ToString(), reader[8].ToString(), reader[9].ToString());
                                        }
                                        num++;
                                    }
                                    reader.Close();
                                }
                                //else MessageBox.Show("nodata");
                            }
                            conn.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                    }
                }
            }
            else
            {
                //дополняем информацию о сервисном обслуживании
                using (MySqlConnection conn = New_connection(conn_str))
                {
                    try
                    {
                        conn.Open();
                        string sql_filters = do_filtres_for_SQL(serv_data_filters);
                        string sqlcom = "SELECT id_tech_work, work_type.name, tech_work.date, setups.name " +
                        "FROM test2base.tech_work " +
                        "LEFT OUTER JOIN test2base.work_type ON test2base.tech_work.id_work_type=test2base.work_type.id_work_type " +
                        "LEFT OUTER JOIN test2base.setups ON test2base.tech_work.id_setups=test2base.setups.id_setups " +
                        " WHERE (tech_work.date >= '" + dateTimePicker_ref_start.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                        " AND tech_work.date <= '" + dateTimePicker_ref_end.Value.ToString("yyyy-MM-dd HH:mm:ss") + "') " +
                        sql_filters +
                        " ORDER BY tech_work.date DESC";
                        using (MySqlCommand comand = new MySqlCommand(sqlcom, conn))
                        {
                            using (MySqlDataReader reader = comand.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    int num = 0;
                                    while (num < show_only_spec)
                                    {
                                        if (reader.Read())
                                        {
                                            string srt = "no";
                                            if (DateTime.TryParse(reader[2].ToString(), out DateTime temp_dat))
                                            {
                                                srt = temp_dat.ToString("yyyy.MM.dd");
                                            }
                                            if (reader[3].ToString() == "")
                                            {
                                                datagrid_researches.Rows.Add("tech", srt, reader[1].ToString(), reader[0].ToString(), "", "", "", "", "", "n/a");
                                            }
                                            else
                                                datagrid_researches.Rows.Add("tech", srt, reader[1].ToString(), reader[0].ToString(), "", "", "", "", "", reader[3].ToString());
                                            // цветовая дифференциация таблицы
                                            int ind = Get_index_datagrid(reader[0].ToString(), 3);
                                            //MessageBox.Show("Index = "+ind.ToString());
                                            if (ind != -1)
                                            {
                                                //MessageBox.Show("меняем цвет у ячейки № "+ind.ToString()+ "поле называется: "+ reader[1].ToString());
                                                switch (reader[1].ToString())
                                                {
                                                    case "No specimen":
                                                        datagrid_researches.Rows[ind].DefaultCellStyle.BackColor = Color.Orange;
                                                        break;
                                                    case "New calibration":
                                                        datagrid_researches.Rows[ind].DefaultCellStyle.BackColor = Color.LightPink;
                                                        break;
                                                    case "Service":
                                                        datagrid_researches.Rows[ind].DefaultCellStyle.BackColor = Color.LightBlue;
                                                        break;
                                                    case "Equipment repair":
                                                        datagrid_researches.Rows[ind].DefaultCellStyle.BackColor = Color.LightYellow;
                                                        break;
                                                }
                                            }
                                        }
                                        num++;
                                    }
                                    reader.Close();
                                }
                                else MessageBox.Show("nodata");
                            }
                            conn.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                    }
                    //пробуем фильтровать на этапе SQL запроса
                    //Do_filters(data_filters);

                }
            }
        }
        private string do_filtres_for_SQL(List<string> filters)
        {
            //пробуем сделать фильтры на основе SQL запроса
            string ans = "";
            if (filters.Count > 0)
            {
                //int num = 1;
                foreach (string str in filters)
                {
                    ans = ans + " AND (" + str + ")";
                }
            }
            return ans;
        }
        private void Fill_one_combo(MySqlConnection conect, string combo, string colname, string table_name)
        {
            try
            {
                conect.Open();
                string sqlcom = "SELECT " + colname + " FROM test2base." + table_name + " ORDER BY " + colname;
                using (MySqlCommand comand = new MySqlCommand(sqlcom, conect))
                {
                    using (MySqlDataReader reader = comand.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                switch (combo)
                                {
                                    case "type":
                                        //combox_f_type.Items.Add(reader[0].ToString());
                                        ch_listbox_type.Items.Add(reader[0].ToString(), true);
                                        break;
                                    case "project":
                                        //combox_f_project.Items.Add(reader[0].ToString());
                                        ch_listbox_projects.Items.Add(reader[0].ToString(), true);
                                        break;
                                    case "setups":
                                        //
                                        combox_setups_select.Items.Add(reader[0].ToString());
                                        break;
                                    case "setups_filter":
                                        //
                                        combox_setup_filter.Items.Add(reader[0].ToString());
                                        break;
                                }
                            }
                            reader.Close();
                        }
                    }
                    //проверить выполнен ли запрос
                    conect.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button1);
            }
        }
        private void Fill_combo()
        {
            using (MySqlConnection conn = New_connection(conn_str))
            {
                //producers
                //combox_f_type.Items.Clear();
                ch_listbox_type.Items.Clear();
                Fill_one_combo(conn, "type", "name", "type");
                //combox_f_project.Items.Clear();
                ch_listbox_projects.Items.Clear();
                Fill_one_combo(conn, "project", "name", "projects");
                combox_setups_select.Items.Clear();
                Fill_one_combo(conn, "setups", "name", "setups");
                combox_setup_filter.Items.Clear();
                Fill_one_combo(conn, "setups_filter", "name", "setups");
                ch_listbox_success.Items.Clear();
                ch_listbox_success.Items.Add("+", true);
                ch_listbox_success.Items.Add("-", true);
                ch_listbox_success.Items.Add("+/-", true);
                ch_listbox_success.Items.Add("-/+", true);
            }
        }

        private void Fill_text(int select_id, MySqlConnection connect, string table_name, string col_name)
        {
            try
            {
                connect.Open();
                string sqlcom = "SELECT " + col_name + " FROM test2base." + table_name +
                    " WHERE id_research = " + select_id.ToString();
                //MessageBox.Show(sqlcom);
                using (MySqlCommand comand = new MySqlCommand(sqlcom, connect))
                {
                    using (MySqlDataReader reader = comand.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            //MessageBox.Show("мы всегда сюда заходим????");
                            //MessageBox.Show(reader.FieldCount.ToString());

                            while (reader.Read())
                            {
                                switch (col_name)
                                {
                                    case "comments":
                                        string posi = reader[0].ToString();
                                        //combox_position.Text = posi;
                                        Lbl_inf_comments.Text = posi;
                                        break;
                                    case "duration":
                                        txtbox_inf_dur_days.Text = reader[0].ToString();
                                        break;
                                    case "data_dir":
                                        txtbox_inf_data_dir.Text = reader[0].ToString();
                                        break;
                                }
                            }
                            reader.Close();
                        }
                        //проверить выполнен ли запрос
                    }
                }
                connect.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        private string Simple_sql_ask(MySqlConnection connect,string request)
        {
        string ans="";
            try
            {
                connect.Open();
                string sqlcom = request;
                //MessageBox.Show(sqlcom);
                using (MySqlCommand comand = new MySqlCommand(sqlcom, connect))
                {
                    using (MySqlDataReader reader = comand.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            //MessageBox.Show("мы всегда сюда заходим????");
                            //MessageBox.Show(reader.FieldCount.ToString());

                            while (reader.Read())
                            {
                                ans = reader[0].ToString();
                            }
                            reader.Close();
                        }
                        //проверить выполнен ли запрос
                    }
                }
                connect.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка в запросе на поиск обработки", MessageBoxButtons.OK, MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            return ans;
        }
        private void Fill_info_text(int index)
        {
            using (MySqlConnection conn = New_connection(conn_str))
            {
                //заполняем поля                
                if (datagrid_researches.Rows[index].Cells[0].Value != null)
                {
                    if (datagrid_researches.Rows[index].Cells[0].Value.ToString() != "tech")
                    {
                        rich_txtbox_tech_inf.Text = "";
                        int i = Convert.ToInt32(datagrid_researches.Rows[index].Cells[0].Value);
                        //комментарии
                        //MessageBox.Show("Заполняееееееееем");
                        Fill_text(i, conn, "researches", "comments");
                        //количество дней
                        Fill_text(i, conn, "researches", "duration");
                        //директория данных
                        Fill_text(i, conn, "researches", "data_dir");
                        //обработка 
                        txtbox_spec_treatment.Text= Simple_sql_ask(conn, "SELECT name FROM treatment WHERE id_treatment = " +
                            "(select id_treatment from specimens where idspecimens = " +
                            "(select id_specimen from researches where id_research='"+i.ToString()+"'));");
                    }
                    else 
                    {
                        //заполняем технического рода информацию
                        Lbl_inf_comments.Text = "";
                        rich_txtbox_tech_inf.Text = "";
                        int i = Convert.ToInt32(datagrid_researches.Rows[index].Cells[3].Value); //ИД технической записи
                        rich_txtbox_tech_inf.Text = Simple_sql_ask(conn, "SELECT Comments FROM test2base.tech_work WHERE id_tech_work = " + i.ToString()+";");
                        combox_setups_select.Text = datagrid_researches.Rows[index].Cells[9].Value.ToString();
                        richTextBox_supp_comments.Text = rich_txtbox_tech_inf.Text;
                    }
                }
            }
        }

        private void Fill_information()
        {
            //заполняем информацию
            //берем её из дата грида
            if (datagrid_researches.Rows.Count > 0 && datagrid_researches.CurrentRow != null)
            {
                int Sel_index = datagrid_researches.CurrentRow.Index;
                //MessageBox.Show("Индекс =" +Sel_index.ToString());
                Fill_info_text(Sel_index);
            }
        }

        private void Researches_Load(object sender, EventArgs e)
        {
            //грузим все данные в датагрид
            on_load = true;
            //MessageBox.Show("value= "+ dateTimePicker_ref_end.Value.ToString()+"\n text=" + dateTimePicker_ref_end.Text);
            //data_filters = new List<string>();
            data_filters.Clear();
            show_only_spec = Convert.ToInt32(combox_show_only.Text);
            //заполняем поля из сохраненных параметров
            combox_show_only.Text = Properties.Settings.Default.show_only_researches.ToString();
            chbox_show_serv.Checked = Properties.Settings.Default.show_techinfo;
            
            DateTime.TryParse(dateTimePicker_ref_start.Text, out DateTime temp_dat_end);         
            string date_start = temp_dat_end.AddYears(-1).ToString("dd MMMM yyyy");
            //MessageBox.Show(date_start + " г.");
            dateTimePicker_ref_start.Text = date_start+" г.";
            //
            Refresh_data_researches();
            Fill_combo();
            on_load = false;
            Fill_information();            
        }



        private void btn_clear_projects_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ch_listbox_projects.Items.Count; i++)
            {
                ch_listbox_projects.SetItemChecked(i, false);
            }
        }

        private void btn_clear_types_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ch_listbox_type.Items.Count; i++)
            {
                ch_listbox_type.SetItemChecked(i, false);
            }
        }
        private void Do_filters(List<string> filters)
        {
            if (filters.Count > 0)
            {
                for (int i = 0; i < datagrid_researches.Rows.Count; i++)
                {
                    bool visi = true;
                        for (int j = 0; j < datagrid_researches.Rows[i].Cells.Count; j++)
                        {
                            foreach (string str in filters)
                            {
                                if (datagrid_researches.Rows[i].Cells[j].Value != null)
                                {
                                    if (datagrid_researches.Rows[i].Cells[j].Value.ToString() == str)
                                    {
                                        //MessageBox.Show("невидимо, так как "+str+" равно "+ datagrid_researches.Rows[i].Cells[j].Value.ToString());
                                        visi = false;
                                    }
                                }
                            }
                        }
                    datagrid_researches.Rows[i].Visible = visi;
                }
            }
            else
            {
                for (int i = 0; i < datagrid_researches.Rows.Count; i++)
                {
                    datagrid_researches.Rows[i].Visible = true;
                }
            }

            //Заполняем сатистику по показанным исследованиям
            int col = -1;
            for (int i = 0; i < datagrid_researches.Rows.Count; i++)
            {
                if (datagrid_researches.Rows[i].Visible)
                {
                    col += 1;
                }
            }
            txtbox_stat_show_res.Text = col.ToString();
        }
        private void ch_listbox_type_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!on_load)
            {
                if (e.NewValue == CheckState.Checked)
                {
                    //тест новых фильтров
                    data_filters.Remove("type.name <> '" + ch_listbox_type.Items[e.Index].ToString() + "'");

                    //удаляем из фильтров
                    //data_filters.Remove(ch_listbox_type.Items[e.Index].ToString());
                }
                else
                {
                    //тест новых фмльтров
                    data_filters.Add("type.name <> '" + ch_listbox_type.Items[e.Index].ToString()+"'");

                    //добавляем к фильтрам
                    //data_filters.Add(ch_listbox_type.Items[e.Index].ToString());
                }                
                Refresh_data_researches();
            }
        }

        private void ch_listbox_projects_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!on_load)
            {
                if (e.NewValue == CheckState.Checked)
                {
                    //тест новых фильтров
                    data_filters.Remove("projects.name <> '" + ch_listbox_projects.Items[e.Index].ToString() + "'");

                    //удаляем из фильтров
                    //data_filters.Remove(ch_listbox_projects.Items[e.Index].ToString());
                }
                else
                {
                    //тест новых фмльтров
                    //MessageBox.Show("Index = "+e.Index.ToString());
                    data_filters.Add("projects.name <> '" + ch_listbox_projects.Items[e.Index].ToString() + "'");

                    //добавляем к фильтрам
                    //data_filters.Add(ch_listbox_projects.Items[e.Index].ToString());
                }
                Refresh_data_researches();
            }
        }

        private void btn_select_all_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ch_listbox_type.Items.Count; i++)
            {
                ch_listbox_type.SetItemChecked(i, true);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ch_listbox_projects.Items.Count; i++)
            {
                ch_listbox_projects.SetItemChecked(i, true);
            }
        }

        private void btn_clear_succ_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ch_listbox_success.Items.Count; i++)
            {
                ch_listbox_success.SetItemChecked(i, false);
            }
        }

        private void ch_listbox_success_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!on_load)
            {
                if (e.NewValue == CheckState.Checked)
                {
                    //тест новых фильтров
                    data_filters.Remove("researches.success <> '" + ch_listbox_success.Items[e.Index].ToString() + "'");

                    //удаляем из фильтров
                    //data_filters.Remove(ch_listbox_success.Items[e.Index].ToString());
                }
                else
                {
                    //тест новых фмльтров
                    data_filters.Add("researches.success <> '" + ch_listbox_success.Items[e.Index].ToString() + "'");

                    //добавляем к фильтрам
                    //data_filters.Add(ch_listbox_success.Items[e.Index].ToString());
                }
                Refresh_data_researches();
            }
        }

        private void datagrid_researches_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void btn_SellAll_succ_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ch_listbox_success.Items.Count; i++)
            {
                ch_listbox_success.SetItemChecked(i, true);
            }
        }

        private void Researches_FormClosed(object sender, FormClosedEventArgs e)
        {
            Save_settings_res();
            GC.Collect();
        }

        private void Researches_FormClosing(object sender, FormClosingEventArgs e)
        {
            Save_settings_res();
            e.Cancel = true;
            //MessageBox.Show("Нельзя просто закрыть окно. \n Надо решить задание. \n Если правильно решишь, то больше будешь играть.");
            //else e.Cancel = false;
            Hide();
            //GC.Collect();
        }
        private int Get_id_research(string where, string index)
        {
            int ans = -1;
            //int index = -1;
            string col_name = "";
            string col_name_2 = "";
            switch (where)
            {
                case "researches":
                    col_name = "id_research";
                    col_name_2 = "id_specimen";
                    break;
                case "specimens":
                    col_name = "id_specimen";
                    col_name_2 = "id_research";
                    break;
            }
            //MessageBox.Show("Ищем ид в " + where + " ид для поиска " + index);
            if (index != "-1")
            {
                //запрос на поиск id
                using (MySqlConnection conn = New_connection(conn_str))
                {
                    try
                    {
                        conn.Open();
                        string sql_comand = "SELECT researches." + col_name + " FROM test2base.researches WHERE " + col_name_2 + "=" + index; //второй сол_наме должен быть id_specimen
                        //MessageBox.Show(sql_comand);
                        using (MySqlCommand comand = new MySqlCommand(sql_comand, conn))
                        {
                            using (MySqlDataReader reader = comand.ExecuteReader())
                            {

                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        ans = Convert.ToInt32(reader[0]);
                                        MessageBox.Show("Найденный ид = " + ans);
                                    }
                                    reader.Close();
                                }
                            }
                        }
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                    }
                }

            }
            return ans;
        }
        private void datagrid_researches_DoubleClick(object sender, EventArgs e)
        {
            //если есть образец этого исследования - переход в окно образцов
            if (datagrid_researches.CurrentRow.Cells[0].Value != null && datagrid_researches.CurrentRow.Cells[0].Value.ToString()!="tech")
            {
                //MessageBox.Show("показываем форму образцов");
                string index = datagrid_researches.CurrentRow.Cells[0].Value.ToString();
                Properties.Settings.Default.main_res_id = Convert.ToInt32(index);
                Properties.Settings.Default.Save();
                int id_res = Get_id_research("specimens", index);
                Parent_form.Show_specimens_form(id_res);
            }
        }

        private void Researches_Activated(object sender, EventArgs e)
        {
            //Select_index();
            //MessageBox.Show("Байда активированна");
            if (Properties.Settings.Default.main_res_id!=-1 && !on_load)
            {
                //MessageBox.Show("нужно показать образец");
                if (Get_index_datagrid(Properties.Settings.Default.main_res_id.ToString(), 0) != -1)
                {
                    datagrid_researches.Rows[Get_index_datagrid(Properties.Settings.Default.main_res_id.ToString(), 0)].Selected = true;
                }
                // ЗОЧЕМ??? присваивать -1???
                //Properties.Settings.Default.main_res_id = -1;
                //Properties.Settings.Default.Save();
            }
        }

        private int Calc_vis_rows()
        {
            //считаем количество видимых строк
            int ans = 0;
            for (int r = 0; r <= datagrid_researches.Rows.Count-1; r++)
            {
                if (datagrid_researches.Rows[r].Visible)
                {
                    ans++;
                }
            }
            return ans;
        }

        private void Make_paragraph(Word._Document Docum, string p_text, int i_bold, int space_after, Word.WdParagraphAlignment align)
        {
            //просто процедура вставки абзаца текста с укзаанием типа, формата и самого текста
            //ВСЕГДА ставляется в конец
            if (i_bold == 1 || i_bold == 0)
            {
                object oEndOfDoc = "\\endofdoc";
                Word.Paragraph oPara;
                object oRng = Docum.Bookmarks.get_Item(ref oEndOfDoc).Range;
                oPara = Docum.Content.Paragraphs.Add(ref oRng);
                oPara.Range.Text = p_text;
                oPara.Range.Font.Bold = i_bold;
                oPara.Format.SpaceAfter = space_after;
                oPara.Range.ParagraphFormat.Alignment = align;
                oPara.Range.InsertParagraphAfter();
            }
            else
            {
                MessageBox.Show("Bad parameters");
            }
        }
        private void Btn_report_create_Click(object sender, EventArgs e)
        {
            //создаем отчеты
            //предупреждение
            if (Properties.Settings.Default.user_access_lvl <= 2)
            {
                MessageBox.Show("Attention! The report will be created for ALL studies in the table at the moment.");
                //если есть строки, то создаем отчет
                if (datagrid_researches.Rows.Count > 1)
                {
                    object oMissing = System.Reflection.Missing.Value;
                    object oEndOfDoc = "\\endofdoc"; /* \endofdoc is a predefined bookmark */

                    //Start Word and create a new document.
                    Word._Application oWord;
                    Word._Document oDoc;
                    oWord = new Word.Application
                    {
                        Visible = false
                    };
                    oDoc = oWord.Documents.Add(ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing);
                    Make_paragraph(oDoc, "НИЦ «КУРЧАТОВСКИЙ ИНСТИТУТ» - ИТЭФ", 0, 3, Word.WdParagraphAlignment.wdAlignParagraphCenter);
                    Make_paragraph(oDoc, "Отдел атомно-масштабных и ядерно-физических методов исследования материалов ядерной техники", 0, 3, Word.WdParagraphAlignment.wdAlignParagraphCenter);
                    Make_paragraph(oDoc, "Лаборатория атомно-масштабных исследований конденсированных сред", 0, 12, Word.WdParagraphAlignment.wdAlignParagraphCenter);
                    DateTime temp_dat = DateTime.Now;
                    Make_paragraph(oDoc, "г. Москва                                                " +
                        "                                                    " + temp_dat.ToString("dd MMMM yyyy"), 1, 3, Word.WdParagraphAlignment.wdAlignParagraphCenter);
                    Make_paragraph(oDoc, "ПРОТОКОЛ №_____", 1, 6, Word.WdParagraphAlignment.wdAlignParagraphCenter);
                    Make_paragraph(oDoc, "АЗТ ИССЛЕДОВАНИЕ ОБРАЗЦОВ", 1, 3, Word.WdParagraphAlignment.wdAlignParagraphCenter);
                    Make_paragraph(oDoc, "1.	Основание выполнения работ: Договор № 313/1710-Д с АО 'Наука и инновации''", 0, 3, Word.WdParagraphAlignment.wdAlignParagraphLeft);
                    Make_paragraph(oDoc, "2.	Цель работы: проведение атомно-зондовых исследований образцов материалов", 0, 3, Word.WdParagraphAlignment.wdAlignParagraphLeft);
                    Make_paragraph(oDoc, "3.	Материал, дата, параметры исследования приведены в таблице", 0, 3, Word.WdParagraphAlignment.wdAlignParagraphLeft);
                    Make_paragraph(oDoc, "4.	Установка: атомно-зондовый томограф ПАЗЛ-3D", 0, 6, Word.WdParagraphAlignment.wdAlignParagraphLeft);

                    //Insert a table, fill it with data, and make the first row
                    //bold and italic.
                    Word.Table oTable;
                    Word.Range wrdRng = oDoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
                    int row_count = Calc_vis_rows();
                    oTable = oDoc.Tables.Add(wrdRng, row_count, 8, ref oMissing, ref oMissing);
                    oTable.Range.ParagraphFormat.SpaceAfter = 6;
                    oTable.Range.Font.Bold = 0;
                    oTable.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    oTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    int row = 2;
                    oTable.Cell(1, 1).Range.Text = "ID";
                    oTable.Cell(1, 2).Range.Text = "Дата";
                    oTable.Cell(1, 3).Range.Text = "Материал";
                    oTable.Cell(1, 4).Range.Text = "Тип образца";
                    oTable.Cell(1, 5).Range.Text = "Температура, К";
                    oTable.Cell(1, 6).Range.Text = "Мощность, мВт";
                    oTable.Cell(1, 7).Range.Text = "Количество атомов, шт";
                    oTable.Cell(1, 8).Range.Text = "Подпись";

                    for (int r = 1; r <= datagrid_researches.Rows.Count - 1; r++)
                    {
                        if (datagrid_researches.Rows[r - 1].Visible)
                        {
                            oTable.Cell(row, 1).Range.Text = datagrid_researches.Rows[r - 1].Cells[0].Value.ToString();
                            oTable.Cell(row, 2).Range.Text = datagrid_researches.Rows[r - 1].Cells[1].Value.ToString();
                            oTable.Cell(row, 3).Range.Text = datagrid_researches.Rows[r - 1].Cells[2].Value.ToString();
                            oTable.Cell(row, 4).Range.Text = datagrid_researches.Rows[r - 1].Cells[4].Value.ToString();
                            oTable.Cell(row, 5).Range.Text = datagrid_researches.Rows[r - 1].Cells[7].Value.ToString();
                            oTable.Cell(row, 6).Range.Text = datagrid_researches.Rows[r - 1].Cells[8].Value.ToString();
                            oTable.Cell(row, 7).Range.Text = "";
                            oTable.Cell(row, 8).Range.Text = "";
                            row++;
                        }
                    }
                    System.Threading.Thread.Sleep(100);
                    Make_paragraph(oDoc, "5.	Контроль формы образца до исследования проведен", 0, 3, Word.WdParagraphAlignment.wdAlignParagraphLeft);
                    Make_paragraph(oDoc, "6.	Особые замечания при исследовании:", 0, 3, Word.WdParagraphAlignment.wdAlignParagraphLeft);
                    Word.Table oTable2;
                    wrdRng = oDoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
                    oTable2 = oDoc.Tables.Add(wrdRng, row_count, 3, ref oMissing, ref oMissing);
                    oTable2.Range.ParagraphFormat.SpaceAfter = 6;
                    oTable2.Range.Font.Bold = 0;
                    oTable2.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    oTable2.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    row = 2;
                    oTable2.Cell(1, 1).Range.Text = "ID";
                    oTable2.Cell(1, 2).Range.Text = "Материал";
                    oTable2.Cell(1, 3).Range.Text = "Замечания/комментарии";

                    for (int r = 1; r <= datagrid_researches.Rows.Count - 1; r++)
                    {
                        if (datagrid_researches.Rows[r - 1].Visible)
                        {
                            oTable2.Cell(row, 1).Range.Text = datagrid_researches.Rows[r - 1].Cells[0].Value.ToString();
                            oTable2.Cell(row, 2).Range.Text = datagrid_researches.Rows[r - 1].Cells[2].Value.ToString();
                            oTable2.Cell(row, 3).Range.Text = "Нет";
                            row++;
                        }
                    }
                    //Make_paragraph(oDoc, "7.	Фото и файлы данных хранятся в установленном месте. В наименовании папок указаны: дата, материал", 0, 12, Word.WdParagraphAlignment.wdAlignParagraphLeft);
                    Make_paragraph(oDoc, "Исполнитель 	                                                                       Разницын О.А. ", 0, 12, Word.WdParagraphAlignment.wdAlignParagraphLeft);
                    Make_paragraph(oDoc, "Контролер                                                                            Никитин А.А.", 0, 3, Word.WdParagraphAlignment.wdAlignParagraphLeft);
                    //получаем путь сохранения
                    saveFileDial_for_report.CreatePrompt = true;
                    saveFileDial_for_report.OverwritePrompt = true;
                    saveFileDial_for_report.RestoreDirectory = true;
                    saveFileDial_for_report.Filter = "Doc files (*.doc)|*.doc|All files (*.*)|*.*";
                    DialogResult result = saveFileDial_for_report.ShowDialog();
                    if (result != DialogResult.Cancel)
                    {
                        string file_name = saveFileDial_for_report.FileName;
                        oDoc.SaveAs2(file_name);
                    }
                    else MessageBox.Show("Вы отказались от своего счастья, приется начинать всё с начала");
                    //возможно без сохранения Ворд всё равно пытается сохранить
                    //public void Close (ref object SaveChanges, ref object OriginalFormat, ref object RouteDocument);
                    //Can be one of the following WdSaveOptions constants: wdDoNotSaveChanges, wdPromptToSaveChanges, or wdSaveChanges.
                    oDoc.Close();
                    oWord.Quit();
                    GC.Collect();
                }
                else
                {
                    MessageBox.Show("No rows?");
                }
            }
            else
                MessageBox.Show("Нет прав доступа, обратитесь к администратору");
        }

        private void FontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //смена шрифта диалог и зменю
            if (fontDial_resech.ShowDialog() == DialogResult.OK)
            {
                //textBox1.Font = fontDialog1.Font;
                //textBox1.ForeColor = fontDialog1.Color;
                try
                {
                    this.Font = new Font(fontDial_resech.Font, this.Font.Style);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                }
            }
        }
        private void Make_serv_record(int type, string comments, int duration)
        {
            //добавляем запись в лог технической информации
            //сначала проверим есть ли уже в этот день запись этого же типа
            bool check_for_exist = false;
            int id_tech_work = -1;
            using (MySqlConnection conn = New_connection(conn_str))
            {
                try
                {
                    conn.Open();
                    DateTime.TryParse(dateTimePicker1.Text, out DateTime temp_dat);
                    string sqlcom_4 = "SELECT id_tech_work FROM test2base.tech_work WHERE  (date = '"+ temp_dat.ToString("yyyy-MM-dd HH:mm:ss") + "' AND id_work_type = "+type.ToString()+
                        " AND id_setups = (SELECT id_setups FROM test2base.setups WHERE (name= '"+combox_setups_select.Text+"')))";
                    using (MySqlCommand comand = new MySqlCommand(sqlcom_4, conn))
                    {
                        using (MySqlDataReader reader = comand.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                check_for_exist = true;
                                id_tech_work = Convert.ToInt32(reader[0]);
                                reader.Close();
                            }
                            else
                            {
                                check_for_exist = false;                                
                            }

                        }
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                }
            }
            //если нет такого, то добавляем
            if (check_for_exist==false)
            {
                using (MySqlConnection conn = New_connection(conn_str))
                {
                    try
                    {
                        conn.Open();
                        //string sqlcom_4 = "INSERT INTO test2base.history (id_spec, action, old, new, date, user) VALUES (@id_spec, @action, @old, @new, @date, @user)";
                        string sqlcom_4 = "INSERT INTO test2base.tech_work (id_work_type, Comments, Duration, date, id_setups) VALUES(@id_work_type, @test_comments, @duration, @date," +
                            " (SELECT id_setups FROM test2base.setups WHERE name = @id_setups))";
                        using (MySqlCommand comand = new MySqlCommand(sqlcom_4, conn))
                        {
                            comand.Parameters.AddWithValue("@id_work_type", type);
                            comand.Parameters.AddWithValue("@test_comments", comments);
                            comand.Parameters.AddWithValue("@duration", duration);
                            DateTime.TryParse(dateTimePicker1.Text, out DateTime temp_dat);
                            comand.Parameters.AddWithValue("@date", temp_dat.ToString("yyyy-MM-dd HH:mm:ss"));
                            comand.Parameters.AddWithValue("@id_setups", combox_setups_select.Text);
                            _ = comand.ExecuteNonQuery();
                        }
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                    }
                }
            }
            else
            {
                //если есть спрашиваем пользователя?
                //UPDATE test2base.tech_work SET Comments = '' WHERE (id_tech_work = '');
                DialogResult dialogResult = MessageBox.Show("Вы пытаетесь обновить запись технической информации, вы действительно этого хотитте?\n " +
                    "Будет сохранено только то, что находится в данный момент в поле описания технической информации\n" +
                    "Yes - информация будет сохранена\n" +
                    "No - ничего не будет происходить", "Запрос сохранения", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes && id_tech_work!=-1)
                {
                    //....
                    MessageBox.Show("сохраняем, на самом деле нет, но мы пытаемся))) \n id_techwork="+id_tech_work.ToString());
                    //обновляем поле сервисной информации
                    using (MySqlConnection conn = New_connection(conn_str))
                    {
                        try
                        {
                            conn.Open();
                            DateTime.TryParse(dateTimePicker1.Text, out DateTime temp_dat);
                            string sqlcom_4 = "UPDATE test2base.tech_work SET Comments = '"+richTextBox_supp_comments.Text+"', date = '"+
                                temp_dat.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE (id_tech_work = '"+ id_tech_work.ToString() + "')";
                            using (MySqlCommand comand = new MySqlCommand(sqlcom_4, conn))
                            {     
                                _ = comand.ExecuteNonQuery();
                            }
                            conn.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1);
                        }
                    }
                }
                else if (dialogResult == DialogResult.No)
                {
                    //...ничего не делаем, очищаем поле

                }
            }
        }

        private void Btn_make_serv_comm_Click(object sender, EventArgs e)
        {
            //добавить запись о Service
            if (Properties.Settings.Default.user_access_lvl <= 2)
            {
                //3 - новая калибровка
                Make_serv_record(3, richTextBox_supp_comments.Text, Convert.ToInt32(combox_duration.Text));
            }
            else
                MessageBox.Show("Не достаточно прав доступа, обратитесь к администратору");
        }

        private void btn_no_specimen_Click(object sender, EventArgs e)
        {
            //добавить запись нет образца
            if (Properties.Settings.Default.user_access_lvl <= 2)
            {
                //1- no specimen
                Make_serv_record(1, richTextBox_supp_comments.Text, Convert.ToInt32(combox_duration.Text));
            }
            else
                MessageBox.Show("Не достаточно прав доступа, обратитесь к администратору");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //добавить новую запись о калибровке
            if (Properties.Settings.Default.user_access_lvl <= 2)
            {
                //2 - новая калибровка
                Make_serv_record(2, richTextBox_supp_comments.Text, Convert.ToInt32(combox_duration.Text));
            }
            else
                MessageBox.Show("Не достаточно прав доступа, обратитесь к администратору");
        }

        private void Btn_make_serv_emerg_Click(object sender, EventArgs e)
        {
            //добавить новую запись о ремонте
            if (Properties.Settings.Default.user_access_lvl <= 2)
            {
                //4 - новая калибровка
                Make_serv_record(4, richTextBox_supp_comments.Text, Convert.ToInt32(combox_duration.Text));
            }
            else MessageBox.Show("Не достаточно прав доступа, обратитесь к администратору");
        }

        private void chbox_show_serv_CheckedChanged(object sender, EventArgs e)
        {
            //MessageBox.Show("измен сост галочки))))= "+ chbox_show_serv.Checked.ToString());
            //if (chbox_show_serv.Checked == true)
            //{
                //удаляем из фильтров
                //data_filters.Remove("tech");

            //}
            //else
            //{
                //добавляем к фильтрам
                //data_filters.Add("tech");
            //}
            //Do_filters(data_filters);
        }

        private void combox_show_only_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (combox_show_only.Text != "" && combox_show_only.Text != "All")
            {
                show_only_spec = Convert.ToInt32(combox_show_only.Text);
            }
            else
            {
                if (combox_show_only.Text == "All")
                {
                    show_only_spec = 10000;
                }
                else MessageBox.Show("SMTH wrong with 'Show_only Box'");
            }
            Refresh_data_researches();
        }

        private void combox_show_only_KeyPress(object sender, KeyPressEventArgs e)
        {
            //шоб не писали символы
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8 && number != 127)
            {
                e.Handled = true;
            }
        }

        private void combox_show_only_TextChanged(object sender, EventArgs e)
        {
            if (combox_show_only.Text != "" && combox_show_only.Text != "All")
            {
                show_only_spec = Convert.ToInt32(combox_show_only.Text);
            }
            else
            {
                if (combox_show_only.Text == "All")
                {
                    show_only_spec = 10000;
                }
                else MessageBox.Show("SMTH wrong with 'Show_only Box'");
            }
            Refresh_data_researches();
        }

        private void Researches_VisibleChanged(object sender, EventArgs e)
        {
            //появляется 2 раза при создании и смерти)
            //MessageBox.Show("Просто тест функции  Визибл чандж");
        }

        private void datagrid_researches_CurrentCellChanged(object sender, EventArgs e)
        {
            //
            if (!on_load)
            {                
                Fill_information();
            }
        }
        private void Save_settings_res()
        {
            //просто сохранение параметров
            Properties.Settings.Default.show_only_researches = Convert.ToInt32(combox_show_only.Text);
            Properties.Settings.Default.show_techinfo = chbox_show_serv.Checked;
            Properties.Settings.Default.Save();
        }

        private void Btn_stat_show_Click(object sender, EventArgs e)
        {
            //посчитать статистику по выбранным исследованиям
            //если даты разные и промежуток между ними не отрицательный или равен нулю
            if (Properties.Settings.Default.user_access_lvl <= 2)
            {
                double FIB = 0;        //2
                double Chem = 0;       //1
                double FIB_ch = 0;     //3
                double FIB_accros = 0; //5
                double FIB_dep = 0;    //4

                //собираем перечень контрактов
                List<string> projects_name = new List<string>();
                List<string> projects_id = new List<string>();
                //List<string> projects_summ = new List<string>();
                List<double> projects_count = new List<double>();
                using (MySqlConnection conn = New_connection(conn_str))
                {
                    try
                    {
                        conn.Open();
                        string sql_comand = "SELECT id_project, name, summ_oper_apt FROM test2base.projects WHERE contract <> 'NO'";
                        using (MySqlCommand comand = new MySqlCommand(sql_comand, conn))
                        {
                            using (MySqlDataReader reader = comand.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        //MessageBox.Show(reader[0].ToString());
                                        projects_id.Add(reader[0].ToString());
                                        projects_name.Add(reader[1].ToString());
                                        //projects_summ.Add(reader[2].ToString());
                                        projects_count.Add(0);
                                    }
                                    reader.Close();
                                }
                            }
                        }
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + "При расчете статистики контрактам", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                    }
                }
                double contract = 0;
                //выбираем все образцы в которых только электрохимия за отчетный период
                using (MySqlConnection conn = New_connection(conn_str))
                {
                    try
                    {
                        conn.Open();
                        DateTime.TryParse(dateTimePicker_stat_start.Text, out DateTime temp_dat_start);
                        DateTime.TryParse(dateTimePicker_stat_end.Text, out DateTime temp_dat_end);
                        string sql_comand = "SELECT specimens.id_treat_type, specimens.id_project FROM test2base.researches  " +
                            "LEFT OUTER JOIN test2base.specimens ON test2base.researches.id_specimen=test2base.specimens.idspecimens" +
                            " WHERE res_date >= '" + temp_dat_start.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                            "AND res_date <= '" + temp_dat_end.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                        //MessageBox.Show(sql_comand);
                        using (MySqlCommand comand = new MySqlCommand(sql_comand, conn))
                        {
                            using (MySqlDataReader reader = comand.ExecuteReader())
                            {

                                if (reader.HasRows)
                                {                                    
                                    while (reader.Read())
                                    {
                                        //MessageBox.Show(reader[0].ToString());
                                        //тип образца
                                        switch (reader[0].ToString())
                                        {
                                            case "1":
                                                Chem+=1;
                                                break;
                                            case "2":
                                                FIB+=1;
                                                break;
                                            case "3":
                                                FIB_ch+=1;
                                                break;
                                            case "4":
                                                FIB_dep+=1;
                                                break;
                                            case "5":
                                                FIB_accros+=1;
                                                break;
                                        }
                                        //тип проекта
                                        foreach (string id in projects_id)
                                        {
                                            if (reader[1].ToString()==id)
                                            {
                                                //значит образец коммерческий
                                                contract += 1;
                                                int index_ofpr = projects_id.IndexOf(id);
                                                projects_count[index_ofpr] =projects_count[index_ofpr]+ 1;
                                            }
                                        }
                                    }
                                    reader.Close();
                                }
                            }
                        }
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message+"При расчете статистики по образцам", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                    }
                }        
                double summ = Chem + FIB + FIB_accros + FIB_ch + FIB_dep;
                //заполняем статистику
                lbl_stat_spec_type.Text = "";
                if (Chem != 0)
                {
                    lbl_stat_spec_type.Text = lbl_stat_spec_type.Text+"Электрохимия - " + Math.Round(Chem / (summ) * 100).ToString() + "% \n";
                }
                if (FIB!=0)
                {
                    lbl_stat_spec_type.Text = lbl_stat_spec_type.Text+"FIB - " + Math.Round(FIB / (summ) * 100).ToString() + "% \n";
                }
                if (FIB_ch != 0)
                {
                    lbl_stat_spec_type.Text = lbl_stat_spec_type.Text+"FIB + Электрохимия - " + Math.Round(FIB_ch / (summ) * 100).ToString() + "% \n";
                }
                if (FIB_accros != 0)
                {
                    lbl_stat_spec_type.Text = lbl_stat_spec_type.Text+ "FIB вдоль - " + Math.Round(FIB_accros / (summ) * 100).ToString() + "% \n";
                }
                if (FIB_dep != 0)
                {
                    lbl_stat_spec_type.Text = lbl_stat_spec_type.Text+ "FIB + напыление - " + Math.Round(FIB_dep / (summ) * 100).ToString() + "% \n";
                }
                lbl_stat_fin_sup.Text = "Коммерческие - "+Math.Round(contract/summ*100).ToString()+ "% \nОстальные - "+ Math.Round((summ-contract)/summ*100).ToString()+ " %";
                string temp_str = "";
                foreach (string name in projects_name)
                {
                    temp_str = temp_str + name+ " - "+projects_count[projects_name.IndexOf(name)].ToString() +" исследований\n ";
                }
                lbl_stat_per_study.Text = temp_str;
            }
        }

        private void txtbox_inf_data_dir_DoubleClick(object sender, EventArgs e)
        {
            //открываем проводник с данным путем
            string str = Directory.GetParent(txtbox_inf_data_dir.Text).ToString();
            //string str = txtbox_inf_data_dir.Text;
            if (Directory.Exists(str))
            {
                Process.Start("explorer.exe", str);
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            //мануальная кнопка обновления
            Refresh_data_researches();
        }

        private void combox_select_setup_KeyPress(object sender, KeyPressEventArgs e)
        {
            //шоб не писали руками ничего
            combox_setups_select.Text = "";
        }

        private void combox_setup_filter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (combox_setup_filter.Text!="")
            {
                foreach (string str in combox_setup_filter.Items)
                {
                    if (str!=combox_setup_filter.Text)
                    {
                        serv_data_filters.Remove("setups.name = '" + str + "'");
                        data_filters.Remove("setups.name = '" + str + "'");
                    }
                    //MessageBox.Show(str);
                }
                data_filters.Add("setups.name = '" + combox_setup_filter.Text + "'");
                serv_data_filters.Add("setups.name = '" + combox_setup_filter.Text + "'");
                //data_filters.Remove("setups.name <> '" + ch_listbox_success.Items[e.Index].ToString() + "'");                                           
                Refresh_data_researches();
            }
        }
    }
}
