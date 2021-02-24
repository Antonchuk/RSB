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

namespace RSB
{
    public partial class Form_specimens : Form
    {
        private readonly RSBMainForm Parent_form;
        private static string conn_str;
        private static List<string> images_paths = new List<string>();
        private static List<string> data_filters = new List<string>();
        private static bool on_load = true;
        private string[] info_files_paths_bef;
        private string[] info_files_paths_aft;
        private bool specimen_new_accepted = true;
        //для картинки на кнопке
        private bool pic_change = true; //true - up, false - down
        private int show_only_spec;
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
        private void Refresh_datagrid()
        {
            int selected_id = -1;
            //MessageBox.Show("Число строк = " + dataGrid_specimens.Rows.Count.ToString());
            if ((!on_load) && dataGrid_specimens.Rows.Count>1 && dataGrid_specimens.CurrentRow!=null)
            {
                //MessageBox.Show("Число строк = "+ dataGrid_specimens.Rows.Count.ToString());
                if (dataGrid_specimens.CurrentRow.Cells[0].Value!=null)
                {
                    selected_id = Convert.ToInt32(dataGrid_specimens.CurrentRow.Cells[0].Value);
                    //MessageBox.Show("Было выбрано" + selected_id.ToString());
                }
            }
            dataGrid_specimens.Rows.Clear();
            //удалить предыдущие
            conn_str = Get_conn_string(Properties.Settings.Default.server, Properties.Settings.Default.port,
                Properties.Settings.Default.database, Parent_form.cbox_username.Text, Parent_form.txtbox_pass.Text);
            //MessageBox.Show(conn_str);

            //может быть удалено?
            //

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
                    string sql_filtres = do_filtres_for_SQL(data_filters);
                    //DateTime.TryParse(dateTimePicker_start.Text, out DateTime temp_dat_start);
                    //DateTime.TryParse(dateTimePicker_end.Text, out DateTime temp_dat_end);
                    string sqlcom = "SELECT specimens.idspecimens, materials.name, type.name, projects.name, specimens.date_prep, producers.surname, storage.name, " +
                        "state.name, specimens.stor_position, specimens.priority " +
                    "FROM test2base.specimens " +
                    "LEFT OUTER JOIN test2base.materials ON test2base.specimens.id_material = test2base.materials.id_material " +
                    "LEFT OUTER JOIN test2base.type ON specimens.id_treat_type = type.id_type " +
                    "LEFT OUTER JOIN test2base.projects ON specimens.id_project = projects.id_project " +
                    "LEFT OUTER JOIN test2base.producers ON specimens.id_producer = producers.id_producer " +
                    "LEFT OUTER JOIN test2base.storage ON specimens.id_storage = storage.id_storage " +
                    "LEFT OUTER JOIN test2base.state ON specimens.id_state = state.id_state " +
                    //" WHERE (specimens.date_prep >= '" + temp_dat_start.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                    //" AND specimens.date_prep <= '" + temp_dat_end.ToString("yyyy-MM-dd HH:mm:ss") + "') " +
                    " WHERE (specimens.date_prep >= '" + dateTimePicker_start.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                    " AND specimens.date_prep <= '" + dateTimePicker_end.Value.ToString("yyyy-MM-dd HH:mm:ss") + "') " +
                    sql_filtres +
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
                                                case "Storage ready for APT":                                                    
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
                            else MessageBox.Show("nodata in refresh");
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
                        }
                    }
                }
            }
            //пробуем встроить фильтры
            if (!on_load)
            {
                //MessageBox.Show("Применили фильтры");
                //Do_filters(data_filters);
                Deal_with_buttons();
            }
        }
        private void Fill_one_combo(string colname, MySqlConnection conect, string table_name, string combo)
        {
            try
            {
                if (combo != "f_succ")
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
                                    }
                                }
                                reader.Close();
                            }
                        }
                        //проверить выполнен ли запрос
                        conect.Close();
                    }
                }
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
                //materials
                Fill_one_combo("name", conn, "materials", "materials");
                //response
                Fill_one_combo("surname", conn, "producers", "response");
                //researchers
                Fill_one_combo("surname", conn, "producers", "researchers");
                //тип установки
                Fill_one_combo("name", conn, "setups", "setup");


                //заполняем фильтры
                //фильтр тип образца
                Fill_one_combo("name", conn, "type", "f_type");
                //фильтр успешности
                Fill_one_combo("name", conn, "state", "f_state");
                //фильтр материала
                Fill_one_combo("name", conn, "materials", "f_material");
                //фильтр проекта
                Fill_one_combo("name", conn, "projects", "f_project");
                
            }
        }
        private void Fill_info_text_sql(int spec_id, MySqlConnection connect, string table_name, string col_name, string id_join, string id2_join)
        {
            try
            {
                connect.Open();
                string sqlcom = "SELECT " + table_name + "." + col_name + ", specimens.idspecimens FROM test2base.specimens " +
                    "LEFT OUTER JOIN test2base." + table_name + " ON specimens." + id2_join + " = " + table_name + "." + id_join +
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
                                switch (table_name)
                                {
                                    case "treatment":
                                        txtbox_treat_inf.Text = reader[0].ToString();
                                        break;
                                    case "producers":
                                        // It's a trap!
                                        //на самом деле это response
                                        txtbox_respon_inf.Text = reader[0].ToString();
                                        break;
                                    case "specimens":
                                        combox_position.Text = reader[0].ToString();
                                        break;
                                }
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
        }
        private void Fill_info_foto(int select_id, MySqlConnection connect, string table_name, string col_name)
        {
            try
            {
                connect.Open();
                string sqlcom = "SELECT " + table_name + "." + col_name + ", specimens.idspecimens FROM test2base.specimens" +
                    " WHERE idspecimens = " + select_id.ToString();
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
                                                if (ext_name == ".jpg" || ext_name == ".jpeg" || ext_name == ".png" || ext_name == ".bmp" || ext_name == ".tiff" || ext_name == ".JPG")
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
                                                if (ext_name == ".jpg" || ext_name == ".jpeg" || ext_name == ".png" || ext_name == ".bmp" || ext_name == ".tiff" || ext_name == ".JPG")
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
                                    case "stor_position":
                                        string posi = reader[0].ToString();
                                        combox_position.Text = posi;
                                        break;

                                }
                            }

                            reader.Close();
                        }
                        else
                        {
                            MessageBox.Show("должны удалять картинки");
                            if (picbox_inf_bef_1.Image != null) picbox_inf_bef_1.Image.Dispose();
                            if (picbox_inf_bef_2.Image != null) picbox_inf_bef_2.Image.Dispose();
                            if (picbox_inf_bef_3.Image != null) picbox_inf_bef_3.Image.Dispose();
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
        private void Fill_info_text(int index)
        {
            //простое заполнение из грида
            if (dataGrid_specimens.Rows[index].Cells[1].Value != null) txtbox_material_inf.Text = dataGrid_specimens.Rows[index].Cells[1].Value.ToString();
            if (dataGrid_specimens.Rows[index].Cells[5].Value != null) txtbox_producer_inf.Text = dataGrid_specimens.Rows[index].Cells[5].Value.ToString();
            if (dataGrid_specimens.Rows[index].Cells[2].Value != null) txtbox_type_inf.Text = dataGrid_specimens.Rows[index].Cells[2].Value.ToString();
            if (dataGrid_specimens.Rows[index].Cells[4].Value != null) txtbox_date_inf.Text = dataGrid_specimens.Rows[index].Cells[4].Value.ToString();
            if (dataGrid_specimens.Rows[index].Cells[3].Value != null) txtbox_project_inf.Text = dataGrid_specimens.Rows[index].Cells[3].Value.ToString();
            if (dataGrid_specimens.Rows[index].Cells[7].Value != null) txtbox_state_inf.Text = dataGrid_specimens.Rows[index].Cells[7].Value.ToString();
            if (dataGrid_specimens.Rows[index].Cells[6].Value != null) txtbox_storage_inf.Text = dataGrid_specimens.Rows[index].Cells[6].Value.ToString();
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
                    Fill_info_text_sql(i, conn, "treatment", "Name", "id_treatment", "id_treatment");
                    //response
                    Fill_info_text_sql(i, conn, "producers", "surname", "id_producer", "id_respon");
                    //storage position
                    Fill_info_foto(i, conn, "specimens", "stor_position");
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
            if (dataGrid_specimens.Rows.Count > 0 && dataGrid_specimens.CurrentRow != null)
            {
                int Sel_index = dataGrid_specimens.CurrentRow.Index;
                Fill_info_text(Sel_index);
            }
        }
        private void Form_specimens_Load(object sender, EventArgs e)
        {
            on_load = true;
            //TypeConverter converter_t = TypeDescriptor.GetConverter(typeof(Font));
            //this.Font = (Font)converter_t.ConvertFromString(Properties.Settings.Default.font_config);
            //MessageBox.Show("load: \n " + Properties.Settings.Default.font_config);
            //MessageBox.Show("размер хххх = "+this.Font.Size.ToString() );
            //MessageBox.Show("value= " + dateTimePicker_end.Value.ToString() + "\n text=" + dateTimePicker_end.Text);
            show_only_spec = Properties.Settings.Default.show_only_specimens;
            if (combox_showonly.Text != "All") combox_showonly.Text = show_only_spec.ToString();
            btn_up_down.Image = Properties.Resources.down;
            pic_change = false;
            combox_material.Text = Properties.Settings.Default.material_add;
            combox_producer.Text = Properties.Settings.Default.producer;
            combox_project.Text = Properties.Settings.Default.project;
            combox_storage.Text = Properties.Settings.Default.storage;
            combox_treatment.Text = Properties.Settings.Default.treatment;
            combox_treat_type.Text = Properties.Settings.Default.type_prep;
            combox_response.Text = Properties.Settings.Default.respons;
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
            dateTimePicker_end.Value = DateTime.Now;
            DateTime tem_dat;
            tem_dat = dateTimePicker_end.Value;
            dateTimePicker_start.Value = tem_dat.AddYears(-1);
            Refresh_datagrid();
            Fill_information();
            on_load = false;
            //запуск таймера на циклическое обновление
            timer_for_refresh.Start();
        }

        private void Btn_refresh_Click(object sender, EventArgs e)
        {
            //dataGrid_specimens.Rows.Clear();
            Refresh_datagrid();
            //Do_filters(data_filters);
        }

        private void Tab_page_new_edit_Enter(object sender, EventArgs e)
        {
            //MessageBox.Show("test");            

        }
        private bool Ch_fields()
        {
            //проврека на заполненность всех полей на форме ADD_EDIT
            if (combox_treat_type.Text != "" && combox_treatment.Text != "" && combox_storage.Text != "" &&
                combox_project.Text != "" && combox_producer.Text != "" && combox_material.Text != "" && date_time_add_edit.Text != "" && combox_pos_add.Text != "")
            {
                return true;
            }
            else
            {
                MessageBox.Show("Not all fields are filled!");
                return false;
            }

        }
        private string Check_for_exist(string surname, MySqlConnection connect, string table_name, string col_name, string name2)
        {
            bool need_new = false;
            string ans = "";
            connect.Open();
            string sqlcom = "SELECT " + col_name + " FROM test2base." + table_name + " WHERE " + name2 + " = '" + surname + "'";
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
                            //MessageBox.Show("хорошо, есть уже такой, ИД=" + ans);
                        }
                        reader.Close();
                    }
                    else
                    {
                        //MessageBox.Show("не такого "+table_name+", сейчас добавим");                        
                        //add_new producer
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
                        add_new_material.ShowDialog();
                        add_new_material.Dispose();
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
                        //Form add_new_material = new Materials_new();
                        //add_new_material.ShowDialog();
                        //add_new_material.Dispose();
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
                        string sqlcom_3 = "INSERT INTO test2base.specimens (id_producer, id_state, date_prep, id_project, id_storage, id_treatment, " +
                            "id_treat_type, id_respon, place_foto_bef, place_foto_after, id_material,stor_position,priority) VALUES (@id_producer,@id_state,@datetime,@id_project,@id_storage,@treatment," +
                        "@id_treat_type,@id_respon,@foto_before,@foro_after,@id_material,@stor_position,@priority)";
                        using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
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
                            comand.Parameters.AddWithValue("@stor_position", new_spec.stor_pos);
                            comand.Parameters.AddWithValue("@priority", new_spec.priority);
                            //MessageBox.Show(comand.CommandText);
                            comand.ExecuteNonQuery();
                            //проверить выполнен ли запрос
                            conn.Close();
                        }

                        //получить новый ID                        

                        conn.Open();
                        //MessageBox.Show("Дата для сравнения"+new_spec.datetime);
                        sqlcom_3 = "SELECT idspecimens FROM test2base.specimens WHERE id_producer=@id_producer AND id_state=@id_state AND date_prep=@datetime AND " +
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
                                        id_new = Convert.ToInt32(reader[0]);
                                    }
                                    reader.Close();
                                }
                            }
                        }
                        //MessageBox.Show("найденный индекс ="+indexx.ToString());
                        conn.Close();

                        //MessageBox.Show("Проверка на ошибку, это до копирования фото");

                        new_spec.foto_before = Copy_fotos(new_spec.foto_before, dir_foto_new, 1,id_new);

                        //MessageBox.Show("Проверка на ошибку, это после копирования фото");
                        //поменять название папки фото до
                        conn.Open();
                        sqlcom_3 = "UPDATE test2base.specimens SET place_foto_bef =@foto_before WHERE (idspecimens = " + id_new.ToString()+")";
                        using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
                        {
                            comand.Parameters.AddWithValue("@foto_before", new_spec.foto_before);
                            comand.ExecuteNonQuery();
                        }
                        conn.Close();
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
        private void Btn_write_to_base_Click(object sender, EventArgs e)
        {
            //проверка все ли поля заполнены
            if (Ch_fields())
            {
                //проврека есть ли такая уже запись по дата+изготовитель+материал+проект
                //если нет, то создем новый
                New_specimen();
                Refresh_datagrid();
            }
            else
            {
                MessageBox.Show("Не все поля заполнены");
            }
        }

        private void Picbox_big_Click(object sender, EventArgs e)
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
                        if (ext_name == ".jpg" || ext_name == ".jpeg" || ext_name == ".png" || ext_name == ".bmp" || ext_name == ".tiff" || ext_name == ".JPG")
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
            catch (Exception ex)
            {
                MessageBox.Show("error" + ex.ToString());
            }
            try
            {
                Properties.Settings.Default.res_temper = Convert.ToInt32(txtbox_temperature.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("error" + ex.ToString());
            }
            //Properties.Settings.Default.user_access_lvl = 1;
            Properties.Settings.Default.Save();
        }
        private void Btn_save_def_Click(object sender, EventArgs e)
        {
            Save_settings();
        }

        private void Form_specimens_FormClosing(object sender, FormClosingEventArgs e)
        {
            Save_settings();
            e.Cancel = true;
            //MessageBox.Show("Нельзя просто закрыть окно. \n Надо решить задание. \n Если правильно решишь, то больше будешь играть.");
            //else e.Cancel = false;
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
                    else MessageBox.Show("SMTH wrong with 'Show_only Box'");
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
                    else MessageBox.Show("SMTH wrong with 'Show_only Box'");
                }
                Refresh_datagrid();
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Fill_information();
        }

        private void DataGrid_specimens_SelectionChanged(object sender, EventArgs e)
        {
            Fill_information();

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
                            Refresh_datagrid();
                            //Do_filters(data_filters);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                        }
                    }
                    Refresh_datagrid();
                }
            }
            else
            {
                MessageBox.Show("У вас не достаточно прав доступа к данной функции, обратитесь к администратору");
            }
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

        private void Fill_num_combo(string storage_name, int type_fill)
        {
            //подгрузить другой набор позиций
            if (storage_name != "")
            {
                switch (type_fill)
                {
                    case 1:
                        combox_pos_add.Items.Clear();
                        break;
                    case 2:
                        combox_move_pos.Items.Clear();
                        break;
                }
                //MessageBox.Show("Тип = "+ type_fill.ToString());
                conn_str = Get_conn_string(Properties.Settings.Default.server, Properties.Settings.Default.port,
                    Properties.Settings.Default.database, Parent_form.cbox_username.Text, Parent_form.txtbox_pass.Text);
                using (MySqlConnection conn = New_connection(conn_str))
                {
                    try
                    {
                        conn.Open();
                        string sqlcom_3 = "SELECT capacity FROM test2base.storage WHERE storage.name ='" + storage_name + "'";
                        //MessageBox.Show("Запрос ="+sqlcom_3);
                        using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
                        {
                            using (MySqlDataReader reader = comand.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        //ans = reader[0].ToString();
                                        int max = Convert.ToInt32(reader[0]);
                                        //MessageBox.Show("Capacity = "+max.ToString());
                                        for (int i = 1; i < max; i++)
                                        {
                                            switch (type_fill)
                                            {
                                                case 1:
                                                    combox_pos_add.Items.Add(i.ToString());
                                                    break;
                                                case 2:
                                                    combox_move_pos.Items.Add(i.ToString());
                                                    break;
                                            }
                                        }
                                        //MessageBox.Show("новый ид"+table_name+"=" + ans);
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
                    conn.Close();

                }
            }
        }
        private void Combox_storage_SelectedIndexChanged(object sender, EventArgs e)
        {
            //подгрузить другой набор позиций
            Fill_num_combo(combox_storage.Text, 1);
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
                case "Storage ready for APT":
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
            //Deal_with_buttons();
            //Do_filters(data_filters);
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
            Fill_num_combo(combox_move_to.Text, 2);
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
        private void Btn_move_new_stor_Click(object sender, EventArgs e)
        {
            if ((combox_move_to.Text != "ПАЗЛ") ^ (combox_move_to.Text == "ПАЗЛ" && Properties.Settings.Default.user_access_lvl <= 2))
            {
                //перемещение образца
                int to_ind = Get_stor_index(combox_move_to.Text);
                int to_ind_pos;
                int indexx = -1;
                if (combox_move_pos.Text != "")
                {
                    to_ind_pos = Convert.ToInt32(combox_move_pos.Text);
                }
                else to_ind_pos = -100;
                conn_str = Get_conn_string(Properties.Settings.Default.server, Properties.Settings.Default.port,
                    Properties.Settings.Default.database, Parent_form.cbox_username.Text, Parent_form.txtbox_pass.Text);
                using (MySqlConnection conn = New_connection(conn_str))
                {
                    try
                    {
                        if (to_ind != 0 && dataGrid_specimens.CurrentRow != null && to_ind_pos != -100)
                        {
                            conn.Open();
                            indexx = Convert.ToInt32(dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value);
                            string sqlcom_3 = "UPDATE test2base.specimens SET specimens.id_storage=" + to_ind.ToString() + ", specimens.stor_position=" + to_ind_pos.ToString() + " WHERE specimens.idspecimens =" + indexx.ToString();
                            using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
                            {
                                comand.ExecuteNonQuery();
                            }
                            conn.Close();
                            combox_pos_add.Text = "";
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
                    Log_action(Properties.Settings.Default.default_username, "change position", txtbox_move_from.Text, combox_move_to.Text + " " + combox_move_pos.Text, indexx.ToString());
                }
                Refresh_datagrid();
                //Do_filters(data_filters);
            }
            else
            {
                MessageBox.Show("У вас не достаточно прав доступа для перемещения образца в установку, обратитесь к администратору");
            }
        }

        private void Combox_move_pos_SelectedIndexChanged(object sender, EventArgs e)
        {
            //проверка на совпадение мест перемещения
            if (txtbox_move_from.Text == combox_move_to.Text + " " + combox_move_pos.Text)
            {
                combox_move_pos.Text = "";
                combox_move_pos.SelectedIndex = -1;
                MessageBox.Show("Wrong position!");
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
            MessageBox.Show("Ищем ид в "+where+" ид для поиска "+index);
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
                Parent_form.Show_researches_from(id_res);
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

        private void Btn_clear_foto_Click(object sender, EventArgs e)
        {            
            Clear_one_picbox(picbox_before_big);
            Clear_one_picbox(picbox_before_sm1);
            Clear_one_picbox(picbox_before_sm2);
            Clear_one_picbox(picbox_before_sm3);
            GC.Collect();
            //обнулить путь картинок
            images_paths.Clear();
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
        private void Checked(ItemCheckEventArgs e, CheckedListBox obje)
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
                }
                if (e.NewValue == CheckState.Checked)
                {
                    //удаляем из фильтров
                    //data_filters.Remove(obje.Items[e.Index].ToString());
                    data_filters.Remove(str_f + obje.Items[e.Index].ToString() + "'");
                }
                else
                {
                    //добавляем к фильтрам
                    //data_filters.Add(obje.Items[e.Index].ToString());
                    data_filters.Add(str_f +obje.Items[e.Index].ToString() + "'");
                }
                //Do_filters(data_filters);
                Refresh_datagrid();
            }
        }

        private void Ch_listbox_state_f_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            Checked(e,ch_listbox_state_f);
        }

        private void Ch_listbox_type_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            Checked(e,ch_listbox_type);
        }

        private void Ch_listbox_material_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            Checked(e, ch_listbox_material);
        }

        private void Ch_listbox_project_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            Checked(e, ch_listbox_project);
        }

        private void Btn_cl_type_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ch_listbox_type.Items.Count; i++)
            {
                ch_listbox_type.SetItemChecked(i, false);
            }
        }

        private void Btn_cl_proj_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ch_listbox_project.Items.Count; i++)
            {
                ch_listbox_project.SetItemChecked(i, false);
            }
        }

        private void Btn_cl_state_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ch_listbox_state_f.Items.Count; i++)
            {
                ch_listbox_state_f.SetItemChecked(i, false);
            }
        }

        private void Btn_cl_mat_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ch_listbox_material.Items.Count; i++)
            {
                ch_listbox_material.SetItemChecked(i, false);
            }
        }

        private void Btn_sel_type_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ch_listbox_type.Items.Count; i++)
            {
                ch_listbox_type.SetItemChecked(i, true);
            }
        }

        private void Btn_sel_proj_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ch_listbox_project.Items.Count; i++)
            {
                ch_listbox_project.SetItemChecked(i, true);
            }
        }

        private void Btn_sel_state_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ch_listbox_state_f.Items.Count; i++)
            {
                ch_listbox_state_f.SetItemChecked(i, true);
            }
        }

        private void Btn_sel_mat_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ch_listbox_material.Items.Count; i++)
            {
                ch_listbox_material.SetItemChecked(i, true);
            }
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
            //Do_filters(data_filters);
            //Deal_with_buttons();
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
                    //Do_filters(data_filters);
                    //Deal_with_buttons();
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
                    dataGrid_specimens.Rows[Get_index_datagrid(Properties.Settings.Default.main_spec_id.ToString(), 0)].Selected = true;
                }
                //зачем?
                Properties.Settings.Default.main_spec_id = -1;
                Properties.Settings.Default.Save();
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
            Refresh_datagrid();
        }


        private void Form_specimens_MouseMove(object sender, MouseEventArgs e)
        {
            timer_for_refresh.Stop();
            timer_for_refresh.Start();
        }

        private void dataGrid_specimens_MouseMove(object sender, MouseEventArgs e)
        {
            timer_for_refresh.Stop();
            timer_for_refresh.Start();
        }

        private void combox_setup_KeyUp(object sender, KeyEventArgs e)
        {
            //шоб не придумывали свои установки
            combox_setup.Text = "";
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            //изменение обработки образца
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
                string sqlcom_3 = "UPDATE test2base.specimens SET id_treatment = '"+new_treatment+"' WHERE (idspecimens = "+ dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value.ToString() + ");";
                using (MySqlCommand comand = new MySqlCommand(sqlcom_3, conn))
                {
                    comand.ExecuteNonQuery();
                }
                conn.Close();
            }

            //лог действия
            Log_action(Properties.Settings.Default.default_username, "change treatment", Properties.Settings.Default.old_treatment, txtbox_treat_inf.Text, dataGrid_specimens.Rows[dataGrid_specimens.CurrentRow.Index].Cells[0].Value.ToString());
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
    }
}
