using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using MySql.Data;
using MySql.Data.MySqlClient;
using Excel = Microsoft.Office.Interop.Excel;
using MihaZupan;
using Newtonsoft.Json;
using Discord.WebSocket;

namespace RSB
{
    public partial class Form_specimens : Form
    {
        private readonly RSBMainForm Parent_form;
        private static string conn_str;
        private static List<string> images_paths = new List<string>();
        //private static List<string> data_filter = new List<string>();
        private static bool on_load = true;
        private string[] info_files_paths_bef;
        private string[] info_files_paths_aft;
        private static bool specimen_new_accepted = true;
        private int refresh_counter = 0;
        private static bool isrefreshing = true; // просто для исключения ненужных обновлений при нажатии кнопок Select All/Clear All
        //для картинки на кнопке
        private bool pic_change = true; //true - up, false - down
        private int show_only_spec;
        private DiscordSocketClient _client;
        public class Filtres_master
        {
            public List<string> common_filters;// = new List<string>();
            public bool is_special_filter;
            public string special_filter;
        }
        public Filtres_master filt_master = new Filtres_master
        {
            common_filters = new List<string>(),
            special_filter = "",
            is_special_filter = false
        };
        class New_specimen_data
        {
            //класс данных для нового образца
            public string producer;
            public string material;
            public string project;
            public string type;
            public string datetime;
            public string foto_before;
            public string foto_after;
            public int state;
            public string storage;
            public string treatment;
            public string resonse;
            public string stor_pos;
            public string priority;
        }
        class New_research
        {
            //класс данных исследования
            public string succ;
            public string temp;
            public string laser_power;
            public string comments;
            public string duration;
            public string date_res;
        }
        class New_producer
        {
            public string name;
            public string surname;
            public int access;
        }
        class New_material
        {
            public string name;
            public string compostion;
        }
        class New_treatment
        {
            public string name;
            public string dose;
            public string irr_ions;
            public string temper;
            public string time;
            public string comments;
            public string energy;
            public string irr_type;
        }
        public Form_specimens(RSBMainForm parent)
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


