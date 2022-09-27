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
using MySql.Data;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace RSB
{
    /// <summary>
    /// форма проекта
    /// </summary>
    public partial class Frm_projects : Form
    {
        private readonly RSBMainForm Parent_form;
        private static string conn_str="";
        private bool on_load = false;
        private static int id_project = -1;
        private List<string> filters_APT = new List<string>();
        private DataTable dt_grid = new DataTable();
        private static int selected_id = 0;
        private static bool first_draw = true;
        private static readonly Random rand = new Random();
        /// <summary>
        /// конструктор формы проекта
        /// </summary>
        public Frm_projects(RSBMainForm parent)
        {
            InitializeComponent();
            Parent_form = parent;
            conn_str = Get_conn_string(Properties.Settings.Default.server, Properties.Settings.Default.port,
               Properties.Settings.Default.database, Parent_form.cbox_username.Text, Parent_form.txtbox_pass.Text);
        }
        /// <summary>
        /// получит ьстроку для коннекта к базе
        /// </summary>
        /// <param name="myhost"></param>
        /// <param name="myport"></param>
        /// <param name="mydatabase"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private string Get_conn_string(string myhost, int myport, string mydatabase, string username, string password)
        {
            string conn_str_loc;
            conn_str_loc = "Server=" + myhost + ";Database=" + mydatabase
                + ";port=" + myport + ";User Id=" + username + ";password=" + password;
            return conn_str_loc;
        }
        /// <summary>
        /// сформировать коннекшен
        /// </summary>
        /// <param name="connString"></param>
        /// <returns></returns>
        private static MySqlConnection New_connection(string connString)
        {
            // Connection String.
            MySqlConnection conn = new MySqlConnection(connString);
            return conn;
        }
        /// <summary>
        /// простой запрос на ответ 1 текстового поля
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private List<string> SQL_str_request(string request)
        {
            List<string> ans = new List<string>();
            if (request != "")
            {
                using (MySqlConnection conn = New_connection(conn_str))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlCommand comand = new MySqlCommand(request, conn))
                        {
                            using (MySqlDataReader reader = comand.ExecuteReader())
                            {
                                
                                //ans.Add("");
                                while (reader.Read())
                                {                                    
                                    ans.Add(reader[0].ToString());
                                }
                            }
                        }
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + "\n" + request, "Ошибка в SQL_str_request", MessageBoxButtons.OK, MessageBoxIcon.Error,
                       MessageBoxDefaultButton.Button1);
                    }
                }
            }
            return ans;
        }
        /// <summary>
        /// получить ид проекта
        /// </summary>
        /// <returns></returns>
        private int Get_id_project()
        {
            int ans = 29; //РНФ МИСиС Торгом
            if (Properties.Settings.Default.info_id_project!=-1)
            {
                ans = Properties.Settings.Default.info_id_project;
            }    

            return ans;
        }
        /// <summary>
        /// Грузим поля во вкладку инфо
        /// </summary>
        private void Refresh_project_maintab()
        {
            id_project = Get_id_project();
            txtbox_info_name.Text = SQL_str_request("SELECT name FROM test2base.projects WHERE (id_project = "+id_project.ToString()+");")[0];
            txtbox_info_contract.Text = SQL_str_request("SELECT contract FROM test2base.projects WHERE (id_project = " + id_project.ToString() + ");")[0];
            txtbox_info_responsible.Text = SQL_str_request("SELECT surname FROM test2base.producers " +
                "WHERE (id_producer = (SELECT id_respons FROM test2base.projects WHERE (id_project=" + id_project.ToString() + ")));")[0];
            txtbox_info_stages_num.Text = SQL_str_request("SELECT stage_count FROM test2base.projects WHERE (id_project = " + id_project.ToString() + ");")[0];
            txtbox_start_date.Text = SQL_str_request("SELECT start_date FROM test2base.projects WHERE (id_project = " + id_project.ToString() + ");")[0];
            txtbox_priority.Text = SQL_str_request("SELECT priority FROM test2base.projects WHERE (id_project = " + id_project.ToString() + ");")[0];
            txtbox_spec_per_state.Text = SQL_str_request("SELECT specs_per_state FROM test2base.projects WHERE (id_project = " + id_project.ToString() + ");")[0];
            string end_date = SQL_str_request("SELECT end_date FROM test2base.projects WHERE (id_project = " + id_project.ToString() + ");")[0];
            if (end_date!=null && end_date != "")
            {
                dateTimePicker_pr_info_end.Value = Convert.ToDateTime(end_date);
            }
            //грузим в боксы список этапов            
            Fill_ch_box(ch_listbox_project_stage);
            Fill_combo(combox_stages, "SELECT stages.name FROM test2base.stages WHERE (id_project = " + id_project.ToString() + ")");
            listbox_stages.Items.Clear();
            listbox_stages.Items.AddRange(SQL_str_request("SELECT stages.name FROM test2base.stages WHERE (id_project = " + id_project.ToString() + ")").ToArray());
        }
        private List<string> Form_list_forGrid(DataTable dt, string querry_part1, string querry_part2)
        {
            List<string> ans = new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string anss = SQL_str_request(querry_part1 + "WHERE (specimens.id_project =  " + id_project.ToString() + ") " +
                    "AND (materials.name = '" + dt.Rows[i].ItemArray[0] + "') " +
                    "AND (treatment.name = '" + dt.Rows[i].ItemArray[1] + "') " +
                    querry_part2)[0];
                ans.Add(anss);
            }
            return ans;
        }
        /// <summary>
        /// для списка путей к файлам(с навзанием файла) считаем кол-во атомов для всех файлов(csv/ieco) в той же директории
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        private double calc_atoms(List<string> paths)
        {
            double ans = 0;
            //получаем род дриеторию
            foreach (string one_path in paths)
            {
                try
                {
                    if (one_path != "")
                    {
                        //проверить, это путь к файли или директории
                        FileInfo[] files;
                        FileInfo fi = new FileInfo(one_path);
                        if (fi.Exists)
                        {
                            DirectoryInfo par_dir = Directory.GetParent(one_path);
                            files = par_dir.GetFiles();
                        }
                        else
                        {
                            DirectoryInfo par_dir = new DirectoryInfo(one_path);
                            files = par_dir.GetFiles();
                        }
                        //список файлов

                        foreach (FileInfo inf in files)
                        {
                            //MessageBox.Show("extention "+inf.Extension);
                            if ((inf.Extension == ".csv") || (inf.Extension == ".CSV"))
                            {
                                //несколько расчетов байт на 1 атом для csv: 51.85, 51.39, 52.19, 50.63
                                //в среднем возьмем 51.5
                                ans = ans + (inf.Length) / 51.2;
                            }
                            else if (inf.Extension == ".ieco" || inf.Extension == ".IECO")
                            {
                                //на 1 атом в средне 24,008-24,0008 байт
                                //в среднем возьмем 24,004
                                ans = ans + (inf.Length) / 24.004;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in calc atoms count\n"+ex.ToString());
                }
            }
            return ans;
        }
        /// <summary>
        /// считаем число атомов в состоянии
        /// </summary>
        /// <param name="dt">таблица данных состояний со столбцами _материал, _обработка</param>
        /// <returns></returns>
        private List<double> Count_atoms(DataTable dt)
        {
            List<double> ans = new List<double>();
            //для каждого состояния
            for (int i=0;i<dt.Rows.Count; i++)
            {
                //список диретокрий для состояния
                List<string> data_dirs = SQL_str_request("SELECT researches.data_dir " +
                    "FROM test2base.specimens " +
                    "LEFT OUTER JOIN test2base.materials ON test2base.specimens.id_material = test2base.materials.id_material " +
                    "LEFT OUTER JOIN test2base.treatment ON specimens.id_treatment = treatment.id_treatment " +
                    "LEFT OUTER JOIN test2base.researches ON specimens.idspecimens = researches.id_specimen " +
                    "WHERE (specimens.id_project =  " + id_project.ToString() + ") " +
                    "AND (materials.name = '" + dt.Rows[i].ItemArray[0] + "') " +
                    "AND (treatment.name = '" + dt.Rows[i].ItemArray[1] + "') " +
                    "AND (researches.success = '+')");
                //по каждому адресу считаем размер файлов
                ans.Add(Math.Round(calc_atoms(data_dirs)));
            }
            return ans;
        }
        /// <summary>
        /// Получаем статус состояния (нужно ещё одно/в процессе обработки данных/готово)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private List<string> Get_statuses(DataTable dt)
        {
            List<string> ans = new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                List<string> str = SQL_str_request("select state.name FROM test2base.materialstate_state " +
                    "LEFT OUTER JOIN test2base.state ON materialstate_state.id_state = state.id_state " +
                    "LEFT OUTER JOIN test2base.materialstate ON materialstate_state.id_materialstate = materialstate.id_materialstate " +
                    "LEFT OUTER JOIN test2base.treatment ON materialstate.id_treatment = treatment.id_treatment " +
                    "LEFT OUTER JOIN test2base.materials ON materialstate.id_material = materials.id_material " +
                    "WHERE (materials.name = '"+ dt.Rows[i].ItemArray[0] + "')  AND(treatment.name = '"+ dt.Rows[i].ItemArray[1] + "'); ");
                if (str.Count>0)
                {
                    ans.Add(str[0]);
                }
                else
                {
                    ans.Add("");
                }
            }
            return ans;
        }
        /// <summary>
        /// считаем колчеcтво образцов в очереди
        /// </summary>
        /// <param name="dt">талица с состояниями |материал|обработка| DataTable</param>
        /// <returns></returns>
        private List<int> Specs_in_queue(DataTable dt)
        {
            List<int> ans = new List<int>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //получаем ИД образца для запроса ниже
                /*string id_spec = SQL_str_request("SELECT idspecimens " +
                    "FROM test2base.specimens " +
                    "LEFT OUTER JOIN test2base.materials ON test2base.specimens.id_material = test2base.materials.id_material " +
                    "LEFT OUTER JOIN test2base.treatment ON specimens.id_treatment = treatment.id_treatment " +
                    "WHERE (specimens.id_project =  " + id_project.ToString() + ") " +
                    "AND (materials.name = '" + dt.Rows[i].ItemArray[0] + "') " +
                    "AND (treatment.name = '" + dt.Rows[i].ItemArray[1] + "') ")[0];*/
                List<string> str = SQL_str_request("SELECT COUNT(*) " +
                    "FROM test2base.specimens " +
                    "LEFT OUTER JOIN test2base.materials ON test2base.specimens.id_material = test2base.materials.id_material " +
                    "LEFT OUTER JOIN test2base.treatment ON specimens.id_treatment = treatment.id_treatment " +
                    "LEFT OUTER JOIN test2base.storage_position ON specimens.idspecimens = storage_position.id_specimen " +
                    "WHERE (specimens.id_project =  " + id_project.ToString() + ") " +
                    "AND (materials.name = '" + dt.Rows[i].ItemArray[0] + "') " +
                    "AND (treatment.name = '" + dt.Rows[i].ItemArray[1] + "') " +
                    "AND (specimens.id_state = '1') " +
                    //"AND ((SELECT COUNT(*) FROM test2base.storage_position WHERE (storage_position.id_specimen = '"+id_spec+"') ) <> 0)");
                    "AND (storage_position.position <> 0)");
                if (str.Count > 0)
                {
                    ans.Add(int.Parse(str[0]));
                }
                else
                {
                    ans.Add(0);
                }
            }
            return ans;
        }
        /// <summary>
        /// добавляем к таблице столбцы с расчетами
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private DataTable Add_colomns(DataTable dt)
        {
            var n_dt = new DataTable();
            var state = new DataColumn("Material");
            var treat = new DataColumn("Treatment");
            n_dt.Columns.Add(state);
            n_dt.Columns.Add(treat);
            DataColumn n_column = new DataColumn("Specimens count");
            n_dt.Columns.Add(n_column);
            DataColumn n_suc = new DataColumn("Success count");
            n_dt.Columns.Add(n_suc);
            DataColumn at_col = new DataColumn("Atoms count");                        
            n_dt.Columns.Add(at_col);            
            DataColumn bools = new DataColumn("Status");            
            n_dt.Columns.Add(bools);

            n_dt.Columns.Add("Specs in queue");
            //datagridview
            //формируем первый доп. столбец (общее количество)
            List<string> col = Form_list_forGrid(dt, "SELECT count(*) " +
                    "FROM test2base.specimens " +
                    "LEFT OUTER JOIN test2base.materials ON test2base.specimens.id_material = test2base.materials.id_material " +
                    "LEFT OUTER JOIN test2base.treatment ON specimens.id_treatment = treatment.id_treatment ","");
            //формируем второй доп.столбец (успешных)
            List<string> suc = Form_list_forGrid(dt, "SELECT count(*) " +
                    "FROM test2base.specimens " +
                    "LEFT OUTER JOIN test2base.materials ON test2base.specimens.id_material = test2base.materials.id_material " +
                    "LEFT OUTER JOIN test2base.treatment ON specimens.id_treatment = treatment.id_treatment " +
                    "LEFT OUTER JOIN test2base.researches ON specimens.idspecimens = researches.id_specimen ", "AND (researches.success = '+')");
            //формируем третий столбец (успешных/количество*100%) + цвет?
            //формируем столбец (количество атомов)
            //List<string> atoms_col = Count_atoms(dt);
            List<double> atoms_col = Count_atoms(dt);
            //столбец статуса
            List<string> status = Get_statuses(dt);
            //столбец образцов в очереди
            // условия: это состояние, только Ready for APT
            List<int> specs_queue = Specs_in_queue(dt);

            //пишем всё это в таблицу
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                n_dt.Rows.Add(new object[] {dt.Rows[i].ItemArray[0], dt.Rows[i].ItemArray[1], col[i], suc[i], atoms_col[i].ToString("### ### ###"), status[i], specs_queue[i].ToString("###")});
            }
            return n_dt;
        }
        private DataTable GetAPTdata(string selectcommand)
        {
            DataSet ds = new DataSet();
            using (MySqlConnection cc = new MySqlConnection(conn_str))
            {
                try
                {
                    cc.Open();
                    MySqlDataAdapter adapter = new MySqlDataAdapter(selectcommand, New_connection(conn_str));
                    
                    adapter.Fill(ds);                    
                    cc.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("error in datagrid APT update\n"+ex.ToString());
                    
                }
            }
            //добавляем столбец с количеством исследований
            //
            ds.Tables.Add(Add_colomns(ds.Tables[0]));
            return ds.Tables[1];            
        }
        private void Fill_ch_box(CheckedListBox box)
        {
            box.Items.Clear();
            List<string> stages_list = SQL_str_request("SELECT stages.name FROM test2base.stages WHERE (id_project = "+id_project.ToString()+")");
            foreach (string str in stages_list)
            {
                box.Items.Add(str, true);
                //filters_APT.Add(" AND (stages.name <> '"+str+"')");
            }
        }
        private void Fill_combo(ComboBox box, string request)
        {
            box.Items.Clear();
            List<string> stages = SQL_str_request(request);
            foreach(string str in stages)
            {
                box.Items.Add(str);
            }
        }
        private void Frm_projects_Load(object sender, EventArgs e)
        {
            on_load = true;
            //splitters
            splitContainer_information.SplitterDistance = Properties.Settings.Default.project_info_split;
            splitContainer_infor_2.SplitterDistance = Properties.Settings.Default.project_split_graphs;
            lbl_progressbar_left.Text = "Now\n(remain specimens)";
            lbl_progressbar_end.Text = "End of project\n(remain days in project)";
            //грузим дефотные настройки в вкладку info
            Refresh_project_maintab();            
            //списоки в combox            
            Fill_combo(combox_projects, "SELECT Projects.name FROM projects");
            Fill_combo(combox_pr_new_resp, "SELECT producers.surname FROM test2base.producers");

            //lbl_cr_new_stage.Text = "Create new stage\n ('start date' and 'end date' would be taken from fields above )";

            //грузим данные АЗТ о проекте
            /*dataGridView1_APT_data.DataSource = GetAPTdata("SELECT distinct materials.name, treatment.name " +
                "FROM test2base.specimens " +
                "LEFT OUTER JOIN test2base.materials ON specimens.id_material = materials.id_material " +
                "LEFT OUTER JOIN test2base.treatment ON specimens.id_treatment = treatment.id_treatment " +
                "WHERE(id_project = "+id_project.ToString()+") " +
                "ORDER BY materials.name;");
            Fill_color_DG();*/
            Refresh_DG();
            //диаграммы
            ZedGraph.GraphPane pane_specs_count = zg_spec_count.GraphPane;
            pane_specs_count.Title.Text = "test";
            pane_specs_count.CurveList.Clear();
            //pane_specs_count.BarSettings.Base = ZedGraph.BarBase.X;
            //pane_specs_count.BarSettings.Type = ZedGraph.BarType.Stack;
            //pane_specs_count.XAxis.Type=ZedGraph.AxisType.
            //pane_specs_count.XAxis.Scale.Min = 
            Refresh_diagrams();

            on_load = false;
        }
        /// <summary>
        /// сохраняем все настройки
        /// </summary>
        private void Save_settings_projects()
        {
            Properties.Settings.Default.info_id_project = id_project;
            Properties.Settings.Default.project_info_split = splitContainer_information.SplitterDistance;
            Properties.Settings.Default.project_split_graphs = splitContainer_infor_2.SplitterDistance;
            Properties.Settings.Default.Save();
        }

        private void Frm_projects_FormClosing(object sender, FormClosingEventArgs e)
        {
            Save_settings_projects();
            //Просто прячем форму на период закрытия
            e.Cancel = true;
            Hide();
        }

        private void dataGridView1_APT_data_SelectionChanged(object sender, EventArgs e)
        {
            //записываем состяние
            if (!on_load && dataGridView1_APT_data.SelectedRows!=null  && dataGridView1_APT_data.SelectedRows.Count>0 && dataGridView1_APT_data.SelectedRows[0].Cells != null)
            {
                on_load = true;
                //MessageBox.Show(dataGridView1_APT_data.SelectedRows[0].ToString());
                List<string> ans = SQL_str_request("SELECT distinct stages.name " +
                    "FROM test2base.stages_specimens " +
                    "LEFT OUTER JOIN test2base.specimens ON specimens.idspecimens = stages_specimens.id_specimen " +
                    "LEFT OUTER JOIN test2base.stages ON stages.id_stage = stages_specimens.id_stage " +
                    "LEFT OUTER JOIN test2base.materials ON materials.id_material = specimens.id_material " +
                    "LEFT OUTER JOIN test2base.treatment ON treatment.id_treatment = specimens.id_treatment " +
                    "WHERE (specimens.id_project = " + id_project.ToString() + ") " +
                    "AND (materials.name = '" + dataGridView1_APT_data.SelectedRows[0].Cells[0].Value.ToString() + "')  " +
                    "AND (treatment.name = '" + dataGridView1_APT_data.SelectedRows[0].Cells[1].Value.ToString() + "');");
                if (ans.Count > 0)
                {
                    txtbox_sel_state_stage.Text = ans[0];
                }
                else
                {
                    txtbox_sel_state_stage.Text = "";
                }
                on_load = false;
            }
            combox_stages.Text = "";
        }
        /// <summary>
        /// команда SQL без ответа
        /// </summary>
        /// <param name="sql_req"></param>
        private void SQL_com(string sql_req)
        {
            using (MySqlConnection conn = New_connection(conn_str))
            {
                conn.Open();
                using (MySqlCommand comand = new MySqlCommand(sql_req, conn))
                {
                    try
                    {
                        comand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString() + " :\n " + sql_req, "Ошибка в SQL_com в запросе", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                    }
                }
                conn.Close();
            }
        }
        private void btn_ch_stage_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.user_access_lvl==1 && combox_stages.Text!=txtbox_sel_state_stage.Text && combox_stages.Text!="")
            {
                //получить все ИД образцов данного состояния - список
                List<string> id_specs = SQL_str_request("SELECT specimens.idspecimens " +
                    "FROM test2base.specimens " +
                    //"LEFT OUTER JOIN test2base.specimens ON specimens.idspecimens = stages_specimens.id_specimen " +                   
                    "LEFT OUTER JOIN test2base.materials ON materials.id_material = specimens.id_material " +
                    "LEFT OUTER JOIN test2base.treatment ON treatment.id_treatment = specimens.id_treatment " +
                    "WHERE (specimens.id_project = " + id_project.ToString() + ") " +
                    "AND (materials.name = '" + dataGridView1_APT_data.SelectedRows[0].Cells[0].Value.ToString() + "')  " +
                    "AND (treatment.name = '" + dataGridView1_APT_data.SelectedRows[0].Cells[1].Value.ToString() + "');");
                //получить новый этап (комбобокс) его ИД
                string new_stage_id = SQL_str_request("SELECT id_stage FROM test2base.stages WHERE (stages.name = '"+ combox_stages.Text + "')")[0] ;                
                //для каждого образца проверяем есть уже запись
                foreach (string id in id_specs)
                {
                    List<string> specs_with_stages = new List<string>();
                    specs_with_stages.Clear();
                    specs_with_stages = SQL_str_request("SELECT * FROM test2base.stages_specimens WHERE (id_specimen = " + id + ")");
                    if (specs_with_stages.Count==0)
                    {
                        //нет записи об образцах - делаем новую
                        SQL_com("INSERT INTO test2base.stages_specimens (`id_stage`, `id_specimen`) VALUES ('"+ new_stage_id + "', '"+ id  +"')");
                    }
                    else
                    {
                        //есть запись, делаем апдейт
                        SQL_com("UPDATE test2base.stages_specimens SET id_stage = '"+ new_stage_id + "' WHERE(id_specimen = "+ id +")");
                    }
                }
            }
            else
            {
                MessageBox.Show("access denied\n" +
                    "or old stage = new stage\n" +
                    "or no new stage");
            }
        }
        private void Filt(ItemCheckEventArgs e, CheckedListBox obje, string name)
        {
            if (!on_load)
            {
                if (e.NewValue == CheckState.Checked)
                {
                    //удаляем из фильтров                    
                    //filt_master.common_filters.Remove(str_f + obje.Items[e.Index].ToString() + "'");
                    //MessageBox.Show("Добавлен этап "+ obje.Items[e.Index].ToString());
                    //filters_APT.Add(name +obje.Items[e.Index].ToString()+ "') ");
                    filters_APT.Remove(name + obje.Items[e.Index].ToString() + "') ");
                }
                else
                {
                    //добавляем к фильтрам                   
                    //filt_master.common_filters.Add(str_f + obje.Items[e.Index].ToString() + "'");
                    //MessageBox.Show("Удален этап " + obje.Items[e.Index].ToString());
                    //filters_APT.Remove(name + obje.Items[e.Index].ToString() + "') ");
                    filters_APT.Add(name + obje.Items[e.Index].ToString() + "') ");
                }
            }
        }

        private void ch_listbox_project_stage_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            //фильтр по этапам проекта
            Filt(e, ch_listbox_project_stage, " AND (stages.name <> '");
            //MessageBox.Show("Filters=\n"+ Filt_tostring(filters_APT));
            Refresh_DG();
        }
        private string Filt_tostring(List<string> filt)
        {
            string ans = "";
            foreach (string str in filt)
            {
                ans += str;
            }
            return ans;
        }
        private void Fill_color_DG()
        {
            for (int i = 0; i < dataGridView1_APT_data.Rows.Count; i++)
            {
                string str = dataGridView1_APT_data.Rows[i].Cells[5].Value.ToString();
                switch (str)
                {
                    case "Need one more":
                        dataGridView1_APT_data.Rows[i].DefaultCellStyle.BackColor = Color.LightPink;
                        break;
                    case "Data processing":
                        dataGridView1_APT_data.Rows[i].DefaultCellStyle.BackColor = Color.LightYellow;
                        break;
                    case "State finished":
                        dataGridView1_APT_data.Rows[i].DefaultCellStyle.BackColor = Color.LightGreen;
                        break;
                }
            }
        }
        private void Refresh_DG()
        {
            if (dataGridView1_APT_data.SelectedRows.Count > 0)
            {
                selected_id = dataGridView1_APT_data.SelectedRows[0].Index;
            }
            //curr_cel = dataGridView1_APT_data.SelectedColumns[0].Index;
            dataGridView1_APT_data.DataSource = null;
            dt_grid = GetAPTdata("SELECT distinct materials.name, treatment.name " +
                "FROM test2base.specimens " +
                "LEFT OUTER JOIN test2base.materials ON specimens.id_material = materials.id_material " +
                "LEFT OUTER JOIN test2base.treatment ON specimens.id_treatment = treatment.id_treatment " +
                "LEFT OUTER JOIN test2base.stages_specimens ON specimens.idspecimens = stages_specimens.id_specimen " +
                "LEFT OUTER JOIN test2base.stages ON stages_specimens.id_stage = stages.id_stage " +
                "WHERE(specimens.id_project = " + id_project.ToString() + ") " +
                Filt_tostring(filters_APT) +
                " ORDER BY materials.name;");
            dataGridView1_APT_data.DataSource = dt_grid;
            if (dataGridView1_APT_data.SelectedRows.Count > 0 && selected_id<=dataGridView1_APT_data.Rows.Count-1)
            {
                dataGridView1_APT_data.Rows[selected_id].Selected = true;
                dataGridView1_APT_data.Rows[selected_id].Cells[5].Selected = true;
            }
            else if (dataGridView1_APT_data.SelectedRows.Count > 0)
            {
                dataGridView1_APT_data.Rows[0].Selected = true;
                dataGridView1_APT_data.Rows[0].Cells[5].Selected = true;
            }
            //раскраска
            Fill_color_DG();
            //апдейт инфорации об образцах на следующей вкладке
            Specs_update("", "", id_project);
        }
        private void Specs_update(string treatment, string material, int project_id)
        {

        }
        private double[] Get_massive(int col, double Y)
        {
            List<double> ans = new List<double>();
            
            for (int i = 0; i <col;i++)
            {
                ans.Add(0);
            }
            ans.Add(Y);
            return ans.ToArray();
        }
        private Color Get_color(bool horizonatal)
        {            
           
            List<Color> col = new List<Color> { Color.Blue, Color.Green, Color.Red, Color.Black, Color.Orange, Color.PaleGreen, Color.Silver, Color.Aqua, Color.Yellow, Color.MediumPurple};
            Color ans = col[rand.Next(col.Count)];
            return ans;
        }
        private void Draw_simple_chart(ZedGraph.ZedGraphControl chart, string title, string[] X_title, double[] Y_values, string X_string, string Y_string, bool is_horisontal)
        {
            int fontsize = 30;
            //ZedGraph.PointPairList list = new ZedGraph.PointPairList();
            chart.GraphPane.CurveList.Clear();
            chart.GraphPane.BarSettings.Type = ZedGraph.BarType.SortedOverlay;
            chart.GraphPane.XAxis.Title.Text = X_string;
            chart.GraphPane.YAxis.Title.Text = Y_string;
            chart.GraphPane.XAxis.Title.FontSpec.Size = fontsize;
            chart.GraphPane.YAxis.Title.FontSpec.Size = fontsize;
            chart.GraphPane.Title.FontSpec.Size = fontsize;
            //chart.GraphPane.Legend.Location.AlignH = ZedGraph.AlignH.Right;
            //chart.GraphPane.Legend.Location.AlignV = ZedGraph.AlignV.Bottom;
            chart.GraphPane.Legend.IsVisible = false;
            if (is_horisontal)
            {
                chart.GraphPane.BarSettings.Base = ZedGraph.BarBase.Y;
                chart.GraphPane.YAxis.Type = ZedGraph.AxisType.Text;
                chart.GraphPane.YAxis.Scale.TextLabels = X_title;
            }
            else
            {
                chart.GraphPane.XAxis.Type = ZedGraph.AxisType.Text;
                chart.GraphPane.XAxis.Scale.TextLabels = X_title;
            }    
            //var rand = new Random();
            for (int i =0; i< X_title.Length; i++)
            {
                ZedGraph.BarItem curve;
                if (is_horisontal)
                {
                    //curve = chart.GraphPane.AddBar(X_title[i], Get_massive(i, Y_values[i]), null, Get_color());
                    curve = chart.GraphPane.AddBar(X_title[i], Get_massive(0, Y_values[i]), null, Get_color(is_horisontal));
                }
                else
                {
                    curve = chart.GraphPane.AddBar(X_title[i], null, Get_massive(i, Y_values[i]), Get_color(is_horisontal));
                }
                curve.Bar.Fill.Type = ZedGraph.FillType.Solid;
            }
            chart.GraphPane.Title.Text = title;
            foreach (string str in X_title)
            {

                //chart.GraphPane.AddBar(str, null, Get_massive(), Color.Blue);

            }            
            //chart.GraphPane.AddBar("Done", null, new double[] {0, Y_values[1]}, Color.Green);
            //chart.GraphPane.AddBar("ToDo", null, new double[] {0,0, Y_values[2]}, Color.Red);

            
            chart.AxisChange();
            chart.Invalidate();
        }
        /// <summary>
        /// считает количество образцов, 2 - всего, 3 - успешных
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private double Count_specs(int pos)
        {
            double ans = 0;
            for (int i=0; i<dataGridView1_APT_data.Rows.Count;i++)
            {
                ans = ans + Convert.ToDouble(dataGridView1_APT_data.Rows[i].Cells[pos].Value);
            }
            return ans;
        }
        /// <summary>
        /// считает успешность образцов из таблички
        /// </summary>
        /// <returns></returns>
        private double[] State_suc_aquis()
        {
            List<double> ans = new List<double>();
            for (int i = 0; i < dataGridView1_APT_data.Rows.Count; i++)
            {
                ans.Add(Convert.ToDouble(dataGridView1_APT_data.Rows[i].Cells[3].Value)/ Convert.ToDouble(dataGridView1_APT_data.Rows[i].Cells[2].Value)*100);
            }
            return ans.ToArray();
        }
        private string[] Get_statenames()
        {
            List<string> ans = new List<string>();
            for (int i = 0; i < dataGridView1_APT_data.Rows.Count; i++)
            {
                ans.Add(dataGridView1_APT_data.Rows[i].Cells[0].Value.ToString() + dataGridView1_APT_data.Rows[i].Cells[1].Value.ToString());
            }
            return ans.ToArray();
        }
        /// <summary>
        /// получаем список имен этапов (тройное дублирование)
        /// </summary>
        /// <returns></returns>
        private string[] Get_stages_names()
        {
            List<string>  temp_ans = SQL_str_request("SELECT stages.name FROM test2base.stages WHERE (id_project = '"+ id_project.ToString() + "')");
            List<string> ans = new List<string>();
            for (int i = 0; i<temp_ans.Count; i++)
            {
                //ans.AddRange(new string [] {temp_ans[i],temp_ans[i],temp_ans[i] });
                ans.Add(temp_ans[i]);
            }
            return ans.ToArray();
        }
        /// <summary>
        /// [0] - start
        /// [1] - end
        /// </summary>
        /// <param name="sql_reqest"></param>
        /// <returns></returns>
        private double Get_stages_end_or_start( string sql_reqest)
        {                   
            string date = SQL_str_request(sql_reqest)[0];
            double ans = Convert.ToDouble(Convert.ToDateTime(date).Ticks);
            return ans;
        }
        private void Add_bars(ZedGraph.GraphPane pane)
        {
            // Y_max|X-title_text|Y-min
            // end_date|null|start date
            List<string> stages = SQL_str_request("SELECT stages.name FROM test2base.stages WHERE (id_project = '" + id_project.ToString() + "')");
            List<double> start_dates = new List<double> ();
            List<double> end_dates = new List<double>();
            for (int i=0;i<stages.Count; i++)
            {
                start_dates.Add(Get_stages_end_or_start("SELECT stages.start_date FROM test2base.stages " +
                    "WHERE (id_project = '" + id_project.ToString() + "') AND (name = '" + stages[i] + "')"));
                end_dates.Add(Get_stages_end_or_start("SELECT stages.end_date FROM test2base.stages " +
                    "WHERE (id_project = '" + id_project.ToString() + "') AND (name = '" + stages[i] + "')"));
            }

            pane.AddHiLowBar("", end_dates.ToArray(), null, start_dates.ToArray(), Color.Blue);
        }
        private void Draw_GANT_chart(ZedGraph.ZedGraphControl zg, string title,string X_title, string Y_title, string[] X_labels, int font)
        {
            int fontsize = font; //30
            zg.GraphPane.CurveList.Clear();
            zg.GraphPane.BarSettings.Type = ZedGraph.BarType.SortedOverlay;
            zg.GraphPane.XAxis.Title.Text = X_title;
            zg.GraphPane.YAxis.Title.Text = Y_title;
            zg.IsShowPointValues = true;
            zg.GraphPane.XAxis.Title.FontSpec.Size = fontsize;
            zg.GraphPane.YAxis.Title.FontSpec.Size = fontsize;
            zg.GraphPane.Title.FontSpec.Size = fontsize;
            zg.GraphPane.Legend.IsVisible = false;
            zg.GraphPane.BarSettings.Base = ZedGraph.BarBase.Y;
            zg.GraphPane.YAxis.Type = ZedGraph.AxisType.Text;
            zg.GraphPane.YAxis.Scale.TextLabels = X_labels;
            Add_bars(zg.GraphPane);            
            zg.GraphPane.Title.Text = title;
            zg.AxisChange();
            zg.Invalidate();
        }
        private void Refresh_diagrams()
        {
            //chart_spec_count.Series.Clear();
            //ZedGraph.PointPairList list_count = new ZedGraph.PointPairList();
            //list_count.Add("All",35);
            
            if (Double.TryParse(txtbox_spec_per_state.Text, out double spec_per_state))
            {
                double specs_all = Count_specs(2);
                double specs_suc = Count_specs(3);
                double specs_all_prog = dataGridView1_APT_data.Rows.Count * spec_per_state;
                TimeSpan days = dateTimePicker_pr_info_end.Value - DateTime.Now;
                double ddd = days.TotalDays;
                //MessageBox.Show("days = "+ddd.ToString()+"\ntimespan = "+days.ToString());
                Draw_simple_chart(zg_spec_count, "Project completion", new string[] { "All", "Done", "ToDo" }, new double[] { specs_all_prog, specs_suc, specs_all_prog-specs_suc },"","Specs number", false);
                Draw_simple_chart(zed_state_succ, "State successfullness", Get_statenames(), State_suc_aquis(), "", "Percentage",false);
                Draw_GANT_chart(zg_gantt, "GAAAANT!","Time","Stages",Get_stages_names(),30);
                //Draw_simple_chart(zed_progress, "Progress", new string[] { "Reamin" }, new double[] { (specs_all_prog - specs_suc), ddd }, "Specimens", "",true);
                if (specs_all_prog - specs_suc >= 0 && ddd>0)
                {
                    prog_bar_remain.Maximum = Convert.ToInt32(Math.Round(ddd));
                    prog_bar_remain.Minimum = 0;
                    prog_bar_remain.Value = Convert.ToInt32(specs_all_prog - specs_suc);
                    prog_bar_remain.BackColor = Color.PaleGreen;
                    lbl_progressbar_end.BackColor = Color.Transparent;
                    lbl_progressbar_end.Text = "End of project\n(remain days in project = " + Convert.ToInt16(ddd).ToString() + " )";
                }
                else
                {
                    prog_bar_remain.Minimum = 0;
                    prog_bar_remain.Maximum = 100;
                    prog_bar_remain.Value = 100;
                    if (specs_all_prog - specs_suc <= 0)
                    {
                        prog_bar_remain.BackColor = Color.PaleGreen;
                        lbl_progressbar_end.BackColor = Color.PaleGreen;
                        lbl_progressbar_end.Text = "End of project\n(remain days in project = " + Convert.ToInt16(ddd).ToString() + " )";
                    }
                    else
                    {
                        prog_bar_remain.BackColor = Color.Red;
                        lbl_progressbar_end.BackColor = Color.Red;
                        lbl_progressbar_end.Text = "End of project\n(remain days in project = " + Convert.ToInt16(ddd).ToString() + " )\n ALERT!!! shit happens";
                    }
                }                
                lbl_progressbar_left.Text = "Now\n(remain specimens = " + (specs_all_prog - specs_suc).ToString() + " )";
            }
            else
            {
                MessageBox.Show("no specimens per state in project info");
            }

        }
        private void Refresh_all()
        {
            //получить ИД
            //обновить интерфейс
            Refresh_project_maintab();
            Refresh_DG();
            //рефреш диаграмм
            Refresh_diagrams();
        }
        private void btn_refresh_all_Click(object sender, EventArgs e)
        {
            Refresh_DG();
        }

        private void combox_projects_SelectedIndexChanged(object sender, EventArgs e)
        {
            selected_id = 0;
            filters_APT.Clear();
            List<string> new_pr_id = SQL_str_request("SELECT id_project FROM test2base.projects WHERE (name = '"+combox_projects.Text+"')");
            if (new_pr_id.Count > 0)
            {
                Properties.Settings.Default.info_id_project = Convert.ToInt32(new_pr_id[0]);
                Properties.Settings.Default.Save();                
            }
            Refresh_all();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (txtbox_new_stage.Text != "" && Properties.Settings.Default.user_access_lvl == 1)
            {
                DialogResult res = MessageBox.Show("Create new stage for active project?", "Quest", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes)
                {
                    if (DateTime.Compare(dateTimePicker_pr_new_start.Value, dateTimePicker_pr_new_end.Value) > 0)
                    {
                        //неправильно указано время
                        MessageBox.Show("bulshit (start date) after (end date)");
                    }
                    else
                    {
                        SQL_com("INSERT INTO test2base.stages (id_project, name, start_date, end_date) " +
                            "VALUES ('"+id_project.ToString()+"', '"+txtbox_new_stage.Text+"', '"+dateTimePicker_pr_new_start.Value.ToString("yyyy-MM-dd HH:mm:ss") + "', '"+dateTimePicker_pr_new_end.Value.ToString("yyyy-MM-dd HH:mm:ss") + "');");
                    }
                }
            }
            else
            {
                MessageBox.Show("no new stage name");
            }
        }

        private void btn_ch_end_date_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.user_access_lvl == 1)
            {
                if (txtbox_start_date.Text == "")
                {
                    SQL_com("UPDATE test2base.projects SET end_date = '" + dateTimePicker_pr_info_end.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE (id_project = '" + id_project.ToString() + "');");
                }
                else if (DateTime.Compare(Convert.ToDateTime(txtbox_start_date.Text), dateTimePicker_pr_info_end.Value) < 0)
                {
                    SQL_com("UPDATE test2base.projects SET end_date = '" + dateTimePicker_pr_info_end.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE (id_project = '" + id_project.ToString() + "');");
                }
                else
                {
                    MessageBox.Show("wrong date");
                }
            }
        }

        private void btn_ch_priority_Click(object sender, EventArgs e)
        {
            if (txtbox_priority.Text!="")
            {
                if (Int32.TryParse(txtbox_priority.Text, out int res))
                {
                    SQL_com("UPDATE test2base.projects SET priority = '" + res.ToString() + "' WHERE (id_project = '" + id_project.ToString() + "');");
                }
                else
                {
                    MessageBox.Show("could not covert YOUR priority");
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (txtbox_spec_per_state.Text != "")
            {
                if (Int32.TryParse(txtbox_spec_per_state.Text, out int res))
                {
                    SQL_com("UPDATE test2base.projects SET specs_per_state = '" + res.ToString() + "' WHERE (id_project = '" + id_project.ToString() + "');");
                }
                else
                {
                    MessageBox.Show("could not covert YOUR specs per state number");
                }
            }
        }

        private void dataGridView1_APT_data_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //MessageBox.Show("test CellValueChanged");
            //срабатывает, даже если был откат изменения
        }
        private bool Paint_Cell(int col, int row)
        {
            bool ans = false;
            int[] rows = { 6, 7, 8};
            int[] cols = { 0, 1, 2, 3, 4 };
            if (cols.Contains(col) && rows.Contains(row))
            {
                ans = true;
            }
            return ans;
        }
        private void tableLayoutPanel_project_info_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            //раскраска клеток
            if (first_draw)
            {
                if (Paint_Cell(e.Column, e.Row))
                {
                    e.Graphics.FillRectangle(Brushes.MediumAquamarine, e.CellBounds);
                    //e.
                    //tableLayoutPanel_project_info.
                }
                else
                {
                    e.Graphics.FillRectangle(Brushes.LightSkyBlue, e.CellBounds);
                }
                //first_draw = false;
            }
        }

        private void dataGridView1_APT_data_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            //MessageBox.Show("test CellEndEdit\n row = "+e.RowIndex.ToString()+", col = "+e.ColumnIndex.ToString()+"\n new val = "+ dataGridView1_APT_data.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
            //нужно учитывать закрытие формы и потерю фокуса
        }

        private void dataGridView1_APT_data_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //MessageBox.Show("test CellContentClick\n row = " + e.RowIndex.ToString() + ", col = " + e.ColumnIndex.ToString());
            //то, что нужно, пишет значение то ,что было до изменения
            if (e.ColumnIndex==5)
            {
                //MessageBox.Show("state = " + dataGridView1_APT_data.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
                //
                
            }
        }

        private void btn_new_stage_del_Click(object sender, EventArgs e)
        {
            //удаление этапа
            MessageBox.Show("не поддерживается, ввиду малой заинтересованности в разработке");
        }

        private void dataGridView1_APT_data_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //меняем циклически состояния
            if (e.ColumnIndex == 5)
            {
                if (dataGridView1_APT_data.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString()!="")
                {
                    //циклически меняем
                    //4 - need one more
                    //5 - data processing
                    //6 - state fifished
                    if (Int32.TryParse(SQL_str_request("SELECT state.id_state FROM test2base.state " +
                        "WHERE (state.name = '"+dataGridView1_APT_data.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString()+"')")[0], out int old_id))
                    {
                        if (old_id == 6)
                        {
                            old_id = 4;
                        }
                        else
                        {
                            old_id = old_id+1;
                        }
                        try
                        {
                            SQL_com("UPDATE test2base.materialstate_state SET id_state = (" + old_id.ToString() + ") " +
                                "WHERE (id_materialstate = (SELECT materialstate.id_materialstate FROM test2base.materialstate " +
                                "WHERE (materialstate.id_treatment = (SELECT treatment.id_treatment FROM test2base.treatment WHERE treatment.name = '" + dataGridView1_APT_data.Rows[e.RowIndex].Cells[1].Value.ToString() + "')) AND " +
                                "(materialstate.id_material = (SELECT materials.id_material FROM test2base.materials WHERE materials.name = '" + dataGridView1_APT_data.Rows[e.RowIndex].Cells[0].Value.ToString() + "')) ))");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("error in doubleclick CELL\n "+ex.ToString());
                        }
                        finally
                        {
                            Refresh_DG();
                        }
                    }
                    
                    
                }
            }
        }

        private void btn_temp_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1_APT_data.Rows)
            {
                /*
                SQL_com("INSERT INTO test2base.materialstate_state (id_materialstate, id_state) " +
                    "VALUES ((SELECT materialstate.id_materialstate FROM test2base.materialstate " +
                    "WHERE (materialstate.id_treatment = (SELECT treatment.id_treatment FROM test2base.treatment WHERE treatment.name = '" + row.Cells[1].Value.ToString() + "')) AND " +
                    "(materialstate.id_material = (SELECT materials.id_material FROM test2base.materials WHERE materials.name = '" + row.Cells[0].Value.ToString() + "'))), '4')");
                */
                /*SQL_com("INSERT INTO test2base.materialstate (id_treatment, name,  id_material) " +
                    "VALUES((SELECT treatment.id_treatment FROM test2base.treatment WHERE(treatment.name = '"+row.Cells[1].Value.ToString()+"')), " +
                    "'"+ row.Cells[0].Value.ToString() + " "+row.Cells[1].Value.ToString() + "',  (SELECT materials.id_material FROM test2base.materials WHERE(materials.name = '"+ row.Cells[0].Value.ToString() + "')) ); ");
                */                
            }
            //MessageBox.Show("test");
        }

        private void listbox_stages_SelectedIndexChanged(object sender, EventArgs e)
        {
            //вывести даты начала и конца этапов
            try
            {
                if (!on_load)
                {
                    //string str = SQL_str_request("SELECT stages.start_date FROM test2base.stages " +
                        //"WHERE((id_project = '" + id_project.ToString() + "') AND (stages.name = '" + listbox_stages.Items[listbox_stages.SelectedIndex] + "'))");
                    dateTimePicker_pr_new_start.Value = Convert.ToDateTime(SQL_str_request("SELECT stages.start_date FROM test2base.stages " +
                        "WHERE ((id_project = '" + id_project.ToString() + "') AND (stages.name = '" + listbox_stages.Items[listbox_stages.SelectedIndex] + "'))")[0]);
                    dateTimePicker_pr_new_end.Value = Convert.ToDateTime(SQL_str_request("SELECT stages.end_date FROM test2base.stages " +
                        "WHERE ((id_project = '" + id_project.ToString() + "') AND (stages.name = '" + listbox_stages.Items[listbox_stages.SelectedIndex] + "'))")[0]);
                }
            
            }
            catch (Exception ex)
            {
                MessageBox.Show("error in date-time converter\n"+ex.ToString());
            }
        }

        private void btn_ch_dates_Click(object sender, EventArgs e)
        {
            //изменение дат этапа проекта
            if ((Properties.Settings.Default.user_access_lvl == 1) && (listbox_stages.SelectedIndex>=0) && (listbox_stages.Items[listbox_stages.SelectedIndex].ToString()!=""))
            {
                SQL_com("UPDATE test2base.stages " +
                    "SET start_date = '"+dateTimePicker_pr_new_start.Value.ToString("yyyy-MM-dd HH:mm:ss") + "', end_date = '" + dateTimePicker_pr_new_end.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                    "WHERE ((id_project = '" + id_project.ToString() + "') AND (stages.name = '" + listbox_stages.Items[listbox_stages.SelectedIndex] + "'))");
            }
            else
            {
                MessageBox.Show("no stages or no selected stages");
            }
        }

        private void tableLayoutPanel_project_info_Paint(object sender, PaintEventArgs e)
        {
            //срабатывает при любой перерисовке
            //MessageBox.Show("test tableLayoutPanel_project_info_Paint");
        }

        private void zedGraph_MouseClick (object sender, MouseEventArgs e)
        {
            /*double x, y;

            // Пересчитываем пиксели в координаты на графике
            // У ZedGraph есть несколько перегруженных методов ReverseTransform.
            zg_gantt.GraphPane.ReverseTransform(e.Location, out x, out y);

            // Выводим результат
            string text = string.Format("X: {0};    Y: {1}", x, y);
            coordLabel.Text = text;*/
        }

        private string zg_gantt_PointValueEvent(ZedGraph.ZedGraphControl sender, ZedGraph.GraphPane pane, ZedGraph.CurveItem curve, int iPt)
        {
            ZedGraph.PointPair pt = curve[iPt];

            string res = new DateTime(Convert.ToInt64(pt.X)).ToString();
            return res;
        }

        private void txtbox_new_stage_Click(object sender, EventArgs e)
        {
            txtbox_new_stage.ForeColor = Color.Black;
            txtbox_new_stage.Text = "";
        }

        private void txtbox_new_stage_Leave(object sender, EventArgs e)
        {
            txtbox_new_stage.ForeColor = Color.LightGray;
            txtbox_new_stage.Text = "name of new stage";
        }
        /// <summary>
        /// проверка полей для создания проекта
        /// </summary>
        /// <returns></returns>
        private bool Check_fields_project()
        {
            bool ans=false;
            if (txtbox_pr_new_name.Text!=""
                && combox_pr_new_resp.Text!=""
                && dateTimePicker_pr_new_start.Value< dateTimePicker_pr_new_end.Value)
            {
                ans = true;
            }

            return ans;
        }
        /// <summary>
        /// проверка, есть ли уже такой проект
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool Check_new_pr_name(string name)
        {
            bool ans = false;
            if (SQL_str_request("SELECT name FROM test2base.projects WHERE (projects.name = '" + name + "')").Count == 0)
            {
                ans = true;
            }            
            return ans;
        }
        /// <summary>
        /// создание новго проекта
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_create_new_project_Click(object sender, EventArgs e)
        {
            //проверка доступа
            //проверка полей
            //есть ли уже с таким названием
            if (Properties.Settings.Default.user_access_lvl==1 
                && Check_fields_project()
                && Check_new_pr_name(txtbox_pr_new_name.Text))
            {
                //создаем
                MessageBox.Show("create");
                int priority = 0;
                if (int.TryParse(txtbox_priority_add.Text, out int prior)) priority = prior;
                SQL_com("INSERT INTO test2base.projects (name, id_respons, priority, contract, start_date, end_date, stage_count, specs_per_state) " +
                    "VALUES ('"+ txtbox_pr_new_name.Text + "', " +
                    "(SELECT id_producer FROM test2base.producers WHERE (surname = '" + combox_pr_new_resp .Text + "')), " +
                    "'"+ priority.ToString() + "', " +
                    "'"+ txtbox_pr_new_contract.Text + "', " +
                    " '"+ dateTimePicker_pr_new_start.Value.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                    " '" + dateTimePicker_pr_new_end.Value.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                    " '1', '" + txtbox_stages_num_add.Text + "');");
            }
            else
            {
                MessageBox.Show("Увага, забаронена*!\nAccess level = 1\nNew name of Project\nStart_date<=End_date\n* - Это белорусский");                
            }

        }

        private void combox_pr_new_resp_KeyUp(object sender, KeyEventArgs e)
        {
            //анти сови-продюсер
            combox_pr_new_resp.Text = "";
        }
    }
}