        private string Get_conn_string(string myhost, int myport, string mydatabase, string username, string password)
        {
            string conn_str_loc;
            conn_str_loc = "Server=" + myhost + ";Database=" + mydatabase
                + ";port=" + myport + ";User Id=" + username + ";password=" + password;
            return conn_str_loc;
        }
        private int Get_index_datagrid(string id_row,int pos)
        {
            int ans = -1;
            for (int i=0; i<dataGrid_specimens.Rows.Count;i++)
            {
                if (dataGrid_specimens.Rows[i].Cells[0].Value != null)
                {
                    if (id_row == dataGrid_specimens.Rows[i].Cells[pos].Value.ToString())
                    {
                        ans = i;
                    }
                }
            }
            return ans;
        }
        private string do_filtres_for_SQL(List<string> filters)
        {
            //пробуем сделать фильтры на основе SQL запроса            
            string ans = "";
            if (filters != null)
            {
                if (filters.Count > 0)
                {
                    //int num = 1;
                    foreach (string str in filters)
                    {
                        ans = ans + " AND (" + str + ")";
                    }
                }
            }
            return ans;
        }
        private void Refresh_datagrid()
        {
            int selected_id = Properties.Settings.Default.main_spec_id;
            if ((!on_load) && dataGrid_specimens.Rows.Count>1 && dataGrid_specimens.CurrentRow!=null)
            {
                if (dataGrid_specimens.CurrentRow.Cells[0].Value!=null)
                {
                    selected_id = Convert.ToInt32(dataGrid_specimens.CurrentRow.Cells[0].Value);
                    Properties.Settings.Default.main_spec_id = selected_id;
                    Properties.Settings.Default.Save();
                }
            }
            dataGrid_specimens.Rows.Clear();
            //удалить предыдущие
            conn_str = Get_conn_string(Properties.Settings.Default.server, Properties.Settings.Default.port,
                Properties.Settings.Default.database, Parent_form.cbox_username.Text, Parent_form.txtbox_pass.Text);
            using (MySqlConnection conn = New_connection(conn_str))
            {
                try
                {
                    conn.Open();
                    New_specimen_data specimen_curr = new New_specimen_data();
                    string sort_dec_asc = "";
                    if (pic_change)
                    {
                        sort_dec_asc = " ASC";
                    }
                    else sort_dec_asc = " DESC";
                    //string sqlcom = "SELECT * FROM Specimens";
                    string sort_sql = "";
                    switch (combox_sort.SelectedIndex)
                    {
                        case 0:
                            sort_sql = "ORDER BY materials.name";
                            break;
                        case 1:
                            sort_sql = "ORDER BY type.name";
                            break;
                        case 2:
                            sort_sql = "ORDER BY projects.name";
                            break;
                        case 3:
                            sort_sql = "ORDER BY specimens.date_prep";
                            break;
                        case 4:
                            sort_sql = "ORDER BY producers.surname";
                            break;
                        case 5:
                            sort_sql = "ORDER BY storage.name";
                            break;
                        case 6:
                            sort_sql = "ORDER BY state.name";
                            break;
                    }
                    if (sort_sql == "")
                    {
                        sort_dec_asc = "";
                    }
                    string sql_filtres = "";
                    if (filt_master.is_special_filter == false)
                    {
                        sql_filtres = do_filtres_for_SQL(filt_master.common_filters);
                        //richTextBox_special_filt.Text = sql_filtres;
                    }
                    else
                    {
                        sql_filtres = filt_master.special_filter;
                    }
                    string sqlcom = "SELECT DISTINCT specimens.idspecimens, materials.name, type.name, projects.name, specimens.date_prep, producers.surname, storage.name, " +
                        "state.name, storage_position.position, specimens.priority " +
                    "FROM test2base.specimens " +
                    "LEFT OUTER JOIN test2base.materials ON test2base.specimens.id_material = test2base.materials.id_material " +
                    "LEFT OUTER JOIN test2base.type ON specimens.id_treat_type = type.id_type " +
                    "LEFT OUTER JOIN test2base.projects ON specimens.id_project = projects.id_project " +
                    "LEFT OUTER JOIN test2base.producers ON specimens.id_producer = producers.id_producer " +
                    "LEFT OUTER JOIN test2base.storage_position ON specimens.idspecimens = storage_position.id_specimen " +
                    "LEFT OUTER JOIN test2base.state ON specimens.id_state = state.id_state "+
                    "LEFT OUTER JOIN test2base.setup_specimen ON specimens.idspecimens = setup_specimen.id_specimen " +
                    "LEFT OUTER JOIN test2base.storage ON storage_position.id_storage = storage.id_storage";
                    if (filt_master.is_special_filter == false)
                    {
                        sqlcom = sqlcom +
                            " WHERE (specimens.date_prep >= '" + dateTimePicker_start.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                    " AND specimens.date_prep <= '" + dateTimePicker_end.Value.ToString("yyyy-MM-dd HH:mm:ss") + "') " +
                        sql_filtres + sort_sql + sort_dec_asc;
                    }
                    else
                    {
                        sqlcom += sql_filtres;
                    }


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
                                        //MessageBox.Show("Нужно не более: "+show_only_spec.ToString()+" , А сейчас: "+num.ToString());
                                        string srt;
                                        if (DateTime.TryParse(reader[4].ToString(), out DateTime temp_dat))
                                        {

                                            //srt = temp_dat.ToShortDateString();
                                            srt = temp_dat.ToString("yyyy.MM.dd");
                                        }
                                        else
                                        {
                                            srt = reader[4].ToString();
                                        }

                                        dataGrid_specimens.Rows.Add(reader[0].ToString(), reader[1].ToString(), reader[2].ToString(),
                                            reader[3].ToString(), srt, reader[5].ToString(), reader[6].ToString() + " " + reader[8].ToString(), reader[7].ToString(),reader[9].ToString());
                                        // цветовая дифференциация таблицы
                                        int ind = Get_index_datagrid(reader[0].ToString(),0);
                                        //MessageBox.Show("Index = "+ind.ToString());
                                        if (ind != -1)
                                        {
                                            switch (reader[7].ToString())
                                            {
                                                case "Ready for APT":                                                    
                                                    dataGrid_specimens.Rows[ind].DefaultCellStyle.BackColor = Color.LightPink;
                                                    break;
                                                case "APT done, need TEM":
                                                    dataGrid_specimens.Rows[ind].DefaultCellStyle.BackColor = Color.LightYellow;
                                                    break;
                                                case "Storage":
                                                    dataGrid_specimens.Rows[ind].DefaultCellStyle.BackColor = Color.LightGray;
                                                    break;
                                                case "APT done":
                                                    dataGrid_specimens.Rows[ind].DefaultCellStyle.BackColor = Color.LightGreen;
                                                    break;
                                            }
                                        }
                                    }
                                    num++;
                                }
                                reader.Close();
                            }
                            //else MessageBox.Show("nodata in refresh");
                        }
                        conn.Close();

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            //Select_index();
            //пытаемся сохранить выбранный образец
            if (selected_id!=-1)
            {
                for (int i=0;i<dataGrid_specimens.Rows.Count;i++)
                {
                    if (dataGrid_specimens.Rows[i].Cells[0].Value!=null)
                    {
                        if (Convert.ToInt32(dataGrid_specimens.Rows[i].Cells[0].Value)==selected_id)
                        {
                            dataGrid_specimens.Rows[i].Selected = true;
                            dataGrid_specimens.CurrentCell = dataGrid_specimens.Rows[i].Cells[0];
                            //dataGrid_specimens.Rows[i].Cells[0].                            
                            //MessageBox.Show("сохранный номер строки" + selected_id.ToString());
                            //dataGrid_specimens.SelectedRows.
                        }
                    }
                }
            }
            //TEST обновляем комбо-боксы
            //НЕВКЛЮЧАТЬ делает много окннектов к базе(хз почему)
            //Fill_combo();
            //пробуем встроить фильтры
            if (!on_load)
            {
                Deal_with_buttons();
            }
        }
        private void Fill_one_combo(string colname, MySqlConnection conect, string table_name, string combo)
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
                                        case "producers":
                                            combox_producer.Items.Add(reader[0].ToString());
                                            break;
                                        case "storage":

                                            combox_storage.Items.Add(reader[0].ToString());
                                            combox_move_to.Items.Add(reader[0].ToString());
                                            break;
                                        case "projects":
                                            combox_project.Items.Add(reader[0].ToString());
                                            break;
                                        case "materials":
                                            combox_material.Items.Add(reader[0].ToString());
                                            break;
                                        case "response":
                                            combox_response.Items.Add(reader[0].ToString());
                                            break;
                                        case "researchers":
                                            combox_researcher.Items.Add(reader[0].ToString());
                                            break;
                                        case "f_type":
                                            ch_listbox_type.Items.Add(reader[0].ToString(), true);
                                            break;
                                        case "f_material":
                                            ch_listbox_material.Items.Add(reader[0].ToString(), true);
                                            break;
                                        case "f_project":
                                            ch_listbox_project.Items.Add(reader[0].ToString(), true);
                                            break;
                                        case "f_state":
                                            ch_listbox_state_f.Items.Add(reader[0].ToString(), true);
                                            break;
                                        case "setup":
                                            combox_setup.Items.Add(reader[0].ToString());
                                            break;
                                        case "storage_filter":                                            
                                            ch_listbox_storage_f.Items.Add(reader[0].ToString(),true);
                                            break;
                                        case "type":
                                            combox_treat_type.Items.Add(reader[0].ToString());
                                            break;
                                        case "setup_type":
                                            ch_listbox_setup_inf.Items.Add(reader[0].ToString(),false);
                                            break;
                                        case "setups_add_new":
                                            ch_listbox_setups_add_new.Items.Add(reader[0].ToString(), false);
                                            break;
                                    }
                                }
                                reader.Close();
                            }
                        }
                        //проверить выполнен ли запрос
                        
                    }
                    conect.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button1);
            }
        }
        private void Clear_combo()
        {
            combox_producer.Items.Clear();
            combox_storage.Items.Clear();
            combox_move_to.Items.Clear();
            combox_project.Items.Clear();
            combox_material.Items.Clear();
            combox_response.Items.Clear();
            combox_researcher.Items.Clear();
            combox_setup.Items.Clear();
            combox_treat_type.Items.Clear();
            ch_listbox_type.Items.Clear();
            ch_listbox_material.Items.Clear();
            ch_listbox_project.Items.Clear();
            ch_listbox_state_f.Items.Clear();
            GC.Collect();
        }
        private void Fill_combo()
        {
            conn_str = Get_conn_string(Properties.Settings.Default.server, Properties.Settings.Default.port,
                Properties.Settings.Default.database, Parent_form.cbox_username.Text, Parent_form.txtbox_pass.Text);
            //MessageBox.Show(conn_str);
            Clear_combo();
            using (MySqlConnection conn = New_connection(conn_str))
            {
                //producers
                Fill_one_combo("surname", conn, "producers", "producers");
                //projects
                Fill_one_combo("name", conn, "projects", "projects");
                //storage
                Fill_one_combo("name", conn, "storage", "storage");
                if (combox_storage.Text!="")
                {
                    Fill_num_combo(combox_storage.Text, combox_pos_add);
                    combox_pos_add.Text = "";
                }
                //researchers
                Fill_one_combo("surname", conn, "producers", "researchers");
                //тип установки
                Fill_one_combo("name", conn, "setups", "setup");
                //тип изготовления образца
                Fill_one_combo("name", conn, "type", "type");
                //установки на которых можно делать исследования
                Fill_one_combo("Name",conn,"setups","setup_type");
                //установки на которых можно делать исследования вкладка добавление нового
                Fill_one_combo("Name", conn, "setups", "setups_add_new");

                //заполняем фильтры
                //фильтр тип образца
                Fill_one_combo("name", conn, "type", "f_type");
                //фильтр успешности
                Fill_one_combo("name", conn, "state", "f_state");
                //фильтр материала
                Fill_one_combo("name", conn, "materials", "f_material");
                //фильтр проекта
                Fill_one_combo("name", conn, "projects", "f_project");
                //фильтр Storage
                Fill_one_combo("name", conn, "storage", "storage_filter");                
            }
        }
        private string Fill_info_text_sql(int spec_id, MySqlConnection connect, string table_name, string col_name, string id_join, string id2_join)
        {
            string ans = "";
            try
            {
                connect.Open();
                string sqlcom = "SELECT " + table_name + "." + col_name + ", specimens.idspecimens FROM test2base.specimens " +
                    "INNER JOIN test2base." + table_name + " ON specimens." + id2_join + " = " + table_name + "." + id_join +
                    " WHERE idspecimens = " + spec_id.ToString();
                //MessageBox.Show(sqlcom);
                using (MySqlCommand comand = new MySqlCommand(sqlcom, connect))
                {
                    using (MySqlDataReader reader = comand.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ans = reader[0].ToString();
                            }
                            reader.Close();
                        }
                    }
                    //проверить выполнен ли запрос
                    connect.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            return ans;
        }
        private void Fill_info_foto(int select_id, MySqlConnection connect, string table_name, string col_name)
        {
            try
            {
                connect.Open();
                string sqlcom = "SELECT " + table_name + "." + col_name + " FROM test2base.specimens" +
                //string sqlcom = "SELECT " + table_name + "." + col_name + ", specimens.idspecimens FROM test2base.specimens" +
                    " WHERE idspecimens = " + select_id.ToString();
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
                                    case "place_foto_bef":
                                        string image_path = reader[0].ToString();
                                        if (Directory.Exists(image_path))
                                        {
                                            //значит картинка есть
                                            //нужно получить количество
                                            //загрузить в список
                                            info_files_paths_bef = Directory.GetFiles(image_path);
                                            int image_count = info_files_paths_bef.Count();
                                            //загрузить первые 3
                                            int num = 0;
                                            foreach (string path in info_files_paths_bef)
                                            {
                                                string ext_name = Path.GetExtension(path);
                                                if (ext_name == ".jpg" || ext_name == ".jpeg" || ext_name == ".png" || ext_name == ".bmp" || ext_name == ".tiff" 
                                                    || ext_name == ".JPG" || ext_name == ".JPEG" || ext_name == ".PNG" || ext_name == ".BMP" || ext_name == ".TIFF"
                                                    || ext_name == ".TIF" || ext_name == ".tif")
                                                {
                                                    num++;
                                                    switch (num)
                                                    {
                                                        case 1:
                                                            if (picbox_inf_bef_1.Image != null) picbox_inf_bef_1.Image.Dispose();
                                                            picbox_inf_bef_1.Image = Image.FromFile(path);
                                                            break;
                                                        case 2:
                                                            if (picbox_inf_bef_2.Image != null) picbox_inf_bef_2.Image.Dispose();
                                                            picbox_inf_bef_2.Image = Image.FromFile(path);
                                                            break;
                                                        case 3:
                                                            if (picbox_inf_bef_3.Image != null) picbox_inf_bef_3.Image.Dispose();
                                                            picbox_inf_bef_3.Image = Image.FromFile(path);
                                                            break;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Clear_pics_info(1);
                                        }
                                        break;
                                    case "place_foto_after":
                                        string image_path_2 = reader[0].ToString();
                                        if (Directory.Exists(image_path_2))
                                        {
                                            //значит картинка есть
                                            //нужно получить количество
                                            //загрузить в список
                                            info_files_paths_aft = Directory.GetFiles(image_path_2);
                                            int image_count = info_files_paths_aft.Count();
                                            //загрузить первые 3
                                            int num = 0;
                                            foreach (string path in info_files_paths_aft)
                                            {
                                                string ext_name = Path.GetExtension(path);
                                                if (ext_name == ".jpg" || ext_name == ".jpeg" || ext_name == ".png" || ext_name == ".bmp" || ext_name == ".tiff" 
                                                    || ext_name == ".JPG" || ext_name == ".JPEG" || ext_name == ".PNG" || ext_name == ".BMP" || ext_name == ".TIFF"
                                                    || ext_name == ".TIF" || ext_name == ".tif")
                                                {
                                                    num++;
                                                    switch (num)
                                                    {
                                                        case 1:
                                                            if (picbox_inf_aft_1.Image != null) picbox_inf_aft_1.Image.Dispose();
                                                            picbox_inf_aft_1.Image = Image.FromFile(path);
                                                            break;
                                                        case 2:
                                                            if (picbox_inf_aft_2.Image != null) picbox_inf_aft_2.Image.Dispose();
                                                            picbox_inf_aft_2.Image = Image.FromFile(path);
                                                            break;
                                                        case 3:
                                                            if (picbox_inf_aft_3.Image != null) picbox_inf_aft_3.Image.Dispose();
                                                            picbox_inf_aft_3.Image = Image.FromFile(path);
                                                            break;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Clear_pics_info(2);
                                        }
                                        break;                                    
                                    case "comments":
                                        rich_txtbox_comments_info.Text = reader[0].ToString();
                                        break;
                                }
                            }

                            reader.Close();
                        }
                        else
                        {
                            if (col_name == "place_foto_after" || col_name == "place_foto_bef")
                            {
                                if (picbox_inf_bef_1.Image != null) picbox_inf_bef_1.Image.Dispose();
                                if (picbox_inf_bef_2.Image != null) picbox_inf_bef_2.Image.Dispose();
                                if (picbox_inf_bef_3.Image != null) picbox_inf_bef_3.Image.Dispose();
                            }
                        }
                    }
                    //проверить выполнен ли запрос

                    connect.Close();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        private void Clear_one_picbox(PictureBox box)
        {
            if (box.Image != null)
            {
                box.Image.Dispose();
                box.Image = null;
            }
        }
        /// <summary>
        /// удаляет картинки  1 - before, 2 - after
        /// </summary>
        /// <param name="type"> 1 - before, 2 - after</param>
        private void Clear_pics_info(int type)
        {
            switch (type)
            {
                case 1:
                    // удалить картинки до
                    Clear_one_picbox(picbox_inf_bef_1);
                    Clear_one_picbox(picbox_inf_bef_2);
                    Clear_one_picbox(picbox_inf_bef_3);
                    break;
                case 2:
                    // удалить картинки после
                    Clear_one_picbox(picbox_inf_aft_1);
                    Clear_one_picbox(picbox_inf_aft_2);
                    Clear_one_picbox(picbox_inf_aft_3);
                    break;
            }
            GC.Collect();
        }
        /// <summary>
        /// Заполняем лист бокс галочками
        /// </summary>
        private void Fill_list_box(int spec_id, MySqlConnection connect, CheckedListBox l_box)
        {
            try
            {
                if (l_box.Items.Count > 0)
                {
                    for (int i = 0; i < l_box.Items.Count; ++i)
                    {
                        l_box.SetItemChecked(i, false);
                    }

                    connect.Open();
                    string sqlcom = "SELECT Name FROM test2base.setup_specimen " +
                        "INNER JOIN test2base.setups ON test2base.setup_specimen.id_setup = test2base.setups.id_setups " +
                        "WHERE id_specimen = " + spec_id.ToString();
                    using (MySqlCommand comand = new MySqlCommand(sqlcom, connect))
                    {
                        using (MySqlDataReader reader = comand.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {

                                while (reader.Read())
                                {
                                    if (l_box.Items.Contains(reader[0].ToString()))
                                    {
                                        l_box.SetItemChecked(l_box.Items.IndexOf(reader[0].ToString()), true);
                                    }
                                    //MessageBox.Show(spec_id.ToString() + " on setup" + reader[0].ToString());
                                }
                                reader.Close();
                            }
                        }
                        connect.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка в блоке заполнения ch-list-box-setups", MessageBoxButtons.OK, MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                connect.Close();
            }
        }
        private void Fill_info_text(int index)
        {
            //простое заполнение из грида
            if (dataGrid_specimens.Rows[index].Cells[1].Value != null) txtbox_material_inf.Text = dataGrid_specimens.Rows[index].Cells[1].Value.ToString();
            if (dataGrid_specimens.Rows[index].Cells[5].Value != null) txtbox_producer_inf.Text = dataGrid_specimens.Rows[index].Cells[5].Value.ToString();
            if (dataGrid_specimens.Rows[index].Cells[2].Value != null) txtbox_type_inf.Text = dataGrid_specimens.Rows[index].Cells[2].Value.ToString();
            if (dataGrid_specimens.Rows[index].Cells[4].Value != null) txtbox_date_inf.Text = dataGrid_specimens.Rows[index].Cells[4].Value.ToString();
            if (dataGrid_specimens.Rows[index].Cells[3].Value != null) txtbox_project_inf.Text = dataGrid_specimens.Rows[index].Cells[3].Value.ToString();
            if (dataGrid_specimens.Rows[index].Cells[7].Value != null) txtbox_state_inf.Text = dataGrid_specimens.Rows[index].Cells[7].Value.ToString();
            if (dataGrid_specimens.Rows[index].Cells[6].Value != null)
            {
                txtbox_storage_inf.Text = dataGrid_specimens.Rows[index].Cells[6].Value.ToString();
                switch (dataGrid_specimens.Rows[index].Cells[6].Value.ToString())
                {
                    case "ПАЗЛ 1":
                        combox_setup.SelectedIndex = combox_setup.Items.IndexOf("ПАЗЛ");
                        break;
                    case "АТЛАЗ 1":
                        combox_setup.SelectedIndex = combox_setup.Items.IndexOf("АТЛАЗ");
                        break;
                    case "ЛАЗТ 1":
                        combox_setup.SelectedIndex = combox_setup.Items.IndexOf("ЛАЗТ");
                        break;
                    default:
                        combox_setup.Text = "";
                        combox_setup.SelectedIndex = -1;
                        break;
                }
                //txtbox_m
            }
            if (dataGrid_specimens.Rows[index].Cells[6].Value != null)
            {
                txtbox_move_from.Text = dataGrid_specimens.Rows[index].Cells[6].Value.ToString();
            }
            // далее по SQL запросам заполняем всё
            conn_str = Get_conn_string(Properties.Settings.Default.server, Properties.Settings.Default.port,
                Properties.Settings.Default.database, Parent_form.cbox_username.Text, Parent_form.txtbox_pass.Text);
            using (MySqlConnection conn = New_connection(conn_str))
            {
                //заполняем остальные поля                
                if (dataGrid_specimens.Rows[index].Cells[0].Value != null)
                {
                    //treatment
                    int i = Convert.ToInt32(dataGrid_specimens.Rows[index].Cells[0].Value);
                    txtbox_treat_inf.Text = Fill_info_text_sql(i, conn, "treatment", "Name", "id_treatment", "id_treatment");
                    //response
                    txtbox_respon_inf.Text = Fill_info_text_sql(i, conn, "producers", "surname", "id_producer", "id_respon");
                    //fill composition
                    txt_composition.Text = Fill_info_text_sql(i, conn, "materials", "composition", "id_material", "id_material");
                    //storage position
                    Fill_info_foto(i, conn, "specimens", "stor_position");
                    //комменты
                    Fill_info_foto(i, conn, "specimens", "comments");
                    //установки
                    Fill_list_box(i, conn, ch_listbox_setup_inf);
                    //Fill_list_box(i, conn, ch_listbox_setup_inf);

                    //foto
                    //удалить картинки "before" и "after"
                    Clear_pics_info(1);
                    Clear_pics_info(2);
                    if (chbox_no_pics.Checked != true)
                    {                        
                        Fill_info_foto(i, conn, "specimens", "place_foto_bef");
                        Fill_info_foto(i, conn, "specimens", "place_foto_after");
                    }                       
                }
            }
        }
        private void Fill_information()
        {
            //заполняем информацию
            //берем её из дата грида            
            if (dataGrid_specimens.Rows.Count > 0 && dataGrid_specimens.SelectedRows != null && dataGrid_specimens.CurrentCell!=null)
            {
                //MessageBox.Show("ОТладка  чило строк="+dataGrid_specimens.Rows.Count.ToString()+
                    //"\n [0] строка равна = "+dataGrid_specimens.CurrentCell.Value.ToString());
                int Sel_index = dataGrid_specimens.SelectedRows[0].Index;
                Fill_info_text(Sel_index);
            }
        }
        private void Form_specimens_Load(object sender, EventArgs e)
        {
            on_load = true;
            show_only_spec = Properties.Settings.Default.show_only_specimens;
            if (combox_showonly.Text != "All") combox_showonly.Text = show_only_spec.ToString();
            btn_up_down.Image = Properties.Resources.down;
            pic_change = false;
            //combox_material.Text = Properties.Settings.Default.material_add;
            combox_material.Text = "";
            combox_producer.Text = Properties.Settings.Default.producer;
            //combox_producer.Text = "";
            //combox_project.Text = Properties.Settings.Default.project;
            combox_project.Text = "";
            combox_storage.Text = Properties.Settings.Default.storage;
            //combox_treatment.Text = Properties.Settings.Default.treatment;
            combox_treatment.Text = "";
            //combox_treat_type.Text = Properties.Settings.Default.type_prep;
            combox_treat_type.Text = "";
            //combox_response.Text = Properties.Settings.Default.respons;
            combox_response.Text = "";
            chbox_no_pics.Checked = Properties.Settings.Default.no_pics;
            combox_priority.Text = "";
            combox_priority.Items.Clear();            
            combox_priority.Items.Add("5 Now");           //5
            combox_priority.Items.Add("4 High");          //4
            combox_priority.Items.Add("3 Normal");        //3
            combox_priority.Items.Add("2 Low");           //2
            combox_priority.Items.Add("1 One fine day");  //1  
            combox_change_priority.Text = "";
            combox_change_priority.Items.Clear();
            combox_change_priority.Items.Add("5 Now");           //5
            combox_change_priority.Items.Add("4 High");          //4
            combox_change_priority.Items.Add("3 Normal");        //3
            combox_change_priority.Items.Add("2 Low");           //2
            combox_change_priority.Items.Add("1 One fine day");  //1 
            date_time_add_edit.Format = DateTimePickerFormat.Custom;
            date_time_add_edit.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            //настройка форм на вкладке research
            combox_succ.Items.Clear();
            combox_succ.Items.Add("+");
            combox_succ.Items.Add("-");
            combox_succ.Items.Add("+/-");
            combox_succ.Items.Add("-/+");
            combox_succ.Text = Properties.Settings.Default.res_succ;
            txtbox_temperature.Text = Properties.Settings.Default.res_temper.ToString();
            txtbox_las_pow.Text = Properties.Settings.Default.res_laser_power.ToString();
            //MessageBox.Show(date_time_add_edit.Value.ToString("u"));
            //заполняем все списки из базы
            //очистки списков НЕТ
            Fill_combo();
            toolTip_specimens.SetToolTip(picbox_inf_bef_1, "Double click for dir open");
            toolTip_specimens.SetToolTip(picbox_inf_bef_2, "Double click for dir open");
            toolTip_specimens.SetToolTip(picbox_inf_bef_3, "Double click for dir open");
            toolTip_specimens.SetToolTip(picbox_inf_aft_1, "Double click for dir open");
            toolTip_specimens.SetToolTip(picbox_inf_aft_2, "Double click for dir open");
            toolTip_specimens.SetToolTip(picbox_inf_aft_3, "Double click for dir open");
            toolTip_specimens.SetToolTip(picbox_before_big, "Click for dir open");
            toolTip_specimens.SetToolTip(txtbox_data_dir, "Double click for choose dir");
            toolTip_specimens.SetToolTip(dataGrid_specimens, "Right click for history of selected specimen");
            //dataGrid_specimens.cellt
            combox_sort.SelectedIndex = 3;
            Properties.Settings.Default.main_res_id = -1;
            Properties.Settings.Default.main_spec_id = -1;            
            dateTimePicker_end.Value = DateTime.Now.AddYears(1);
            DateTime tem_dat;
            tem_dat = dateTimePicker_end.Value;
            dateTimePicker_start.Value = tem_dat.AddYears(-20);
            if (Properties.Settings.Default.ini_split_inf!=0) split_inf.SplitterDistance = Properties.Settings.Default.ini_split_inf;
            if (Properties.Settings.Default.ini_split_add_new != 0) splitContainer_add_new.SplitterDistance = Properties.Settings.Default.ini_split_add_new;
            //грузим фильтры из json
            filt_master.common_filters = new List<string>();
            Load_def_json(@"\Settings\test_json.json");
            //richTextBox_special_filt.Text = filt_master.special_filter;
            filt_master.is_special_filter = false;
            //btn_sql_filter_special.BackColor = Color.Red;
            Refresh_datagrid();
            Fill_information();
            on_load = false;
            //запуск таймера на циклическое обновление
            timer_for_refresh.Start();
        }

        private void Btn_refresh_Click(object sender, EventArgs e)
        {
            Refresh_datagrid();
        }

        private void Tab_page_new_edit_Enter(object sender, EventArgs e)
        {
            //MessageBox.Show("test");            

        }
        private bool Ch_fields()
        {
            //проврека на заполненность всех полей на форме ADD_EDIT
            if (combox_treat_type.Text != "" && combox_treatment.Text != "" && combox_storage.Text != "" &&
                combox_project.Text != "" && combox_producer.Text != "" && combox_material.Text != "" && 
                date_time_add_edit.Text != "" && combox_pos_add.Text != "" && (ch_listbox_setups_add_new.CheckedItems.Count>0))
            {
                return true;
            }
            else
            {
                MessageBox.Show("Not all fields are filled! \n Some of them are empty \n May be Target setups need to be selected");
                return false;
            }

        }

        /// <summary>
        /// Ищем и если нужно добавляем новую запись
        /// surname - искомое
        /// name2 - название поля(столбца) таблицы
        /// col_name - что ищем (обычно ИД)
        /// возращает ИД найденной или созданной записи
        /// </summary>
        /// <param name="surname"></param>
        /// <param name="connect"></param>
        /// <param name="table_name"></param>
        /// <param name="col_name"></param>
        /// <param name="name2"></param>
        /// <returns></returns>
        private string Check_for_exist(string surname, MySqlConnection connect, string table_name, string col_name, string name2)
        {
            bool need_new = false;
            string ans = "";
            connect.Open();
            string sqlcom = "SELECT " + col_name + " FROM test2base." + table_name + " WHERE " + name2 + " = '" + surname + "'";
            using (MySqlCommand comand = new MySqlCommand(sqlcom, connect))
            {
                using (MySqlDataReader reader = comand.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ans = reader[0].ToString();
                            //MessageBox.Show("хорошо, есть уже такой, ИД=" + ans);
                        }
                        reader.Close();
                    }
                    else
                    {
                        need_new = true;
                        reader.Close();
                    }
                }
            }
            //добавляем нового пользователя
            if (need_new)
            {
                switch (table_name)
                {
                    case "producers":
                        Form add_new_producer = new New_producer_add();
                        add_new_producer.ShowDialog();
                        add_new_producer.Dispose();
                        New_producer new_prod = new New_producer
                        {
                            access = Properties.Settings.Default.pro_access,
                            name = Properties.Settings.Default.pro_name,
                            surname = Properties.Settings.Default.pro_surname
                        };
                        string sqlcom_new = "INSERT INTO test2base." + table_name + " (name,surname,access) VALUES (@name,@surname,@access)";
                        using (MySqlCommand comand = new MySqlCommand(sqlcom_new, connect))
                        {
                            comand.Parameters.AddWithValue("@name", new_prod.name);
                            comand.Parameters.AddWithValue("@surname", new_prod.surname);
                            comand.Parameters.AddWithValue("@access", new_prod.access);
                            _ = comand.ExecuteNonQuery();
                        }
                        string sqlcom_2 = "SELECT max(" + col_name + ") FROM test2base." + table_name;
                        using (MySqlCommand commm = new MySqlCommand(sqlcom_2, connect))
                        {
                            using (MySqlDataReader reader = commm.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        ans = reader[0].ToString();
                                        //MessageBox.Show("новый ид" + table_name + "=" + ans);
                                    }
                                    reader.Close();
                                }
                            }
                        }
                        break;
                    case "materials":
                        //add_new_material
                        Properties.Settings.Default.material_name = surname;
                        Properties.Settings.Default.Save();
                        Form add_new_material = new Materials_new();
                        //add_new_material.ShowDialog();
                        if (add_new_material.ShowDialog() == DialogResult.OK)
                        {
                            //add_new_material.Dispose();

                            New_material new_mat = new New_material
                            {
                                name = Properties.Settings.Default.material_name,
                                compostion = Properties.Settings.Default.material_composition
                            };
                            string sqlcom_3 = "INSERT INTO test2base." + table_name + " (name,composition) VALUES (@name,@comp)";
                            using (MySqlCommand comand = new MySqlCommand(sqlcom_3, connect))
                            {
                                comand.Parameters.AddWithValue("@name", new_mat.name);
                                comand.Parameters.AddWithValue("@comp", new_mat.compostion);
                                _ = comand.ExecuteNonQuery();
                            }
                            sqlcom_3 = "SELECT max(" + col_name + ") FROM test2base." + table_name;
                            using (MySqlCommand comm = new MySqlCommand(sqlcom_3, connect))
                            {
                                using (MySqlDataReader reader = comm.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            ans = reader[0].ToString();
                                            //MessageBox.Show("новый ид" + table_name + "=" + ans);
                                        }
                                        reader.Close();
                                    }
                                }
                            }
                        }
                        else
                        {
                            //запрещаем создание образца (ползователь не добавил материал)
                            specimen_new_accepted = false;
                        }
                        break;
                    case "storage":
                        //новое создать нельзя, только администратору
                        MessageBox.Show("No accept new storage place creation");
                        specimen_new_accepted = false;
                        break;
                    case "type":
                        MessageBox.Show("No accept new specimen type creation");
                        specimen_new_accepted = false;
                        break;
                    case "treatment":
                        //add new treatment
                        Properties.Settings.Default.treatment = surname;
                        Properties.Settings.Default.Save();
                        New_treatment treat_new = new New_treatment
                        {
                            name = Properties.Settings.Default.treatment,
                            dose = "",
                            irr_ions = "",
                            temper = "",
                            time = "",
                            comments = "",
                            energy = "",
                            irr_type = "1"
                        };
                        string sqlcom_4 = "INSERT INTO test2base." + table_name + " (name) VALUES (@name)";
                        using (MySqlCommand comand = new MySqlCommand(sqlcom_4, connect))
                        {
                            comand.Parameters.AddWithValue("@name", treat_new.name);
                            //comand.Parameters.AddWithValue("@comp", treat_new);
                            _ = comand.ExecuteNonQuery();
                        }
                        //какой-то тсранный выбор ИД, основанный на том, что последний и есть максимальный
                        string sqlcom_5 = "SELECT max(" + col_name + ") FROM test2base." + table_name;
                        using (MySqlCommand comm = new MySqlCommand(sqlcom_5, connect))
                        {
                            using (MySqlDataReader reader = comm.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        ans = reader[0].ToString();
                                        //MessageBox.Show("новый ид"+table_name+"=" + ans);
                                    }
                                    reader.Close();
                                }
                            }
                        }
                        break;
                    case "projects":
                        //новое создать нельзя, только администратору
                        MessageBox.Show("No accept new project creation");
                        specimen_new_accepted = false;
                        break;
                }
            }
            connect.Close();
            return ans;
        }
        private New_specimen_data Check_for_new_data(New_specimen_data raw_data, MySqlConnection connect)
        {
            //проверяем каждое поле на предмет существования в таблицах            
            //Изготовитель
            raw_data.producer = Check_for_exist(raw_data.producer, connect, "producers", "id_producer", "surname");
            //материал
            raw_data.material = Check_for_exist(raw_data.material, connect, "materials", "id_material", "name");
            //Место хранения
            raw_data.storage = Check_for_exist(raw_data.storage, connect, "storage", "id_storage", "name");
            //тип
            raw_data.type = Check_for_exist(raw_data.type, connect, "type", "id_type", "name");
            //обработка облучение/отжиг и т.д.
            raw_data.treatment = Check_for_exist(raw_data.treatment, connect, "treatment", "id_treatment", "name");
            //ответственный
            raw_data.resonse = Check_for_exist(raw_data.resonse, connect, "producers", "id_producer", "surname");
            //проект
            raw_data.project = Check_for_exist(raw_data.project, connect, "projects", "id_project", "name");

            return raw_data;
        }
        private string Copy_fotos(string old_directory, string directory_new, int type, int spec_id)
        {
            //type 1 - фото до
            //2 - фото после
            //3 - фото СЭМ  
            //4 - фото после к любому образцу
            //MessageBox.Show("1 старая директория "+old_directory);
            if (!Directory.Exists(directory_new))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(directory_new);
                dirInfo.Create();
                
            }
            //MessageBox.Show("2 старая директория " + old_directory);
            directory_new += @"\" + spec_id.ToString();
            //MessageBox.Show("3 новая директория 1" + directory_new);
            switch (type)
            {
                case 1:
                    directory_new += @"\TEM before";
                    break;
                case 2:
                    directory_new += @"\TEM after";
                    break;
                case 3:
                    directory_new += @"\SEM";
                    break;
            }
            if (!Directory.Exists(directory_new))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(directory_new);
                dirInfo.Create();
            }
            //MessageBox.Show("4 новая директория 2" + directory_new);
            if (old_directory != "")
            {
                string[] pic_list = Directory.GetFiles(old_directory);
                // Copy picture files.
                //MessageBox.Show("5");
                foreach (string f in pic_list)
                {
                    // Remove path from the file name.
                    string fName = f.Substring(old_directory.Length + 1);
                    //MessageBox.Show("6");
                    // Use the Path.Combine method to safely append the file name to the path.
                    // Will overwrite if the destination file already exists.
                    File.Copy(Path.Combine(old_directory, fName), Path.Combine(directory_new, fName), true);
                }
            }
            return directory_new;
        }
        /// <summary>
        /// проверка есть ли такое состояние, true - да
        /// </summary>
        /// <param name="c"></param>
        /// <param name="tr"></param>
        /// <param name="mat"></param>
        /// <returns></returns>
        private bool Is_state_exist(MySqlConnection c,string tr, string mat)
        {
            bool ans = false;
            c.Open();            
            string com = "SELECT materialstate.Name FROM test2base.materialstate " +
                "WHERE (id_treatment="+tr+") AND " +
                "(id_material='"+mat+"')";
            using (MySqlCommand comand = new MySqlCommand(com, c))
            {                
                using (MySqlDataReader reader = comand.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        ans = true;
                        reader.Close();
                    }
                }
            }
            c.Close();
            //MessageBox.Show("find such state = "+ans.ToString());
            return ans;
        }
        /// <summary>
        /// добавляем новое состояние
        /// </summary>
        /// <param name="ccc"></param>
        /// <param name="treat"></param>
        /// <param name="material"></param>
        /// <param name="name"></param>
        /// <param name="id_project"></param>
        private void Push_state(MySqlConnection ccc, string treat, string material, string name, string id_project)
        {
            //если нет такого состояния - создаем
            if (!Is_state_exist(ccc, treat, material ))
            {
                //создаем новое состояние
                ccc.Open();
                string com = "INSERT INTO test2base.materialstate (id_treatment, name,  id_material, id_project) " +
                    "VALUES ("+treat+", '"+name+"', "+material+", "+id_project+") ";
                using (MySqlCommand comand = new MySqlCommand(com, ccc))
                {
                    comand.ExecuteNonQuery();
                }
                //создаем связь (состояние материала) - состояние
                com = "INSERT INTO test2base.materialstate_state (id_materialstate, id_state) " +
                    "VALUES ((SELECT materialstate.id_materialstate FROM test2base.materialstate " +
                    "WHERE (materialstate.id_treatment = "+treat+") AND (materialstate.id_material = "+material+ ") AND (materialstate.id_project = id_project)), " +
                    "'4' )";
                using (MySqlCommand comand = new MySqlCommand(com, ccc))
                {
                    comand.ExecuteNonQuery();
                }
                ccc.Close();
            }            
        }
        private void New_specimen()
        {
            conn_str = Get_conn_string(Properties.Settings.Default.server, Properties.Settings.Default.port,
                Properties.Settings.Default.database, Parent_form.cbox_username.Text, Parent_form.txtbox_pass.Text);
            int id_new = -1;
            using (MySqlConnection conn = New_connection(conn_str))
            {
                try
                {
                    //проверяем есть ли уже все записи в таблицах
                    date_time_add_edit.Value = DateTime.Now;
                    New_specimen_data new_spec = new New_specimen_data
                    {
                        //datetime = date_time_add_edit.Text,                        
                        datetime = date_time_add_edit.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                        material = combox_material.Text,
                        producer = combox_producer.Text,
                        project = combox_project.Text,
                        resonse = combox_response.Text,
                        type = combox_treat_type.Text,
                        //состояние
                        state = 1, //1 - готовый образец
                        storage = combox_storage.Text,
                        treatment = combox_treatment.Text,
                        foto_after = "",
                        foto_before = "",
                        stor_pos = combox_pos_add.Text,
                        priority=combox_priority.Text
                    };
                    //если нет, то добавляем новые
                    specimen_new_accepted = true;
                    string state_name = new_spec.material + " " + new_spec.treatment;
                    Check_for_new_data(new_spec, conn);                     
                    if (specimen_new_accepted)
                    {
                        if (!(new_spec.priority== "4 High" || new_spec.priority == "5 Now" || new_spec.priority == "3 Normal" || new_spec.priority == "2 Low" || new_spec.priority == "1 One fine day"))
                        {
                            new_spec.priority = "2 Low";
                        }
                        //проверяем заполненность полей фото, если нет, то маркируется как не для АЗТ
                        string dir_foto_new  = @"\\HOLY-BOX\APTfiles\Photo specimens" + @"\" + combox_project.Text;  //+папка для образца
                        if (images_paths.Count != 0 && images_paths[0] != "")
                        {
                            new_spec.state = 1;
                            //просто получаем директорию файла
                            new_spec.foto_before = images_paths[0];
                            FileInfo fileInf = new FileInfo(new_spec.foto_before);
                            new_spec.foto_before = fileInf.DirectoryName;
                        }
                        else
                        {
                            MessageBox.Show("No foto seleted, specimen marked as NOT ready for APT");
                            new_spec.state = 7;
                        }
                        
                        conn.Open();
                        string sqlcom_3 = "INSERT INTO test2base.specimens (id_producer, id_state, date_prep, id_project, id_treatment, " +
                            "id_treat_type, id_respon, place_foto_bef, place_foto_after, id_material, priority, comments) VALUES (@id_producer,@id_state,@datetime,@id_project,@treatment," +
                        "@id_treat_type,@id_respon,@foto_before,@foro_after,@id_material,@priority, @comment)";
                        using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
                        {
                            comand.Parameters.AddWithValue("@id_producer", new_spec.producer);
                            comand.Parameters.AddWithValue("@id_state", new_spec.state);
                            comand.Parameters.AddWithValue("@datetime", new_spec.datetime);
                            comand.Parameters.AddWithValue("@id_project", new_spec.project);
                            //comand.Parameters.AddWithValue("@id_storage", new_spec.storage);
                            comand.Parameters.AddWithValue("@treatment", new_spec.treatment);
                            comand.Parameters.AddWithValue("@id_treat_type", new_spec.type);
                            comand.Parameters.AddWithValue("@foto_before", new_spec.foto_before);
                            comand.Parameters.AddWithValue("@foro_after", new_spec.foto_after);
                            comand.Parameters.AddWithValue("@id_material", new_spec.material);
                            comand.Parameters.AddWithValue("@id_respon", new_spec.resonse);
                            //comand.Parameters.AddWithValue("@stor_position", new_spec.stor_pos);
                            comand.Parameters.AddWithValue("@priority", new_spec.priority);
                            comand.Parameters.AddWithValue("@comment", rich_txtbox_comments.Text);
                            //MessageBox.Show(comand.CommandText);
                            comand.ExecuteNonQuery();
                            //проверить выполнен ли запрос
                            conn.Close();
                        }
                        //получить новый ID                        
                        conn.Open();
                        //MessageBox.Show("Дата для сравнения"+new_spec.datetime);
                        sqlcom_3 = "SELECT idspecimens FROM test2base.specimens WHERE id_producer=@id_producer AND id_state=@id_state AND date_prep=@datetime AND " +
                            "id_project=@id_project AND id_treatment=@id_treatment AND " +
                            "id_treat_type=@id_treat_type AND id_respon=@id_respon AND id_material=@id_material";
                        using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
                        {
                            comand.Parameters.AddWithValue("@id_producer", new_spec.producer);
                            comand.Parameters.AddWithValue("@id_state", new_spec.state);
                            comand.Parameters.AddWithValue("@datetime", new_spec.datetime);
                            comand.Parameters.AddWithValue("@id_project", new_spec.project);
                            //comand.Parameters.AddWithValue("@id_storage", new_spec.storage);
                            comand.Parameters.AddWithValue("@id_treatment", new_spec.treatment);
                            comand.Parameters.AddWithValue("@id_treat_type", new_spec.type);
                            comand.Parameters.AddWithValue("@id_material", new_spec.material);
                            comand.Parameters.AddWithValue("@id_respon", new_spec.resonse);
                            //MessageBox.Show("1");
                            using (MySqlDataReader reader = comand.ExecuteReader())
                            {
                                //MessageBox.Show("прошли");
                                if (reader.HasRows)
                                {
                                    //MessageBox.Show("2");
                                    while (reader.Read())
                                    {
                                        id_new = Convert.ToInt32(reader[0]);
                                    }
                                    reader.Close();
                                }
                            }
                        }
                        //MessageBox.Show("найденный индекс ="+indexx.ToString());
                        conn.Close();
                        if (id_new != -1)
                        {
                            //записать местоположение в новую базу данных
                            Ch_or_create_stor_pos(int.Parse(new_spec.storage), int.Parse(new_spec.stor_pos), id_new, false);
                            //Simple_SQL_req("INSERT INTO test2base.storage_position (id_storage, position, id_specimen) " +
                            //    "VALUES (" + new_spec.storage + ", " + new_spec.stor_pos + ", " + id_new.ToString() + ");");
                        }

                        new_spec.foto_before = Copy_fotos(new_spec.foto_before, dir_foto_new, 1,id_new);
                        //проверяем и добавляем состояние
                        Push_state(conn, new_spec.treatment, new_spec.material, state_name, new_spec.project);
                        //поменять название папки фото до
                        conn.Open();
                        sqlcom_3 = "UPDATE test2base.specimens SET place_foto_bef =@foto_before WHERE (idspecimens = " + id_new.ToString()+")";
                        using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
                        {
                            comand.Parameters.AddWithValue("@foto_before", new_spec.foto_before);
                            comand.ExecuteNonQuery();
                        }
                        //добавить информацию о целевых установках
                        Ch_list_ad_new(ch_listbox_setups_add_new, id_new);
                        conn.Close();
                        MessageBox.Show("Если вы дошли до этого сообщения,\n" +
                            "то образец скорее всего создан.");
                    }
                    else MessageBox.Show("smth wrong 'specimen_new_accepted'=false");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка при создании образца", MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1);
                }
            }
            if (id_new != -1)
            {
                Log_action(Properties.Settings.Default.default_username, "new specimen", "universe", combox_storage.Text + " " + combox_pos_add.Text, id_new.ToString());
            }
        }

        private void Picbox_big_Click(object sender, EventArgs e)
        {
            
        }

        private void Combox_storage_KeyPress(object sender, KeyPressEventArgs e)
        {
            //шоб не вводили свои хранилища образцов
            combox_storage.Text = "";
        }

        private void Combox_storage_KeyUp(object sender, KeyEventArgs e)
        {
            //шоб не вводили свои хранилища образцов
            combox_storage.Text = "";
        }

        private void Combox_treat_type_KeyUp(object sender, KeyEventArgs e)
        {
            //шоб не придумывали свои типы образцов
            combox_treat_type.Text = "";
        }
        private void Save_settings()
        {
            Properties.Settings.Default.material_add = combox_material.Text;
            Properties.Settings.Default.producer = combox_producer.Text;
            Properties.Settings.Default.project = combox_project.Text;
            Properties.Settings.Default.storage = combox_storage.Text;
            Properties.Settings.Default.treatment = combox_treatment.Text;
            Properties.Settings.Default.type_prep = combox_treat_type.Text;
            Properties.Settings.Default.respons = combox_response.Text;
            Properties.Settings.Default.font_config = this.Font.ToString();
            Properties.Settings.Default.ini_split_add_new = splitContainer_add_new.SplitterDistance;
            Properties.Settings.Default.ini_split_inf = split_inf.SplitterDistance;
            //MessageBox.Show("ssave: \n " + Properties.Settings.Default.font_config);
            if (combox_showonly.Text=="All")
            {
                Properties.Settings.Default.show_only_specimens = 50;
            }
            else Properties.Settings.Default.show_only_specimens = Convert.ToInt32(combox_showonly.Text);
            Properties.Settings.Default.no_pics = chbox_no_pics.Checked;
            Properties.Settings.Default.res_succ = combox_succ.Text;
            Properties.Settings.Default.open_data_file = open_data_file.InitialDirectory;
            try
            {
                Properties.Settings.Default.res_laser_power = Convert.ToDecimal(txtbox_las_pow.Text);
            }
            catch (FormatException)
            {
                Properties.Settings.Default.res_laser_power = 0;
                txtbox_las_pow.Text = "0";
                MessageBox.Show("Вы попытались вписать в поле мощности лазера недопустимый символ. Просьба вписывать только цифры и запятую");
                
            }
            try
            {
                Properties.Settings.Default.res_temper = Convert.ToInt32(txtbox_temperature.Text);
            }
            catch (FormatException)
            {
                Properties.Settings.Default.res_temper = 0;
                txtbox_temperature.Text = "0";
                MessageBox.Show("Вы попытались вписать в поле температуры недопустимый символ. Просьба вписывать только цифры и запятую");

            }
            Properties.Settings.Default.Save();
        }

        private void Form_specimens_FormClosing(object sender, FormClosingEventArgs e)
        {
            Save_settings();
            //сохранить фильтры в json
            Save_def_json(@"\Settings\test_json.json");
            e.Cancel = true;
            /*if (_client.ConnectionState == Discord.ConnectionState.Connected)
            {
                _client.StopAsync();
            }*/
            Hide();
        }

        private void Combox_project_KeyUp(object sender, KeyEventArgs e)
        {
            //шоб тут не это
            combox_project.Text = "";
        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private void Combox_sort_SelectedIndexChanged(object sender, EventArgs e)
        {            
            if (!on_load) Refresh_datagrid();
        }

        private void Btn_up_down_Click(object sender, EventArgs e)
        {
            if (pic_change)
            {
                btn_up_down.Image.Dispose();
                btn_up_down.Image = Properties.Resources.down;
                pic_change = false;
            }
            else
            {
                btn_up_down.Image.Dispose();
                btn_up_down.Image = Properties.Resources.upp;
                pic_change = true;
            }

            Refresh_datagrid();
        }

        private void Combox_showonly_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!on_load)
            {
                if (combox_showonly.Text != "" && combox_showonly.Text != "All")
                {
                    show_only_spec = Convert.ToInt32(combox_showonly.Text);
                }
                else
                {
                    if (combox_showonly.Text == "All")
                    {
                        show_only_spec = 1000;
                    }
                    else MessageBox.Show("SMTH wrong with 'Combox_showonly_SelectedIndexChanged'");
                }
                Refresh_datagrid();
            }
        }

        private void Combox_showonly_KeyPress(object sender, KeyPressEventArgs e)
        {
            //шоб не писали символы
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8 && number != 127)
            {
                e.Handled = true;
            }
        }

        private void Combox_showonly_TextChanged(object sender, EventArgs e)
        {
            if (!on_load)
            {
                if (combox_showonly.Text != "" && combox_showonly.Text != "All")
                {
                    show_only_spec = Convert.ToInt32(combox_showonly.Text);
                }
                else
                {
                    if (combox_showonly.Text == "All")
                    {
                        show_only_spec = 9999;
                    }
                    else MessageBox.Show("SMTH wrong with 'Combox_showonly_TextChanged'");
                }
                Refresh_datagrid();
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("сообщите антону, что вы нажали эту кнопку");
            Fill_information();
        }

        private void DataGrid_specimens_SelectionChanged(object sender, EventArgs e)
        {
            if ((!on_load) && dataGrid_specimens.SelectedRows != null)// && dataGrid_specimens.CurrentRow != null
            {
                if (dataGrid_specimens.SelectedRows.Count > 0 && dataGrid_specimens.SelectedRows[0].Cells != null)
                {
                    on_load = true; //выключаем срабатывание Form_Activated при отладке (реагирует на MessageBox)
                    Properties.Settings.Default.main_spec_id = Convert.ToInt32(dataGrid_specimens.SelectedRows[0].Cells[0].Value);
                    Properties.Settings.Default.Save();
                    combox_move_to.Text = "";
                    combox_move_pos.Text = "";
                    Fill_information();
                    on_load = false;
                }
            }

            //здесь происходит магия и почему-то херово запоминается ИД или процедура происходит не тогда, когда нужно

            //Fill_information();

        }

        private void Picbox_1_DoubleClick(object sender, EventArgs e)
        {
            //открыть проводник
            string str = Directory.GetParent(info_files_paths_bef[0]).ToString();
            if (info_files_paths_bef[0] != null && picbox_inf_bef_1.Image != null)
            {
                Process.Start("explorer.exe", str);
            }
        }

        private void Picbox_1_Click(object sender, EventArgs e)
        {
            //смена картинок

        }

        private void Picbox_2_DoubleClick(object sender, EventArgs e)
        {
            //открыть проводник
            string str = Directory.GetParent(info_files_paths_bef[0]).ToString();
            if (info_files_paths_bef[0] != null && picbox_inf_bef_2.Image!=null)
            {
                Process.Start("explorer.exe", str);
            }
        }

        private void Picbox_3_DoubleClick(object sender, EventArgs e)
        {
            //открыть проводник
            string str = Directory.GetParent(info_files_paths_bef[0]).ToString();
            if (info_files_paths_bef[0] != null && picbox_inf_bef_3.Image != null)
            {
                Process.Start("explorer.exe", str);
            }
        }

        private void Combox_producer_KeyUp(object sender, KeyEventArgs e)
        {
            //шоб не придумывали своих изготовителей
            combox_producer.Text = "";
        }

        private void Combox_response_KeyUp(object sender, KeyEventArgs e)
        {
            //шоб не придумывали своих ответственных
            combox_response.Text = "";
        }
        private bool Check_fields_research()
        {
            if (txtbox_data_dir.Text != "" && txtbox_las_pow.Text != "" && txtbox_temperature.Text != "" && combox_succ.Text != "" && combox_setup.Text!="")
            {
                return true;
            }
            else
            {
                MessageBox.Show("Fill all fields");
                return false;
            }
        }
        private void Button1_Click_1(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.user_access_lvl <= 2)
            {
                if (Check_fields_research())
                {
                    //сохранить настройки                
                    Save_settings();
                    //добавляем инфу об исследовании
                    conn_str = Get_conn_string(Properties.Settings.Default.server, Properties.Settings.Default.port,
                    Properties.Settings.Default.database, Parent_form.cbox_username.Text, Parent_form.txtbox_pass.Text);
                    using (MySqlConnection conn = New_connection(conn_str))
                    {
                        try
                        {
                            //проверяем есть ли уже все записи в таблицах
                            int indexx = Convert.ToInt32(dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value);
                            if (true) //заглушка
                            {
                                //если нет, то добавляем новые
                                //MessageBox.Show(date_time_add_edit.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                                string id_researcher = Check_for_exist(combox_researcher.Text, conn, "producers", "id_producer", "surname");
                                
                                string id_setup_str = Check_for_exist(combox_setup.Text, conn, "setups", "id_setups", "name");
                                conn.Open();
                                string sqlcom_3 = "INSERT INTO test2base.researches (id_specimen, res_date, temperature, power_laser, comments, success, " +
                                    "data_dir, id_researcher, duration, id_setup) VALUES (@id_specimen,@res_date,@temperature,@power_laser,@comments,@success," +
                                "@data_dir,@id_researcher,@duration,@id_setup)";


                                string duration = txtbox_duration.Text;
                                if (duration == "") duration = "1";

                                using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
                                {
                                    comand.Parameters.AddWithValue("@id_specimen", indexx.ToString());
                                    comand.Parameters.AddWithValue("@res_date", dateTimePicker_research.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                                    comand.Parameters.AddWithValue("@temperature", txtbox_temperature.Text);
                                    comand.Parameters.AddWithValue("@power_laser", txtbox_las_pow.Text);
                                    comand.Parameters.AddWithValue("@comments", txtbox_comments.Text);
                                    comand.Parameters.AddWithValue("@success", combox_succ.Text);
                                    comand.Parameters.AddWithValue("@data_dir", txtbox_data_dir.Text);
                                    comand.Parameters.AddWithValue("@id_researcher", id_researcher);
                                    comand.Parameters.AddWithValue("@duration", duration);                                    
                                    comand.Parameters.AddWithValue("@id_setup", id_setup_str);
                                    comand.ExecuteNonQuery();
                                    //проверить выполнен ли запрос
                                    conn.Close();
                                }
                                //изменяем состояние образца
                                // 2 -  по умолчанию сделано АЗТ, не нужен ПЭМ
                                // 3 - сделано АЗТ, нужен ПЭМ
                                DialogResult result = MessageBox.Show("Need TEM control?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);


                                conn.Open();
                                int state;
                                if (result == DialogResult.Yes)
                                {
                                    state = 3;
                                }
                                else state = 2;
                                sqlcom_3 = "UPDATE test2base.specimens SET specimens.id_state=" + state.ToString() + " WHERE specimens.idspecimens =" + indexx.ToString();
                                using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
                                {
                                    comand.ExecuteNonQuery();

                                }
                                conn.Close();
                            }
                            Log_action(Properties.Settings.Default.default_username, "research was made", "", "", indexx.ToString());
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                        }
                    }                    
                }
            }
            else
            {
                MessageBox.Show("У вас не достаточно прав доступа к данной функции, обратитесь к администратору");
            }
            Refresh_datagrid();
        }

        private void Txtbox_data_dir_DoubleClick(object sender, EventArgs e)
        {
            open_data_file.InitialDirectory = Properties.Settings.Default.open_data_file;

            //диалог на поиск директории с данными
            DialogResult result = open_data_file.ShowDialog();
            if (result == DialogResult.OK)
            {
                //MessageBox.Show(open_data_file.FileNames[0]);
                //проверка на тип данных
                txtbox_data_dir.Text = open_data_file.FileNames[0];
            }
            //else MessageBox.Show("SMTH wrong with data file dialog");

        }

        private void Combox_succ_KeyUp(object sender, KeyEventArgs e)
        {
            if (!(combox_succ.Text == "+" || combox_succ.Text == "-" || combox_succ.Text == "+/-"))
            {
                combox_succ.Text = "";
            }
        }

        private void Combox_researcher_KeyUp(object sender, KeyEventArgs e)
        {
            //шоб своих не писали
            combox_researcher.Text = "";
        }
        /// <summary>
        /// получаем максимальное значение позиций в хранилище
        /// </summary>
        /// <returns></returns>
        private int Get_max_positions(MySqlConnection conection, string stor_name)
        {
            int deflt = 0;
            //using (MySqlConnection conn = New_connection(conn_str))
            using (conection)
            {
                try
                {
                    conection.Open();
                    string sqlcom_3 = "SELECT capacity FROM test2base.storage WHERE storage.name ='" + stor_name + "'";
                    //MessageBox.Show("Запрос ="+sqlcom_3);
                    using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conection))
                    {
                        using (MySqlDataReader reader = comand.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {                                    
                                    deflt = Convert.ToInt32(reader[0]);
                                    
                                }
                                reader.Close();
                            }
                        }
                    }
                    //изменяем состояние образца                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1);
                }
                conection.Close();

            }
            return deflt;
        }
        /// <summary>
        /// Получить занятые позиции
        /// </summary>
        /// <returns></returns>
        private List<int> Get_ocupied_pos(MySqlConnection conection, string stor_name)
        {
            List<int> ans = new List<int>();
            using (conection)
            {
                try
                {
                    conection.Open();
                    string sqlcom_3 = "SELECT position, id_specimen FROM test2base.storage_position " +
                        "WHERE (id_storage = (SELECT id_storage FROM test2base.storage WHERE (name = '"+stor_name+"')))";
                    using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conection))
                    {
                        using (MySqlDataReader reader = comand.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    //deflt = Convert.ToInt32(reader[0]);
                                    if (Convert.ToInt32(reader[1])!=0)
                                    ans.Add(Convert.ToInt32(reader[0]));

                                }
                                reader.Close();
                            }
                        }
                    }                   
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка в выборе свободных позиций хранилища", MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1);
                }
                conection.Close();

            }

            return ans;
        }
        private void Fill_num_combo(string storage_name,ComboBox box)
        {
            //подгрузить другой набор позиций
            if (storage_name != "")
            {                
                //MessageBox.Show("Тип = "+ type_fill.ToString());
                conn_str = Get_conn_string(Properties.Settings.Default.server, Properties.Settings.Default.port,
                    Properties.Settings.Default.database, Parent_form.cbox_username.Text, Parent_form.txtbox_pass.Text);
                //получаем макс значение позиций
                int max_pos = Get_max_positions(New_connection(conn_str), storage_name);
                //делаем список позиций
                List<string> position_list = new List<string>(max_pos);                
                for (int i = 1; i <=max_pos;i++)
                {
                    position_list.Add(i.ToString());
                }
                //запрашиваем занятые позиции
                List<int> occupied = Get_ocupied_pos(New_connection(conn_str), storage_name);
                //если позиция занята - удаляем из списка
                foreach (int pos in occupied)
                {
                    if (position_list.Contains(pos.ToString()))
                    {
                        position_list.Remove(pos.ToString());
                    }
                }
                //выводим список позиций
                box.Items.Clear();
                box.Items.AddRange(position_list.ToArray());
            }
        }
        private void Combox_storage_SelectedIndexChanged(object sender, EventArgs e)
        {
            //подгрузить другой набор позиций
            Fill_num_combo(combox_storage.Text, combox_pos_add);
            combox_pos_add.Text = "";

        }
        private void Ch_btn_state(bool make, bool new_stor, bool add_tem, bool add_foto_after, bool add_foto_before)
        {
            btn_make_research.Enabled = make;
            btn_move_new_stor.Enabled = new_stor;
            btn_need_TEM.Enabled = add_tem;
            btn_foto_after_add.Enabled = add_foto_after;
            Btn_add_TEM_before.Enabled = add_foto_before;
        }
        private void Deal_with_buttons()
        {
            //просто процедура определения доступности/не доступности кнопок
            //проверяем что можно делать с этим образцом
            string indexx = "";
            if (dataGrid_specimens.CurrentRow != null)
            {
                if (dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[7].Value != null)
                {
                    indexx = dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[7].Value.ToString();
                }
            }
            if (dataGrid_specimens.CurrentRow != null)
            {
                Ch_btn_state(false, false, false, false, false);
            }
            switch (indexx)
            {
                //make res - new stor - nedd TEM - TEM after - TEM before
                case "Ready for APT":
                    Ch_btn_state(true, true, false, false, false);
                    break;
                case "APT done":
                    Ch_btn_state(false, true, true, false, false);
                    break;
                case "APT done, need TEM":
                    Ch_btn_state(false, true, false, true, false);
                    break;
                case "APT stopped":
                    Ch_btn_state(false, true, true, false, false);
                    break;
                case "APT in progress":
                    Ch_btn_state(false, false, false, false, false);
                    break;
                case "APT vanished":
                    Ch_btn_state(false, false, false, false, false);
                    break;
                case "Storage":
                    Ch_btn_state(false, true, false, false, true);
                    break;
            }
        }
        private void DataGrid_specimens_CurrentCellChanged(object sender, EventArgs e)
        {
            //MessageBox.Show("Произошло смена CurrentCellChanged");
            Deal_with_buttons();
        }

        private void Btn_need_TEM_Click(object sender, EventArgs e)
        {
            //пометить ,что нужен ПЭМ
            conn_str = Get_conn_string(Properties.Settings.Default.server, Properties.Settings.Default.port,
                Properties.Settings.Default.database, Parent_form.cbox_username.Text, Parent_form.txtbox_pass.Text);
            int indexx = Convert.ToInt32(dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value);
            using (MySqlConnection conn = New_connection(conn_str))
            {
                try
                {
                    if (dataGrid_specimens.CurrentRow != null)
                    {
                        conn.Open();                        
                        string sqlcom_3 = "UPDATE test2base.specimens SET specimens.id_state=3 WHERE specimens.idspecimens =" + indexx.ToString();
                        using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
                        {
                            comand.ExecuteNonQuery();
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
            {
                Log_action(Properties.Settings.Default.default_username, "mark need TEM", "", "", indexx.ToString());
            }
            Refresh_datagrid();
        }


        private void Combox_move_to_KeyUp(object sender, KeyEventArgs e)
        {
            //шоб своё не ридумывали
            combox_move_to.Text = "";
        }

        private void Combox_move_to_SelectedIndexChanged(object sender, EventArgs e)
        {
            //чтобы не было небезопасного перемещения
            combox_move_pos.Text = "";
            combox_move_pos.SelectedIndex = -1;
            Fill_num_combo(combox_move_to.Text, combox_move_pos);
        }
        private int Get_stor_index(string from)
        {
            int ans = 0;
            conn_str = Get_conn_string(Properties.Settings.Default.server, Properties.Settings.Default.port,
                Properties.Settings.Default.database, Parent_form.cbox_username.Text, Parent_form.txtbox_pass.Text);
            using (MySqlConnection conn = New_connection(conn_str))
            {
                try
                {
                    if (from != "" && dataGrid_specimens.CurrentRow != null)
                    {
                        conn.Open();
                        int indexx = Convert.ToInt32(dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value);
                        string sqlcom_3 = "SELECT id_storage FROM test2base.storage WHERE name='" + from + "'";
                        using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
                        {
                            using (MySqlDataReader reader = comand.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        ans = Convert.ToInt32(reader[0]);
                                    }
                                    reader.Close();
                                }
                            }
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
            return ans;
        }
        /// <summary>
        /// есть ли запись с по этому хранилищу и с этой позицией, возращается ИД записи или 0
        /// </summary>
        /// <returns></returns>
        private int Get_stor_pos(int ID_stor, int stor_pos)
        {
            int ans = 0;
            conn_str = Get_conn_string(Properties.Settings.Default.server, Properties.Settings.Default.port,
                    Properties.Settings.Default.database, Parent_form.cbox_username.Text, Parent_form.txtbox_pass.Text);
            using (MySqlConnection conn = New_connection(conn_str))
            {
                try
                {
                    conn.Open();
                    //indexx = Convert.ToInt32(dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value);
                    string sqlcom_3 = "SELECT id_storage_position FROM test2base.storage_position " +
                        "WHERE (id_storage = "+ ID_stor.ToString()+ ") AND (position = "+stor_pos.ToString() +")";
                    using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
                    {
                        using (MySqlDataReader reader = comand.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    ans = Convert.ToInt32(reader[0]);
                                }
                                reader.Close();
                            }
                        }
                    }
                    conn.Close();                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка в модуле определения есть ли запись с образцом в storage_position", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                }
            }

            return ans;
        }
        /// <summary>
        /// изменяет или создает запись с указанным ид образца, 
        /// </summary>
        private void Ch_or_create_stor_pos(int stor, int pos, int ID_spec, bool is_delete)
        {
            //проверяем есть ли запись c таким ИД_хранилища, если есть - получаем
            //int ID_stor_pos_new = Get_stor_pos(stor, pos);
            string sql_m = "";
            int new_val = 0;
            if (!is_delete) new_val = ID_spec;
            //если 0 - значит создаем запись
            //если нет - апдейтим
            if (int.TryParse(SQL_List_querry("SELECT id_storage_position FROM test2base.storage_position " +
                        "WHERE (id_storage = '" + stor.ToString() + "') AND (position = '" + pos.ToString() + "')")[0], out int ID_stor_pos) && ID_stor_pos!=0)
            {
                sql_m = "UPDATE test2base.storage_position SET id_specimen = " + new_val.ToString() + " WHERE (id_storage_position = " + ID_stor_pos.ToString() + ")";                
            } 
            else
            {
                sql_m = "INSERT INTO test2base.storage_position (id_storage, position, id_specimen) " +
                        "VALUES('" + stor.ToString() + "', '" + pos.ToString() + "', '" + new_val.ToString() + "')";
            }
            //выполняем запрос
            Simple_SQL_req(sql_m);            
        }
        private void Btn_move_new_stor_Click(object sender, EventArgs e)
        {
            //изменить проверку
            if (Properties.Settings.Default.user_access_lvl <= 3
                && combox_move_pos.Text!=""
                && combox_move_to.Text!=""
                && txtbox_move_from.Text!=""
                && txtbox_move_from.Text.Length>3)
            {
                //получаем ид образца, позицию
                int stor_new_id = Get_stor_index(combox_move_to.Text);                
                int indexx = Convert.ToInt32(dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value);
                int pos_new = Convert.ToInt32(combox_move_pos.Text);
                //MessageBox.Show(txtbox_move_from.Text.Substring(0, txtbox_move_from.Text.Length - txtbox_move_from.Text.IndexOf(' ')+1));
                //MessageBox.Show("-"+txtbox_move_from.Text.Substring(txtbox_move_from.Text.LastIndexOf(' ')+1)+"-");
                int pos_old = Convert.ToInt32(txtbox_move_from.Text.Substring(txtbox_move_from.Text.LastIndexOf(' ') + 1));
                //string pos_o = txtbox_move_from.Text.Substring(txtbox_move_from.Text.LastIndexOf(' ') + 1);
                int stor_old_id = Get_stor_index(txtbox_move_from.Text.Substring(0, txtbox_move_from.Text.LastIndexOf(' ')));
                                
                //удаляем запись о старом месте
                Ch_or_create_stor_pos(stor_old_id, pos_old, indexx, true);
                //делаем новую запись о новом положении
                //если не нужно удалять образец
                if (combox_move_to.Text != "Deleted")
                {
                    Ch_or_create_stor_pos(stor_new_id, pos_new, indexx, false);
                }
                else MessageBox.Show("Образец к вам больше не вернется!");

                if (indexx != -1)
                {
                    Log_action(Properties.Settings.Default.default_username, "change position", txtbox_move_from.Text, combox_move_to.Text + " " + combox_move_pos.Text, indexx.ToString());
                }
                Refresh_datagrid();
                combox_move_pos.Text = "";
                combox_move_to.Text = "";
            }
            else
            {
                MessageBox.Show("У вас не достаточно прав доступа для перемещения образца, обратитесь к администратору\n" +
                    "или вы пытаетесь переместить удаленный уже образец");
                combox_move_pos.Text = "";
                combox_move_to.Text = "";
            }
        }

        private void Combox_move_pos_SelectedIndexChanged(object sender, EventArgs e)
        {
            //проверка на совпадение мест перемещения
            if (txtbox_move_from.Text == combox_move_to.Text + " " + combox_move_pos.Text)
            {
                combox_move_pos.Text = "";
                combox_move_pos.SelectedIndex = -1;
                MessageBox.Show("Wrong position!\nPosotion duplicate.");
            }
        }
        private int Get_id_research(string where, string index)
        {
            int ans = -1;
            //int index = -1;
            string col_name="";
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
            //MessageBox.Show("Ищем ид в "+where+" ид для поиска "+index);
            if (index!="-1")
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
                                        MessageBox.Show("Найденный ид = "+ans);
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
        private void DataGrid_specimens_DoubleClick(object sender, EventArgs e)
        {
            //если есть исследование этого образца - переход в окно исследований и пока этого образца
            if (dataGrid_specimens.CurrentRow.Cells[7].Value != null)
            {
                
                string index = dataGrid_specimens.CurrentRow.Cells[0].Value.ToString();
                Properties.Settings.Default.main_spec_id = Convert.ToInt32(index);
                //MessageBox.Show("показываем форму исследовнаий, выбранный индекс = "+index);
                Properties.Settings.Default.Save();
                int id_res = Get_id_research("researches", index);
                Parent_form.Show_from(id_res, Convert.ToInt32(index),1);
            }
        }
        private void Select_index()
        {
            int sel_index = Properties.Settings.Default.main_spec_id;
            int index = -1;
            for (int i = 0; i < dataGrid_specimens.Rows.Count; i++)
            {
                if (sel_index == Convert.ToInt32(dataGrid_specimens.Rows[i].Cells[0].Value))
                {
                    index = i;
                }
            }
            if (index != -1)
            {
                dataGrid_specimens.Rows[index].Selected = true;
                dataGrid_specimens.CurrentCell = dataGrid_specimens.Rows[index].Cells[0];
            }
        }

        private void Form_specimens_FormClosed(object sender, FormClosedEventArgs e)
        {
            //this.Dispose();
            //GC.Collect();
        }
        private void Read_from_exel(int count)
        {
            Excel.Application ObjWorkExcel = new Excel.Application(); //открыть эксель
            Excel.Workbook ObjWorkBook = ObjWorkExcel.Workbooks.Open(@"D:\Эксперименты new.xlsx",
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing); //открыть файл
            Excel.Worksheet ObjWorkSheet = (Excel.Worksheet)ObjWorkBook.Sheets[1]; //получить 1 лист
            var lastCell = ObjWorkSheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell);//1 ячейку

            int lastColumn = (int)lastCell.Column;//!сохраним непосредственно требующееся в дальнейшем
            int lastRow = (int)lastCell.Row;
            //потом убрать
            if (lastRow > count) lastRow = count;
            //
            string[,] list = new string[lastCell.Column, lastCell.Row]; // массив значений с листа равен по размеру листу
            for (int i = 0; i < lastColumn; i++) //по всем колонкам
                for (int j = 0; j < lastRow; j++) // по всем строкам
                    list[i, j] = ObjWorkSheet.Cells[j + 1, i + 1].Text.ToString();//считываем текст в строку
            ObjWorkBook.Close(false, Type.Missing, Type.Missing); //закрыть не сохраняя
            ObjWorkExcel.Quit(); // выйти из экселя
            GC.Collect(); // убрать за собой -- в том числе не используемые явно объекты !
            for (int j = 1; j < lastRow; j++) // по всем строкам
            {
                MessageBox.Show("Материал " + j.ToString() + " = " + list[3, j]);
                //запускаем бодягу на экспорт
                New_specimen_data new_spec = new New_specimen_data
                {
                    //datetime = date_time_add_edit.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                    datetime = list[0, j],
                    material = list[3, j],
                    producer = list[6, j], //нужен проверочный диалог
                    project = list[12, j], //нужен проверочный диалог
                    resonse = list[6, j], //умолчание приготовитель
                    type = list[4, j],
                    //состояние
                    state = 2, //2 - "APT done"
                    storage = "5", //5 - Deleted
                    treatment = list[5, j],
                    foto_after = "", //нужен диалог
                    foto_before = "", //нужен диалог
                    stor_pos = "1" //по умлочанию в Deleted позиция 1
                };
                if (new_spec.treatment == "") new_spec.treatment = "no"; //нет обработки
                if (new_spec.project == "") new_spec.project = "ВНИР общий"; //нет проекта
                //диалог на фото до                
                string spec_id = list[1,j];
                if (spec_id == "") spec_id = "-1";
                open_dial_im_before.Title = "Загрузите фото до";
                DialogResult result = open_dial_im_before.ShowDialog();
                if (result == DialogResult.OK)
                {

                    string path = open_dial_im_before.FileName;
                    //MessageBox.Show("Диретокрия + файл" + path);
                    FileInfo fileInf = new FileInfo(path);
                    path = fileInf.DirectoryName;
                    //MessageBox.Show("Диретокрия" + path);
                    string dir_foto_new = @"\\HOLY-BOX\APTfiles\Photo specimens" + @"\" + new_spec.project;  //+папка для образца
                    new_spec.foto_before = Copy_fotos(path, dir_foto_new, 1,Convert.ToInt16(spec_id));
                }
                else
                {
                    new_spec.foto_before = "";
                }

                //диалог на фото после
                open_dial_im_before.Title = "Загрузите фото после";
                result = open_dial_im_before.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string path = open_dial_im_before.FileName;
                    //MessageBox.Show("Диретокрия + файл" + path);
                    FileInfo fileInf = new FileInfo(path);
                    path = fileInf.DirectoryName;
                    //MessageBox.Show("Диретокрия" + path);
                    string dir_foto_new = @"\\HOLY-BOX\APTfiles\Photo specimens" + @"\" + new_spec.project;  //+папка для образца
                    new_spec.foto_after = Copy_fotos(path, dir_foto_new, 2, Convert.ToInt16(spec_id));
                }
                else
                {
                    new_spec.foto_after = "";
                }

                New_research new_res = new New_research
                {
                    comments = list[10, j],
                    date_res = list[0, j],
                    duration = list[11, j],
                    laser_power = list[9, j],
                    temp = list[8, j],
                    succ = list[7, j]
                };
                //MessageBox.Show("дата из таблички= "+new_spec.datetime);

                DateTime.TryParse(new_spec.datetime, out DateTime temp_dat);
                //MessageBox.Show("Теперь дата= "+temp_dat.ToString("yyyy-MM-dd HH:mm:ss"));
                new_spec.datetime = temp_dat.ToString("yyyy-MM-dd HH:mm:ss");
                using (MySqlConnection connect = New_connection(conn_str))
                {
                    try
                    {
                        //проверяем есть ли такие уже
                        new_spec.producer = Check_for_exist(new_spec.producer, connect, "producers", "id_producer", "surname");
                        //материал
                        new_spec.material = Check_for_exist(new_spec.material, connect, "materials", "id_material", "name");
                        //тип
                        new_spec.type = Check_for_exist(new_spec.type, connect, "type", "id_type", "name");
                        //обработка облучение/отжиг и т.д.                        
                        new_spec.treatment = Check_for_exist(new_spec.treatment, connect, "treatment", "id_treatment", "name");
                        //ответственный
                        new_spec.resonse = Check_for_exist(new_spec.resonse, connect, "producers", "id_producer", "surname");
                        //проект
                        new_spec.project = Check_for_exist(new_spec.project, connect, "projects", "id_project", "name");
                        // если поля заполнены, то пишем в базу
                        if (new_spec.project == "") new_spec.project = "21"; //нет проекта
                        if (new_spec.material != "" && new_spec.producer != "")
                        {
                            MessageBox.Show("Можно писать - пишем");

                            connect.Open();
                            string sqlcom_3 = "INSERT INTO test2base.specimens (id_producer, id_state, date_prep, id_project, id_storage, id_treatment, " +
                                "id_treat_type, id_respon, place_foto_bef, place_foto_after, id_material) VALUES (@id_producer,@id_state,@datetime,@id_project,@id_storage,@treatment," +
                            "@id_treat_type,@id_respon,@foto_before,@foro_after,@id_material)";
                            using (MySqlCommand comand = new MySqlCommand(sqlcom_3, connect))
                            {
                                comand.Parameters.AddWithValue("@id_producer", new_spec.producer);
                                comand.Parameters.AddWithValue("@id_state", new_spec.state);
                                comand.Parameters.AddWithValue("@datetime", new_spec.datetime);
                                comand.Parameters.AddWithValue("@id_project", new_spec.project);
                                comand.Parameters.AddWithValue("@id_storage", new_spec.storage);
                                comand.Parameters.AddWithValue("@treatment", new_spec.treatment);
                                comand.Parameters.AddWithValue("@id_treat_type", new_spec.type);
                                comand.Parameters.AddWithValue("@foto_before", new_spec.foto_before);
                                comand.Parameters.AddWithValue("@foro_after", new_spec.foto_after);
                                comand.Parameters.AddWithValue("@id_material", new_spec.material);
                                comand.Parameters.AddWithValue("@id_respon", new_spec.resonse);
                                comand.ExecuteNonQuery();
                                //проверить выполнен ли запрос
                                connect.Close();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Что-то пошло не так и мы это исследование пропустим");
                        }


                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                    }
                }
                //сразу заносим исследовани
                using (MySqlConnection conn = New_connection(conn_str))
                {
                    try
                    {
                        //проверяем есть ли уже все записи в таблицах
                        if (true) //заглушка
                        {
                            //MessageBox.Show("Перед добавлением записи");
                            //если нет, то добавляем новые
                            //MessageBox.Show(date_time_add_edit.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                            //по умолчанию Лукьянчук
                            //string id_researcher = Check_for_exist("Лукьянчук", conn, "producers", "id_producer", "surname");
                            string id_researcher = "6";
                            //string duration = list[11, j];
                            //MessageBox.Show("нашли ид исследователя");
                            if (new_res.duration == "") new_res.duration = "1";
                            //int indexx = Convert.ToInt32(dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value);
                            int indexx = -1;
                            conn.Open();
                            //MessageBox.Show("Дата для сравнения"+new_spec.datetime);
                            string sqlcom_3 = "SELECT idspecimens FROM test2base.specimens WHERE id_producer=@id_producer AND id_state=@id_state AND date_prep=@datetime AND " +
                                "id_project=@id_project AND id_storage=@id_storage AND id_treatment=@id_treatment AND " +
                                "id_treat_type=@id_treat_type AND id_respon=@id_respon AND id_material=@id_material";
                            using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
                            {
                                comand.Parameters.AddWithValue("@id_producer", new_spec.producer);
                                comand.Parameters.AddWithValue("@id_state", new_spec.state);
                                comand.Parameters.AddWithValue("@datetime", new_spec.datetime);
                                comand.Parameters.AddWithValue("@id_project", new_spec.project);
                                comand.Parameters.AddWithValue("@id_storage", new_spec.storage);
                                comand.Parameters.AddWithValue("@id_treatment", new_spec.treatment);
                                comand.Parameters.AddWithValue("@id_treat_type", new_spec.type);
                                comand.Parameters.AddWithValue("@id_material", new_spec.material);
                                comand.Parameters.AddWithValue("@id_respon", new_spec.resonse);
                                //MessageBox.Show("1");
                                using (MySqlDataReader reader = comand.ExecuteReader())
                                {
                                    //MessageBox.Show("прошли");
                                    if (reader.HasRows)
                                    {
                                        //MessageBox.Show("2");
                                        while (reader.Read())
                                        {
                                            indexx = Convert.ToInt32(reader[0]);
                                        }
                                        reader.Close();
                                    }
                                }
                            }
                            //MessageBox.Show("найденный индекс ="+indexx.ToString());
                            conn.Close();
                            //формат дата-время 

                            string data_dir = "no";
                            open_dial_im_before.Title = "Укажите хотя бы 1 файл данных";
                            result = open_dial_im_before.ShowDialog();
                            //images_paths.Clear();
                            if (result == DialogResult.OK)
                            {
                                string path = open_dial_im_before.FileName;
                                //MessageBox.Show("Диретокрия + файл" + path);
                                FileInfo fileInf = new FileInfo(path);
                                path = fileInf.DirectoryName;
                                //MessageBox.Show("Диретокрия" + path);
                                data_dir = path;
                            }

                            conn.Open();
                            sqlcom_3 = "INSERT INTO test2base.researches (id_specimen, res_date, temperature, power_laser, comments, success, " +
                                "data_dir, id_researcher, duration) VALUES (@id_specimen,@res_date,@temperature,@power_laser,@comments,@success," +
                            "@data_dir,@id_researcher,@duration)";
                            using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
                            {
                                comand.Parameters.AddWithValue("@id_specimen", indexx.ToString());
                                comand.Parameters.AddWithValue("@res_date", new_spec.datetime);
                                comand.Parameters.AddWithValue("@temperature", new_res.temp);
                                comand.Parameters.AddWithValue("@power_laser", new_res.laser_power);
                                comand.Parameters.AddWithValue("@comments", new_res.comments);
                                comand.Parameters.AddWithValue("@success", new_res.succ);
                                comand.Parameters.AddWithValue("@data_dir", data_dir);
                                comand.Parameters.AddWithValue("@id_researcher", id_researcher);
                                comand.Parameters.AddWithValue("@duration", new_res.duration);
                                comand.ExecuteNonQuery();
                                //проверить выполнен ли запрос
                                conn.Close();
                            }                          
                        }                        
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                    }
                }


                Refresh_datagrid();
                //Do_filters(data_filters);
            }
        }

        private void Btn_csv_load_Click(object sender, EventArgs e)
        {
            //импорт из экселя
            if (Properties.Settings.Default.user_access_lvl <= 1)
            {
                MessageBox.Show("Начинаем читать из диалога");
                Read_from_exel(10);
                MessageBox.Show("закончили читать из диалога");
            }
            else
            {
                MessageBox.Show("У вас не достаточно прав доступа к данной функции, обратитесь к администратору");
            }
        }
        private void Do_filters(List<string> filters)
        {
            if (filters.Count > 0)
            {
                for (int i = 0; i < dataGrid_specimens.Rows.Count; i++)
                {
                    bool visi = true;
                    for (int j = 0; j < dataGrid_specimens.Rows[i].Cells.Count; j++)
                    {
                        foreach (string str in filters)
                        {
                            if (dataGrid_specimens.Rows[i].Cells[j].Value != null)
                            {
                                if (dataGrid_specimens.Rows[i].Cells[j].Value.ToString() == str)
                                {
                                    //MessageBox.Show("невидимо, так как "+str+" равно "+ datagrid_researches.Rows[i].Cells[j].Value.ToString());
                                    visi = false;
                                }
                            }
                        }

                    }
                    // MessageBox.Show("видимость = "+visi.ToString());
                    dataGrid_specimens.Rows[i].Visible = visi;
                }
            }
            else
            {
                for (int i = 0; i < dataGrid_specimens.Rows.Count; i++)
                {
                    dataGrid_specimens.Rows[i].Visible = true;
                }
            }
        }
        private void Checked(ItemCheckEventArgs e, CheckedListBox obje, bool dorefresh)
        {
            if (!on_load)
            {
                string str_f = "";
                switch (obje.Name)
                {
                    case "ch_listbox_type":
                        str_f = "type.name <> '";
                        break;
                    case "ch_listbox_project":
                        str_f = "projects.name <> '";
                        break;
                    case "ch_listbox_state_f":
                        str_f = "state.name <> '";
                        break;
                    case "ch_listbox_material":
                        str_f = "materials.name <> '";
                        break;
                    case "ch_listbox_storage_f":
                        str_f = "storage.name <> '";
                        break;
                }
                if (e.NewValue == CheckState.Checked)
                {
                    //удаляем из фильтров
                    //data_filters.Remove(obje.Items[e.Index].ToString());
                    filt_master.common_filters.Remove(str_f + obje.Items[e.Index].ToString() + "'");
                }
                else
                {
                    //добавляем к фильтрам
                    //data_filters.Add(obje.Items[e.Index].ToString());
                    filt_master.common_filters.Add(str_f +obje.Items[e.Index].ToString() + "'");
                }
                //Do_filters(data_filters);
                if (dorefresh) Refresh_datagrid();
            }
        }

        private void Ch_listbox_state_f_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            Checked(e,ch_listbox_state_f, isrefreshing);
        }

        private void Ch_listbox_type_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            Checked(e,ch_listbox_type, isrefreshing);
        }

        private void Ch_listbox_material_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            Checked(e, ch_listbox_material, isrefreshing);
        }

        private void Ch_listbox_project_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            Checked(e, ch_listbox_project, isrefreshing);
        }

        private void Btn_cl_type_Click(object sender, EventArgs e)
        {
            isrefreshing = false;
            for (int i = 0; i < ch_listbox_type.Items.Count; i++)
            {
                ch_listbox_type.SetItemChecked(i, false);
            }
            isrefreshing = true;
            Refresh_datagrid();
        }

        private void Btn_cl_proj_Click(object sender, EventArgs e)
        {
            isrefreshing = false;
            for (int i = 0; i < ch_listbox_project.Items.Count; i++)
            {
                ch_listbox_project.SetItemChecked(i, false);
            }
            isrefreshing = true;
            Refresh_datagrid();
        }

        private void Btn_cl_state_Click(object sender, EventArgs e)
        {
            isrefreshing = false;
            for (int i = 0; i < ch_listbox_state_f.Items.Count; i++)
            {
                ch_listbox_state_f.SetItemChecked(i, false);
            }
            isrefreshing = true;
            Refresh_datagrid();
        }

        private void Btn_cl_mat_Click(object sender, EventArgs e)
        {
            isrefreshing = false;
            for (int i = 0; i < ch_listbox_material.Items.Count; i++)
            {
                ch_listbox_material.SetItemChecked(i, false);
            }
            isrefreshing = true;
            Refresh_datagrid();
        }

        private void Btn_sel_type_Click(object sender, EventArgs e)
        {
            isrefreshing = false;
            for (int i = 0; i < ch_listbox_type.Items.Count; i++)
            {
                ch_listbox_type.SetItemChecked(i, true);
            }
            isrefreshing = true;
            Refresh_datagrid();
        }

        private void Btn_sel_proj_Click(object sender, EventArgs e)
        {
            isrefreshing = false;
            for (int i = 0; i < ch_listbox_project.Items.Count; i++)
            {
                ch_listbox_project.SetItemChecked(i, true);
            }
            isrefreshing = true;
            Refresh_datagrid();
        }

        private void Btn_sel_state_Click(object sender, EventArgs e)
        {
            isrefreshing = false;
            for (int i = 0; i < ch_listbox_state_f.Items.Count; i++)
            {
                ch_listbox_state_f.SetItemChecked(i, true);
            }
            isrefreshing = true;
            Refresh_datagrid();
        }

        private void Btn_sel_mat_Click(object sender, EventArgs e)
        {
            isrefreshing = false;
            for (int i = 0; i < ch_listbox_material.Items.Count; i++)
            {
                ch_listbox_material.SetItemChecked(i, true);
            }
            isrefreshing = true;
            Refresh_datagrid();
        }


        private void Picbox_inf_aft_1_DoubleClick(object sender, EventArgs e)
        {
            //открыть проводник
            string str = Directory.GetParent(info_files_paths_aft[0]).ToString();
            if (info_files_paths_aft[0] != null && picbox_inf_aft_1.Image != null)
            {                
                Process.Start("explorer.exe", str);
            }
        }

        private void Picbox_inf_aft_2_DoubleClick(object sender, EventArgs e)
        {
            //открыть проводник
            string str = Directory.GetParent(info_files_paths_aft[0]).ToString();
            if (info_files_paths_aft[0] != null && picbox_inf_aft_2.Image != null)
            {
                Process.Start("explorer.exe", str);
            }
        }

        private void Picbox_inf_aft_3_DoubleClick(object sender, EventArgs e)
        {
            //открыть проводник
            string str = Directory.GetParent(info_files_paths_aft[0]).ToString();
            if (info_files_paths_aft[0] != null && picbox_inf_aft_3.Image != null)
            {
                Process.Start("explorer.exe", str);
            }
        }

        private void Btn_foto_after_add_Click(object sender, EventArgs e)
        {
            //добавить путь к файлам фото после
            int indexx = Convert.ToInt32(dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value);
            using (MySqlConnection conn = New_connection(conn_str))
            {
                try
                {
                    if (dataGrid_specimens.CurrentRow != null)
                    {
                        conn.Open();
                        //int indexx = Convert.ToInt32(dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value);
                        string path_TEM = "";
                        open_dial_im_before.Title = "Load TEM foto after research";
                        DialogResult result = open_dial_im_before.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            string path = open_dial_im_before.FileName;
                            //MessageBox.Show("Диретокрия + файл" + path);
                            FileInfo fileInf = new FileInfo(path);
                            path = fileInf.DirectoryName;
                            //MessageBox.Show("Диретокрия" + path);
                            string dir_foto_new = @"\\HOLY-BOX\APTfiles\Photo specimens" + @"\" + dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[3].Value.ToString();
                            path_TEM = Copy_fotos(path, dir_foto_new, 2,indexx);
                        }
                        if (path_TEM != "")
                        {
                            string sqlcom_3 = "UPDATE test2base.specimens SET specimens.id_state = 2, specimens.place_foto_after=@foto_after WHERE (idspecimens = " + indexx.ToString() + ")";
                            using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
                            {
                                comand.Parameters.AddWithValue("@foto_after", path_TEM);
                                comand.ExecuteNonQuery();
                            }
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
            if (indexx != -1)
            {
                Log_action(Properties.Settings.Default.default_username, "add foto after", "", "", indexx.ToString());
            }
            Refresh_datagrid();
        }

        private void Btn_add_TEM_before_Click(object sender, EventArgs e)
        {
            //берем ид из grid
            int indexx = Convert.ToInt32(dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value);            
            //добавляем поля и маркируем как готовый к исследованию
            if (dataGrid_specimens.CurrentRow != null)
            {
                if (indexx != 0)
                {
                    using (MySqlConnection conn = New_connection(conn_str))
                    {                                                
                        open_dial_im_before.Title = "Load TEM foto before research";
                        DialogResult result = open_dial_im_before.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            string path = open_dial_im_before.FileName;
                            //MessageBox.Show("Диретокрия + файл" + path);
                            FileInfo fileInf = new FileInfo(path);
                            path = fileInf.DirectoryName;
                            //MessageBox.Show("Диретокрия" + path);
                            string dir_foto_new = @"\\HOLY-BOX\APTfiles\Photo specimens" + @"\"+ dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[3].Value.ToString();
                            string path_TEM_b = Copy_fotos(path, dir_foto_new, 1,indexx);
                            conn.Open();
                            string sqlcom_3 = "UPDATE test2base.specimens SET id_state = 1, place_foto_bef =@foto_before WHERE (idspecimens = " + indexx.ToString() + ")";
                            using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
                            {
                                comand.Parameters.AddWithValue("@foto_before", path_TEM_b);
                                comand.ExecuteNonQuery();
                            }
                            conn.Close();
                        }
                    }
                    Refresh_datagrid();
                }
            }
        }

        private void Form_specimens_Activated(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.main_spec_id != -1 && !on_load)
            {
                //MessageBox.Show("нужно показать образец");
                if (Get_index_datagrid(Properties.Settings.Default.main_spec_id.ToString(), 0) != -1)
                {
                    int sel_ind = Get_index_datagrid(Properties.Settings.Default.main_spec_id.ToString(), 0);
                    dataGrid_specimens.Rows[sel_ind].Selected = true;
                    //dataGrid_specimens.CurrentCell = dataGrid_specimens.Rows[sel_ind].Cells[0];
                    dataGrid_specimens.CurrentCell = dataGrid_specimens.Rows[sel_ind].Cells[0];
                }
                //Properties.Settings.Default.main_spec_id = -1;
                //Properties.Settings.Default.Save();
            }
        }
        private void Log_action(string user, string action_type, string val_old, string val_new, string id_spec)
        {
            //логируем действия
            //change position
            //change state
            //дата и время берутся системные
            using (MySqlConnection conn = New_connection(conn_str))
            {
                try
                {
                    conn.Open();
                    string sqlcom_4 = "INSERT INTO test2base.history (id_spec, action, old, new, date, user) VALUES (@id_spec, @action, @old, @new, @date, @user)";
                    using (MySqlCommand comand = new MySqlCommand(sqlcom_4, conn))
                    {
                        comand.Parameters.AddWithValue("@id_spec", id_spec);
                        comand.Parameters.AddWithValue("@action", action_type);
                        comand.Parameters.AddWithValue("@old", val_old);
                        comand.Parameters.AddWithValue("@new", val_new);
                        //DateTime.TryParse(new_spec.datetime, out DateTime temp_dat);
                        DateTime temp_dat = DateTime.Now;
                        //MessageBox.Show("дата время"+temp_dat.ToString());
                        comand.Parameters.AddWithValue("@date", temp_dat.ToString("yyyy-MM-dd HH:mm:ss"));
                        //MessageBox.Show("дата время  после конвертации"+temp_dat.ToString("yyyy-MM-dd HH:mm:ss"));
                        comand.Parameters.AddWithValue("@user", user);
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
            //пытаемся слать уведомление research was made
            if (action_type == "research was made" || action_type == "new specimen" || action_type == "change position")
            Send_discord(false, user, action_type, id_spec);            
        }
        private List<string> Get_related(string action_type, string id_spec)
        {
            List<string> ans = new List<string>();
            //получаем изготовителя
            string prod = SQL_List_querry("SELECT discord_id FROM test2base.producers " +
                        "WHERE (id_producer = (SELECT id_producer FROM test2base.specimens WHERE (idspecimens='" + id_spec + "')));")[0];
            if (prod == "")
            {
                prod = SQL_List_querry("SELECT user_name FROM test2base.producers " +
                        "WHERE (id_producer = (SELECT id_producer FROM test2base.specimens WHERE (idspecimens='" + id_spec + "')));")[0];
            }
            ans.Add("pr " +prod+",");
            switch (action_type)
            {
                case "research was made":
                case "new specimen":                                        
                    //получаем отв за проект
                    string resp = SQL_List_querry("SELECT discord_id FROM test2base.producers " +
                        "WHERE (id_producer = (SELECT id_respon FROM test2base.specimens WHERE (idspecimens='" + id_spec + "')));")[0];
                    if (resp == "")
                    {
                        resp = SQL_List_querry("SELECT user_name FROM test2base.producers " +
                        "WHERE (id_producer = (SELECT id_respon FROM test2base.specimens WHERE (idspecimens='" + id_spec + "')));")[0];
                    }
                    ans.Add("rsp " + resp);
                    return ans;
                case "change position":
                    return ans;
                default:
                    return new List<string>();
            }
        }
        private async void Send_discord(bool voice, string actor, string action_t, string id_spec)
        {
            List<string> actors = new List<string>();
            //пробуем получить кто
            string actor_id = SQL_List_querry("SELECT producers.discord_id FROM test2base.producers WHERE (user_name = '"+actor+"')")[0];
            if (actor_id == "")
            {
                actors.Add("author " + actor + ",");
            }
            else
            {
                actors.Add("author " + actor_id + ",");
            }
            //получаем зависимые
            actors.AddRange(Get_related(action_t, id_spec));
            string hook;
            if (action_t == "research was made")
            {
                hook = SQL_List_querry("SELECT token FROM test2base.producers WHERE (user_name = 'res')")[0];
            }
            else
            {
                hook = SQL_List_querry("SELECT token FROM test2base.producers WHERE (user_name = 'move')")[0];
            }
            //название материала и обработку
            string _material = SQL_List_querry("SELECT materials.name FROM test2base.materials WHERE (id_material = (SELECT id_material FROM test2base.specimens WHERE (idspecimens = '"+ id_spec + "')))")[0];
            string _treat = SQL_List_querry("SELECT treatment.name FROM test2base.treatment WHERE (id_treatment = (SELECT id_treatment FROM test2base.specimens WHERE (idspecimens = '" + id_spec + "')))")[0];
            Discord.Webhook.DiscordWebhookClient disc_client = new Discord.Webhook.DiscordWebhookClient(hook);
            string message = id_spec + " " +_material + " " +_treat + " " + action_t + ", " + String.Join(" ", actors.ToArray());
            await disc_client.SendMessageAsync(message, voice, null, "RSB", null, null, Discord.AllowedMentions.All);
            disc_client.Dispose();
        }
        private string Show_history(int spec_id)
        {
            //показываем контекстную историю выбранного образца
            // показываются перемещения, состояния

            //заглушка
            //string cur_user = "testadmin";
            string ans = "История действий с образцом\n";
            using (MySqlConnection conn = New_connection(conn_str))
            {
                try
                {
                    conn.Open();
                    string sql_comand = "SELECT history.date, history.user, history.action, history.old, history.new  " +
                    "FROM test2base.history " +                    
                    "WHERE history.id_spec="+spec_id.ToString() +
                    " ORDER BY history.date";                                                                                                                                           
                    using (MySqlCommand comand = new MySqlCommand(sql_comand, conn))
                    {
                        using (MySqlDataReader reader = comand.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    ans = ans + reader[0].ToString() + " "+reader[1].ToString() + ": " + reader[2].ToString() + " " + reader[3].ToString() + " -> " + reader[4].ToString()+"\n";
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
            return ans;
        }

        private void DataGrid_specimens_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button==MouseButtons.Right && dataGrid_specimens.Rows[dataGrid_specimens.CurrentCell.RowIndex].Cells[0].Value != null)
            {
                string idnex = dataGrid_specimens.Rows[dataGrid_specimens.CurrentCell.RowIndex].Cells[0].Value.ToString();
                //MessageBox.Show("Spec ID = "+idnex);
                //conn_str
                MessageBox.Show(Show_history(Convert.ToInt32(idnex)));
            }
        }

        private void combox_pos_add_KeyUp(object sender, KeyEventArgs e)
        {
            combox_pos_add.Text = "";
        }

        private void combox_priority_KeyUp(object sender, KeyEventArgs e)
        {
            combox_priority.Text = "";
        }

        private void combox_change_priority_KeyUp(object sender, KeyEventArgs e)
        {
            combox_change_priority.Text = "";
        }

        private void btn_priority_ch_Click(object sender, EventArgs e)
        {
            if (dataGrid_specimens.CurrentRow.Cells[0].Value!=null && combox_change_priority.Text!="")
            {
                int indexx = Convert.ToInt32(dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value);
                using (MySqlConnection conn = New_connection(conn_str))
                {
                     conn.Open();
                     string sqlcom_3 = "UPDATE test2base.specimens SET priority = '"+combox_change_priority.Text+"' WHERE (idspecimens = " + indexx.ToString() + ")";
                     using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
                     {
                         //comand.Parameters.AddWithValue("@foto_before", path_TEM_b);
                         comand.ExecuteNonQuery();
                     }
                     conn.Close();                    
                }
                Log_action(Properties.Settings.Default.default_username, "change priority", dataGrid_specimens.CurrentRow.Cells[8].Value.ToString(), combox_change_priority.Text, indexx.ToString());
                Refresh_datagrid();
                //Do_filters(data_filters);               
            }
            else
            {
                MessageBox.Show("Specimen or new priority value is missed");
            }
        }

        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FontDial_specim.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    this.Font = new Font(FontDial_specim.Font, this.Font.Style);
                    //сохраним выбранный стиль
                    //Properties.Settings.Default.font_config = this.Font.ToString();
                    //Properties.Settings.Default.Save();
                    //MessageBox.Show(Properties.Settings.Default.font_config);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void Timer_for_refresh_Tick(object sender, EventArgs e)
        {
            //просто таймер для обновления системы раз в Х секунд
            refresh_counter = refresh_counter+1;
            //закрываем базу после 3 по 5 минут простоя
            //MessageBox.Show("tick "+refresh_counter.ToString());
            if (refresh_counter == 4)
            {
                Parent_form.Close_all();
            }
            else
            {
                Refresh_datagrid();
            }
        }


        private void Form_specimens_MouseMove(object sender, MouseEventArgs e)
        {
            timer_for_refresh.Stop();
            timer_for_refresh.Start();
            refresh_counter = 0;
        }

        private void dataGrid_specimens_MouseMove(object sender, MouseEventArgs e)
        {
            timer_for_refresh.Stop();
            timer_for_refresh.Start();
            refresh_counter = 0;
        }

        private void combox_setup_KeyUp(object sender, KeyEventArgs e)
        {
            //шоб не придумывали свои установки
            combox_setup.Text = "";
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            //изменение обработки образца
            //проверка - есть ли у вас права (спойлер - нет)
            if (Check_access_project(txtbox_respon_inf.Text, Properties.Settings.Default.default_username))
            {
                //поиск есть ил такая обработка, если нет, то создаем новую
                string new_treatment = "";
                using (MySqlConnection conn = New_connection(conn_str))
                {
                    try
                    {
                        new_treatment = Check_for_exist(txtbox_treat_inf.Text, conn, "treatment", "id_treatment", "name");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка при обнолвении обработки", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                    }
                }
                using (MySqlConnection conn = New_connection(conn_str))
                {
                    conn.Open();
                    string sqlcom_3 = "UPDATE test2base.specimens SET id_treatment = '" + new_treatment + "' WHERE (idspecimens = " + dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value.ToString() + ");";
                    using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
                    {
                        comand.ExecuteNonQuery();
                    }
                    conn.Close();
                }

                //лог действия
                Log_action(Properties.Settings.Default.default_username, "change treatment", Properties.Settings.Default.old_treatment, txtbox_treat_inf.Text, dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value.ToString());
            }
            else
            {
                MessageBox.Show("The treatment could be changed only by project responsible");
            }
        }

        private void txtbox_treat_inf_KeyUp(object sender, KeyEventArgs e)
        {
            //должны сохранить предыдущее значение
            //MessageBox.Show("кто-то пытается удалить важнейшую информацию");
            //Properties.Settings.Default.old_treatment = txtbox_treat_inf.Text;
            //Properties.Settings.Default.Save();
        }

        private void txtbox_treat_inf_Enter(object sender, EventArgs e)
        {
            //MessageBox.Show("кто-то пытается удалить важнейшую информацию");
            Properties.Settings.Default.old_treatment = txtbox_treat_inf.Text;
            Properties.Settings.Default.Save();
        }

        private void btn_test_telegrambot_Click(object sender, EventArgs e)
        {
            //тестируем телеграм бот
            //1662678916:AAH63Zoolu7RgaZr_Cw9dtH6X3PIyQ5iQmk
            var proxy = new HttpToSocks5Proxy(new[] {
                new ProxyInfo("207.97.174.134", 1080),
                new ProxyInfo("23.106.35.130",1637),
                new ProxyInfo("135.181.203.208",80)
            });
            proxy.ResolveHostnamesLocally = true;
            //var botClient = new TelegramBotClient("", proxy);
            //var me = botClient.GetMeAsync().Result;
            //MessageBox.Show("My first bot is "+me.FirstName);
            //botClient.OnMessage += Bot_OnMessage;
            //botClient.StartReceiving();            

        }
        /// <summary>
        /// читаем файл как бинарник
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private byte[] GetBinaryFile(string filename)
        {
            byte[] ans;
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                ans = new byte[fs.Length];
                fs.Read(ans, 0, (int)fs.Length);
                MessageBox.Show("array length = " +fs.Length.ToString());
            }
            
            return ans;
        }
        /// <summary>
        /// запись данных в БД с параметризованным полем файл @file, data - byte[]
        /// </summary>
        /// <param name="sqlreq"></param>
        /// <param name="data"></param>
        private void File_to_DB_test(string sqlreq, byte[] data)
        {
            using (MySqlConnection conn = New_connection(conn_str))
            {
                conn.Open();                
                using (MySqlCommand comand = new MySqlCommand(sqlreq, conn))
                {
                    comand.Parameters.AddWithValue("@file", data);                    
                    _ = comand.ExecuteNonQuery();
                }
                conn.Close();
            }
        }
        private byte[] ReadDataFromDB(string sqlreq)
        {
            //List<byte> list_data = new List<byte>();
            byte[] ans = new byte[] { };
            try
            {
                using (MySqlConnection conn = New_connection(conn_str))
                {
                    conn.Open();
                    using (MySqlCommand comand = new MySqlCommand(sqlreq, conn))
                    {
                        using (MySqlDataReader reader = comand.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    //list_data.Add(reader[0].);
                                    //MessageBox.Show(reader[0].GetType().Name);
                                    ans = (byte[]) reader[0];
                                }
                            }
                            reader.Close();
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("error in ReadDataFromDB:\n" + sqlreq + "\n" + ex.ToString());
            }

            if (ans.Length == 0) ans = new byte[] { 0 };
            return ans;
        }
        private void Save_file_From_DB(byte[] data)
        {
            try
            {
                using (FileStream fs = new FileStream(@"C:\Users\antch\Documents\test_pic.bmp", FileMode.CreateNew))
                {
                    fs.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "error in Save_file_From_DB", MessageBoxButtons.OK, MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button1);
            }
        }
        private async Task test_D_message(string mess)
        {
            _client = new DiscordSocketClient();
            //получаем токен бота
            var token = SQL_List_querry("SELECT producers.token FROM test2base.producers WHERE (user_name = 'chuk')")[0];
            await _client.LoginAsync(Discord.TokenType.Bot, token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }
        private async void btn_test_Click(object sender, EventArgs e)
        {
            //кнопка для разных тестов
            MessageBox.Show("test discord bot");
            //шлем сообщение            
           /* Discord.Webhook.DiscordWebhookClient cl = new Discord.Webhook.DiscordWebhookClient(Properties.Settings.Default.disc_hook_research);
            await cl.SendMessageAsync("Тридцать три корабля лавировали-лавировали, лавировали-лавировали, " +
                "лавировали-лавировали, да не вылавировали, не вылавировали, не вылавировали, тридцать три корабля", true, null, "loh");*/
            //cl.Dispose();
            //cl.
        }

        private void btn_clr_storage_Click(object sender, EventArgs e)
        {
            //очистка фильтра storage
            isrefreshing = false;
            for (int i = 0; i < ch_listbox_storage_f.Items.Count; i++)
            {
                ch_listbox_storage_f.SetItemChecked(i, false);
            }
            isrefreshing = true;
            Refresh_datagrid();
        }

        private void btn_sel_storage_Click(object sender, EventArgs e)
        {
            //выбор всех фильтров по Storage
            isrefreshing = false;
            for (int i = 0; i < ch_listbox_storage_f.Items.Count; i++)
            {
                ch_listbox_storage_f.SetItemChecked(i, true);
            }
            isrefreshing = true;
            Refresh_datagrid();
        }

        private void ch_listbox_storage_f_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            Checked(e, ch_listbox_storage_f, isrefreshing);
        }
        private void Fill_chbox(CheckedListBox ch_box, string ch_box_filter)
        {
            //ставим галку у найденного элемента ListCheckBox
            for (int i = 0; i < ch_box.Items.Count; i++)
            {                                
                if (ch_box.Items[i].ToString() == ch_box_filter)
                {
                    ch_box.SetItemChecked(i, false);
                }
            }
        }
        private void Fill_chbox_true(CheckedListBox ch_box)
        {
            //просто ставим у ListCheckedBox все галочки
            for (int i = 0; i < ch_box.Items.Count; i++)
            {
                ch_box.SetItemChecked(i, true);
            }
        }
        private bool Reverse_ch_boxes_filtres()
        {
            bool ans = false;
            //процедура проставляет галочик в чек-боксах в соответствии с фильтрами
            if (filt_master.common_filters!=null)
            if (filt_master.common_filters.Count>0)
            {
                //в фильтрах что-то есть, проверяем каждый чек-бокс     
                //ставим везде галочки
                Fill_chbox_true(ch_listbox_type);
                Fill_chbox_true(ch_listbox_state_f);
                Fill_chbox_true(ch_listbox_project);
                Fill_chbox_true(ch_listbox_storage_f);
                Fill_chbox_true(ch_listbox_material);
                foreach (string filt in filt_master.common_filters)
                {
                    string ch_box_name = filt.Substring(0, filt.IndexOf("<")-1);
                    string ch_box_filter = filt.Substring(filt.IndexOf("'")+1, filt.Length-filt.IndexOf("'")-2);                    
                    switch (ch_box_name)
                    {
                        case "type.name":
                            Fill_chbox(ch_listbox_type, ch_box_filter);
                            break;
                        case "projects.name":
                            Fill_chbox(ch_listbox_project, ch_box_filter);
                            break;
                        case "state.name":
                            Fill_chbox(ch_listbox_state_f, ch_box_filter);
                            break;
                        case "materials.name":
                            Fill_chbox(ch_listbox_material, ch_box_filter);
                            break;
                        case "storage.name":
                            Fill_chbox(ch_listbox_storage_f, ch_box_filter);
                            break;
                    }    
                }
            }
            return ans;
        }
        private void Load_def_json(string name)
        {
            //процедура грузит из json фильтры с обновлением
            try
            {
                string json_path = Directory.GetCurrentDirectory() + name;
                if (File.Exists(json_path))
                {
                    //filt_master = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(json_path));
                    filt_master = JsonConvert.DeserializeObject<Filtres_master>(File.ReadAllText(json_path));
                    /*if (filt_master.is_special_filter == true)
                    {
                        btn_sql_filter_special.BackColor = Color.Green;
                        richTextBox_special_filt.Text = filt_master.special_filter;
                    }
                    else btn_sql_filter_special.BackColor = Color.Red;*/
                    isrefreshing = false;
                    on_load = true;
                    Reverse_ch_boxes_filtres();
                    isrefreshing = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка при загрузке дефотных фильтров", MessageBoxButtons.OK, MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button1);
            }
        }
        private void Save_def_json (string name)
        {
            string json_path = Directory.GetCurrentDirectory() + name;
            //DirectoryInfo dirinfo = new DirectoryInfo(json_path + @"\Settings");            
            //попытаться сохранить фильтры в json
            using (StreamWriter file = File.CreateText(json_path))
            {
                JsonSerializer seriz = new JsonSerializer();
                //seriz.Serialize(file, data_filters);
                seriz.Serialize(file, filt_master);
            }
        }

        private void Form_specimens_Deactivate(object sender, EventArgs e)
        {
            Save_settings();
            //сохранить фильтры в json
            Save_def_json(@"\Settings\test_json.json");
        }

        private void button1_Click_3(object sender, EventArgs e)
        {
            /*
            //кнопка выполнения специального запроса фильтрования
            if (filt_master.is_special_filter == false)
            {
                btn_sql_filter_special.BackColor = Color.Green;
                filt_master.is_special_filter = true;
                filt_master.special_filter = richTextBox_special_filt.Text;
            }
            else
            {
                btn_sql_filter_special.BackColor = Color.Red;
                filt_master.is_special_filter = false;
            }
            Refresh_datagrid();*/
        }

        private void dataGrid_specimens_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            //MessageBox.Show("Случилось RowValidated");
        }

        private void dataGrid_specimens_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            //MessageBox.Show("Случилось RowEnter");
        }

        private void txtbox_temperature_KeyUp(object sender, KeyEventArgs e)
        {
            //валидатор на ввод только цифр
            
        }

        private void txtbox_temperature_KeyPress(object sender, KeyPressEventArgs e)
        {
            /*
            //валидатор на ввод только цифр
            char number = e.KeyChar;
            int n = (int)number;
            //MessageBox.Show("test= " + n.ToString());
            if (n != 8 && txtbox_temperature.Text != "")
            {
                if (!decimal.TryParse(txtbox_temperature.Text + number, out _))
                {
                    txtbox_temperature.Text = "";
                    MessageBox.Show("вы ввели какую-то фигню, можно вводить только числа вида 1548,003  или 0 или 25");
                    e.Handled = true;
                }
            }*/
        }

        private void txtbox_las_pow_KeyPress(object sender, KeyPressEventArgs e)
        {
            /*char number = e.KeyChar;
            int n = (int)number;
            //MessageBox.Show("test= " + n.ToString());
            if (n != 8 && txtbox_las_pow.Text!="")
            {
                if (!decimal.TryParse(txtbox_las_pow.Text + number, out _))
                {
                    txtbox_las_pow.Text = "";
                    MessageBox.Show("вы ввели какую-то фигню, можно вводить только числа вида 1548,003  или 0 или 25");
                    e.Handled = true;
                }
            }*/
        }

        private void combox_treatment_Leave(object sender, EventArgs e)
        {
            //тест валидатора, который заменяет все запятые и точки на нижнее подчеркивание
            combox_treatment.Text = combox_treatment.Text.Replace('.', '_');
            combox_treatment.Text = combox_treatment.Text.Replace(',', '_');
            combox_treatment.Text = combox_treatment.Text.Replace('/', '_');
            combox_treatment.Text = combox_treatment.Text.Replace('%', '_');
            combox_treatment.Text = combox_treatment.Text.Replace('!', '_');
            combox_treatment.Text = combox_treatment.Text.Replace('@', '_');
            //combox_treatment.Text = combox_treatment.Text.Replace(',', '_');

        }

        private void txtbox_data_dir_Leave(object sender, EventArgs e)
        {
            //тест валидатор на существование пути
            if (!Directory.GetParent(txtbox_data_dir.Text).Exists)
            {
                txtbox_data_dir.Text = "";
                MessageBox.Show("Компьютер считает, что вашей диретокрии не существует, просьба ввести правильную");
            }
        }
        /// <summary>
        /// простой апрос sql INSERT DELETE UPDATE
        /// </summary>
        /// <param name="sql_req"></param>
        private bool Simple_SQL_req(string sql_req)
        {
            bool finished_well = false;
            using (MySqlConnection conn = New_connection(conn_str))
            {
                conn.Open();
                //string sqlcom_3 = sql_req;
                using (MySqlCommand comand = new MySqlCommand(sql_req, conn))
                {
                    try
                    {
                        comand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + " :\n "+sql_req, "Ошибка в Simple_SQL_req в запросе", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                    }
                }
                conn.Close();
            }
            finished_well = true;
            return finished_well;
        }
        /// <summary>
        /// добавляет  новые целевые установки в базу по чек-листбоксу
        /// </summary>
        /// <param name="l_box"></param>
        private void Ch_list_ad_new(CheckedListBox l_box, int id)
        {
            //получаем список чекнутых установок
            List<string> setup_list = new List<string>();
            foreach (object item in l_box.CheckedItems)
            {
                setup_list.Add(item.ToString());
            }
            //MessageBox.Show("is checked "+ setup_list.);            
            //если не пустая строка - добавляем установки, предварительно удалив последний символ
            if (setup_list.Count > 0)
            {
                for (int ind = 0; ind < setup_list.Count; ind++)
                {
                    Simple_SQL_req("INSERT INTO test2base.setup_specimen (`id_setup`, `id_specimen`) " +
                        "VALUES ((SELECT id_setups FROM test2base.setups WHERE (Name = '" + setup_list[ind].ToString() + "')), '" + id.ToString() + "');");
                }
            }
            //пишем в историю
            Log_action(Properties.Settings.Default.default_username, "change target setups", "unknown", String.Join(", ", setup_list), id.ToString());
        }
        /// <summary>
        /// изменение целевых установок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click_4(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value);
            //удаляем все целевые установки
            Simple_SQL_req("DELETE FROM test2base.setup_specimen WHERE (id_specimen = " + id.ToString() + ");");
            //добавляем новые целевые установки
            Ch_list_ad_new(ch_listbox_setup_inf, id);
        }
        /// <summary>
        /// Фильтруем табличку по образца для установки с ид
        /// is_in - если true - внутри установок
        /// false - очередь+ внутри установок
        /// </summary>
        /// <param name="id">1 - ПАЗЛ, 2 - ЛАЗТ, 3 - АТЛАЗ</param>
        /// <param name="is_in"></param>
        private void Setup_filter(int id, bool is_in)
        {
            //фильтры если is-In - только то, что в установке
            filt_master.is_special_filter = true;
            if (is_in)
            {                                
                switch (id)
                {
                    case 1:
                        filt_master.special_filter = " WHERE ((storage.name = 'ПАЗЛ') OR (storage.name = 'ПАЗЛ Кассета')) ORDER BY idspecimens DESC";
                        break;
                    case 2:
                        filt_master.special_filter = " WHERE ((storage.name = 'ЛАЗТ') OR (storage.name = 'ЛАЗТ Загрузка') " +
                            "OR (storage.name = 'ЛАЗТ Барабан')) ORDER BY idspecimens DESC";
                        break;
                    case 3:
                        filt_master.special_filter = " WHERE ((storage.name = 'АТЛАЗ') OR (storage.name = 'АТЛАЗ Загрузка') " +
                            "OR (storage.name = 'АТЛАЗ Барабан'))  ORDER BY idspecimens DESC";
                        break;
                }
            }            
            else
            {
                filt_master.special_filter = " WHERE (id_setup = " + id.ToString() + ") " +
                "AND (state.name = 'Ready for APT')";
            }
            Refresh_datagrid();
        }
        /// <summary>
        /// включено отображение очереди АТЛАЗ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radio_btn_ATLAS.Checked)
            {                
                Setup_filter(3,false);                
            }
        }
        /// <summary>
        /// ОТключено отображение очереди
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radio_btn_none_CheckedChanged(object sender, EventArgs e)
        {
            if (radio_btn_none.Checked)
            {
                filt_master.is_special_filter = false;
                Refresh_datagrid();
            }
        }
        /// <summary>
        /// включено отображение очереди ЛАЗТ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radio_btn_LAZT_CheckedChanged(object sender, EventArgs e)
        {
            if (radio_btn_LAZT.Checked)
            {
                Setup_filter(2,false);
            }
        }
        /// <summary>
        /// включено отображение очереди ПАЗЛ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radio_btn_APPLE_CheckedChanged(object sender, EventArgs e)
        {
            if (radio_btn_APPLE.Checked)
            {
                Setup_filter(1,false);
            }
        }
        /// <summary>
        /// включено отображение внутри ПАЗЛ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radio_btn_inAPPLE_CheckedChanged(object sender, EventArgs e)
        {
            if (radio_btn_inAPPLE.Checked)
            {
                Setup_filter(1,true);                
            }
        }
        /// <summary>
        /// включено отображение внутри АТЛАЗ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radio_btn_inATLAS_CheckedChanged(object sender, EventArgs e)
        {
            if (radio_btn_inATLAS.Checked)
            {
                Setup_filter(3,true);
            }
        }
        /// <summary>
        /// включено отображение внутри ЛАЗТ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radio_btn_inLAZT_CheckedChanged(object sender, EventArgs e)
        {
            if (radio_btn_inLAZT.Checked)
            {
                Setup_filter(2,true);
            }
        }
        /// <summary>
        /// создаем новый образец
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_aproove_Click(object sender, EventArgs e)
        {
            //проверка все ли поля заполнены
            if (Ch_fields())
            {
                //проврека есть ли такая уже запись по дата+изготовитель+материал+проект
                //если нет, то создем новый
                New_specimen();
                Refresh_datagrid();
                //очистить часть полей для заполнения
                Clear_pics_info(1);
                Clear_pics_info(2);
                combox_pos_add.Text = "";
                combox_pos_add.Items.Clear();
                combox_setup.SelectedIndex = -1;
            }
        }
        /// <summary>
        /// сохраняем дефолтные настройки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_default_save_Click(object sender, EventArgs e)
        {
            Save_settings();
        }
        /// <summary>
        /// Очистка фото
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_foto_clr_Click(object sender, EventArgs e)
        {
            Clear_one_picbox(picbox_before_big);
            Clear_one_picbox(picbox_before_sm1);
            Clear_one_picbox(picbox_before_sm2);
            Clear_one_picbox(picbox_before_sm3);
            GC.Collect();
            //обнулить путь картинок
            images_paths.Clear();
        }
        /// <summary>
        /// Изменить комментарии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ch_comments_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value);
            if (id.ToString() != "")
            {
                Simple_SQL_req("UPDATE test2base.specimens SET comments = '" + rich_txtbox_comments_info.Text + "' WHERE (idspecimens = " + id.ToString() + ");");
            }
        }
        /// <summary>
        /// меняем или создаем новый состав
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ch_composotion_Click(object sender, EventArgs e)
        {            
            //имщем материал + состав

            //обновляем состав            
            if (int.TryParse(dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value.ToString(), out int id_spec))
            {
                Simple_SQL_req("UPDATE test2base.materials SET composition = '" + txt_composition.Text + "' " +
                "WHERE (id_material = (SELECT id_material FROM test2base.specimens WHERE idspecimens = '" + id_spec.ToString() + "'));");
            }                      
        }
        /// <summary>
        /// проверка не в установке АЗТ ли этот образец 
        /// str - место хранения + позиция "ПАЗЛ 1"
        /// true - можно удалять
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool Check_position_noAPT(string str)
        {
            //MessageBox.Show("position = "+str);
            bool ans = false;
            if (str != "ПАЗЛ 1" &&
                str != "ПАЗЛ Кассета 1" &&
                str != "ПАЗЛ Кассета 2" &&
                str != "ПАЗЛ Кассета 3" &&
                str != "ЛАЗТ 1" &&
                str != "ЛАЗТ Загрузка 1" &&
                str != "АТЛАЗ 1" &&
                str != "АТЛАЗ Загрузка 1")                
            {
                ans = true;
                if (str.Length >= 13 && str.Substring(0, 13) == "АТЛАЗ Барабан")
                {
                    ans = false;
                }
            }
            else
            {
                MessageBox.Show("wrong place");
            }
            return ans;
        }
        /// <summary>
        /// проверка можно ли удалять образец
        /// ИД образца, 
        /// state состояние Ready for APT или Storage\n, 
        /// level_access уровень доступа, 
        /// actor залогиневшийся пользователь
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state_name"></param>
        /// <param name="level_access"></param>
        /// <param name="actor"></param>
        /// <param name="producer"></param>
        /// <returns></returns>
        private bool Can_del_specimen(int id, string state_name, int level_access, string actor, string producer)
        {
            bool ans = false;
            //правила удаления
            //уровень достпа не выше 2
            //только если этот образец вы создавали 
            //только в состоянии Ready for APT или Storage 
            string actor_login = SQL_List_querry("SELECT user_name FROM test2base.producers WHERE (surname = '" + actor + "')")[0];
            string resp_login = SQL_List_querry("SELECT user_name FROM test2base.producers WHERE (surname = '" + producer + "')")[0];
            MessageBox.Show("level = " + level_access.ToString() + "\nCheck pos = " + Check_position_noAPT(dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[6].Value.ToString()).ToString() +
                "\nState name = " + state_name +
                "\nproducer = " + actor_login +
                "\nrespon = " + resp_login);            
            if (level_access <= 2 &&
                ((state_name == "Ready for APT"
                && Check_position_noAPT(dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[6].Value.ToString())) || state_name == "Storage")
                && (Properties.Settings.Default.default_username == actor_login || Properties.Settings.Default.default_username == resp_login))
            {
                ans = true;
            }
            return ans;
        }
        /// <summary>
        /// Простой SQL запрос на пполучение списка ответов List (string)
        /// </summary>
        /// <param name="sql_request"></param>
        /// <returns></returns>
        private List<string> SQL_List_querry(string sql_request)
        {
            List<string> ans = new List<string>();
            try
            {
                using (MySqlConnection conn = New_connection(conn_str))
                {
                    conn.Open();
                    using (MySqlCommand comand = new MySqlCommand(sql_request, conn))
                    {
                        using (MySqlDataReader reader = comand.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    ans.Add(reader[0].ToString());
                                }                                
                            }
                            reader.Close();
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("error in SQL_List_querry with request:\n"+sql_request+"\n" + ex.ToString());
            }
            if (ans.Count == 0) ans.Add("");
            return ans;
        }        
        /// <summary>
        /// удаление орбрзца
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_delete_selected_Click(object sender, EventArgs e)
        {
            //проверить можно ли удалять(права)
            if (int.TryParse(dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value.ToString(), out int id) && 
                Can_del_specimen(id, dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[7].Value.ToString(), 
                Properties.Settings.Default.user_access_lvl,
                dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[5].Value.ToString(),
                txtbox_respon_inf.Text))
            {
                //спрашиваем, действительно ли мы хотим удалять образец?
                DialogResult res = MessageBox.Show("Delete selected specimen?\nAre you SURE?\n no reincarnation","Warning", MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button2);
                if (res == DialogResult.Yes)
                {
                    //удаляем образец
                    //удалить из history
                    if (Simple_SQL_req("DELETE FROM test2base.history WHERE (id_spec = " + id.ToString() + ")"))
                    {
                        //удалить из storage-position
                        if (Simple_SQL_req("UPDATE test2base.storage_position SET id_specimen = 0 WHERE (id_specimen = " + id.ToString() + ")"))
                        {
                            //удалить из setup_specimens
                            if (Simple_SQL_req("DELETE FROM test2base.setup_specimen WHERE (id_specimen = " + id.ToString() + ")"))
                            {
                                //удалить из stages_specimens
                                if (Simple_SQL_req("DELETE FROM test2base.stages_specimens WHERE (id_specimen = " + id.ToString() + ")"))
                                {
                                    //удалить из specimens
                                    Simple_SQL_req("DELETE FROM test2base.specimens WHERE (idspecimens = " + id.ToString() + ")");
                                    MessageBox.Show("yes we delete specimen");
                                }
                            }
                        }
                    }
                }
                Refresh_datagrid();
            }   
            else
            {
                MessageBox.Show("deletion not permitted\n" +
                    "producer = you\n" +
                    "only Ready for APT or Storage\n" +
                    "access level <=2");
            }
        }

        private void picbox_before_big_DoubleClick(object sender, EventArgs e)
        {
            if (picbox_before_big.Image != null)
            {
                string temp_path = images_paths[0];
                images_paths.RemoveAt(0);
                images_paths.Insert(images_paths.Count, temp_path);
                picbox_before_big.Image.Dispose();
                picbox_before_big.Image = Image.FromFile(images_paths[0]);
                if (images_paths.Count > 1)
                {
                    picbox_before_sm1.Image.Dispose();
                    picbox_before_sm1.Image = Image.FromFile(images_paths[1]);
                    if (images_paths.Count > 2)
                    {
                        picbox_before_sm2.Image.Dispose();
                        picbox_before_sm2.Image = Image.FromFile(images_paths[2]);
                        if (images_paths.Count > 3)
                        {
                            picbox_before_sm3.Image.Dispose();
                            picbox_before_sm3.Image = Image.FromFile(images_paths[3]);
                        }
                    }
                }
            }
            else
            {
                //грузим новые картинки
                DialogResult result = open_dial_im_before.ShowDialog();
                bool ttt = true;
                images_paths.Clear();
                if (result == DialogResult.OK && ttt)
                {
                    int num = 0;
                    foreach (string image_path in open_dial_im_before.FileNames)
                    {
                        //MessageBox.Show(image_path);
                        string ext_name = Path.GetExtension(image_path);
                        if (ext_name == ".jpg" || ext_name == ".jpeg" || ext_name == ".png" || ext_name == ".bmp" || ext_name == ".tiff"
                            || ext_name == ".JPG"
                            || ext_name == ".PNG"
                            || ext_name == ".JPEG"
                            || ext_name == ".BMP"
                            || ext_name == ".TIF"
                            || ext_name == ".TIFF"
                            || ext_name == ".tif")
                        {
                            Image image_new = Image.FromFile(image_path);
                            images_paths.Add(image_path);
                            num++;
                            switch (num)
                            {
                                case 1:
                                    picbox_before_big.Image = image_new;
                                    break;
                                case 2:
                                    picbox_before_sm1.Image = image_new;
                                    break;
                                case 3:
                                    picbox_before_sm2.Image = image_new;
                                    break;
                                case 4:
                                    picbox_before_sm3.Image = image_new;
                                    break;
                            }
                        }
                        else MessageBox.Show("No images selected");
                    }
                    picbox_before_big.BackgroundImage = null;
                }
                else MessageBox.Show("No path or no directory");
            }
        }
        private void Fill_combox_pr_filt(ComboBox box, string sql)
        {
            box.Items.Clear();
            box.Text = "";
            box.Items.AddRange(SQL_List_querry(sql).ToArray());
        }
        private void Fill_clever_combo(ComboBox box, string response, string sql_first, string sql_next, CheckBox ch_box)
        {
            string _sql_material;
            if (Check_access_project(response, Properties.Settings.Default.default_username) && ch_box.Checked)
            {
                //_sql_material = "SELECT DISTINCT materials.name FROM test2base.materials ORDER BY materials.name ASC;";
                _sql_material = sql_first;
            }
            else
            {
                /*_sql_material = "SELECT DISTINCT materials.name FROM test2base.materialstate " +
                "LEFT OUTER JOIN test2base.materials ON materials.id_material = materialstate.id_material " +
                "WHERE (materialstate.id_project = (SELECT id_project FROM test2base.projects WHERE projects.name = '" + combox_project.Text + "'));";*/
                _sql_material = sql_next + combox_project.Text + "'));";
            }
            Fill_combox_pr_filt(box, _sql_material);
        }
        private void combox_project_SelectedIndexChanged(object sender, EventArgs e)
        {
            //меняем отвественного на того, что указан в проекте
            combox_response.Text = SQL_List_querry("SELECT producers.surname FROM test2base.projects " +
                "LEFT OUTER JOIN test2base.producers ON projects.id_respons = producers.id_producer " +
                "WHERE (projects.name = '"+combox_project.Text+"')")[0];
            //грузим материалы только для данного проекта или если вы выладелец проекта - то все доступные
            Fill_clever_combo(combox_material, combox_response.Text, 
                "SELECT DISTINCT materials.name FROM test2base.materials ORDER BY materials.name ASC;",
                "SELECT DISTINCT materials.name FROM test2base.materialstate LEFT OUTER JOIN test2base.materials ON materials.id_material = materialstate.id_material WHERE (materialstate.id_project = (SELECT id_project FROM test2base.projects WHERE projects.name = '", ch_box_all_materials);
            Fill_clever_combo(combox_treatment, combox_response.Text,
                "SELECT DISTINCT treatment.name FROM test2base.treatment ORDER BY treatment.name ASC;",
                "SELECT DISTINCT treatment.name FROM test2base.materialstate LEFT OUTER JOIN test2base.treatment ON treatment.id_treatment = materialstate.id_treatment WHERE (materialstate.id_project = (SELECT id_project FROM test2base.projects WHERE projects.name = '", ch_box_all_treats);

            /*string sql_treat;            
            
            if (Check_access_project(combox_response.Text, Properties.Settings.Default.default_username) && ch_box_all_treats.Checked)
            {
                sql_treat = "SELECT DISTINCT treatment.name FROM test2base.treatment ORDER BY treatment.name ASC; ";
            }
            else
            {
                sql_treat = "SELECT DISTINCT treatment.name FROM test2base.materialstate " +
                "LEFT OUTER JOIN test2base.treatment ON treatment.id_treatment = materialstate.id_treatment " +
                "WHERE (materialstate.id_project = (SELECT id_project FROM test2base.projects WHERE projects.name = '" + combox_project.Text + "'));";
            }
            
            //грузим обработки только для даннго проекта или если вы выладелец проекта - то все доступные            
            Fill_combox_pr_filt(combox_treatment, sql_treat);*/
        }

        private void combox_treatment_Click(object sender, EventArgs e)
        {
            
        }
        /// <summary>
        /// проверяем - если логин=ответсвенному за проект
        /// </summary>
        /// <param name="response"></param>
        /// <param name="login"></param>
        /// <returns></returns>
        private bool Check_access_project(string response, string login)
        {
            //true - если можно
            bool ans = false;
            string resp_login = SQL_List_querry("SELECT surname FROM test2base.producers WHERE (user_name = '" + login + "')")[0];
            if (resp_login == response)
            {
                ans = true;
            }
            return ans;
        }
        private void combox_material_KeyUp(object sender, KeyEventArgs e)
        {
            //проверяем имеете ли вы право создавать новые материалы
            if (!Check_access_project(combox_response.Text, Properties.Settings.Default.default_username))
            {
                combox_material.Text = "";
            }
        }

        private void combox_treatment_KeyUp(object sender, KeyEventArgs e)
        {
            //проверяем имеете ли вы право создавать новые обработки
            if (!Check_access_project(combox_response.Text, Properties.Settings.Default.default_username))
            {
                combox_treatment.Text = "";
            }
        }

        private void ch_box_all_materials_CheckedChanged(object sender, EventArgs e)
        {
            //перезаполняем комбобокс
            combox_material.Items.Clear();
            Fill_clever_combo(combox_material, combox_response.Text,
                "SELECT DISTINCT materials.name FROM test2base.materials ORDER BY materials.name ASC;",
                "SELECT DISTINCT materials.name FROM test2base.materialstate LEFT OUTER JOIN test2base.materials ON materials.id_material = materialstate.id_material WHERE (materialstate.id_project = (SELECT id_project FROM test2base.projects WHERE projects.name = '", ch_box_all_materials);
        }

        private void ch_box_all_treats_CheckedChanged(object sender, EventArgs e)
        {
            combox_treatment.Items.Clear();
            Fill_clever_combo(combox_treatment, combox_response.Text,
                "SELECT DISTINCT treatment.name FROM test2base.treatment ORDER BY treatment.name ASC;",
                "SELECT DISTINCT treatment.name FROM test2base.materialstate LEFT OUTER JOIN test2base.treatment ON treatment.id_treatment = materialstate.id_treatment WHERE (materialstate.id_project = (SELECT id_project FROM test2base.projects WHERE projects.name = '", ch_box_all_treats);
        }
    }
}
