﻿using System;
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
using System.Diagnostics;
using Microsoft.Office.Interop.Word;
using ZedGraph;
using Microsoft.Office.Interop.Excel;

namespace RSB
{
    /// <summary>
    /// форма проекта
    /// </summary>
    public partial class Frm_projects : Form
    {
        private readonly RSBMainForm Parent_form;
        //private readonly Form_specimens _spec_form;
        private static string conn_str="";
        private bool on_load = false;
        private static int id_project = -1;
        private List<string> filters_APT = new List<string>();
        private System.Data.DataTable dt_grid = new System.Data.DataTable();
        private static int selected_id = 0;
        private static bool first_draw = true;
        private static readonly Random rand = new Random();
        private static bool is_FilterCheked = true;
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
        /// простой запрос, на ответ List_string_
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
            if (ans.Count == 0) ans.Add("");
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
            richtxtbox_contacts_info.Text = SQL_str_request("SELECT contacts FROM test2base.projects WHERE (id_project = " + id_project.ToString() + ")")[0];
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
        private List<string> Form_list_forGrid(System.Data.DataTable dt, string querry_part1, string querry_part2)
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
                    //MessageBox.Show($"путь {one_path}");
                    //MessageBox.Show("Error in calc atoms count\n"+ex.ToString());
                }
            }
            return ans;
        }
        /// <summary>
        /// считаем число атомов в состоянии
        /// </summary>
        /// <param name="dt">таблица данных состояний со столбцами _материал, _обработка</param>
        /// <returns></returns>
        private List<double> Count_atoms(System.Data.DataTable dt)
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
        /// расчет числа атомов для состояния
        /// </summary>
        /// <param name="material"></param>
        /// <param name="treatment"></param>
        /// <returns></returns>
        private List<double> Count_atoms(string material, string treatment)
        {
            List<double> ans = new List<double>();
            List<string> data_dirs = SQL_str_request("SELECT researches.data_dir " +
                   "FROM test2base.specimens " +
                   "LEFT OUTER JOIN test2base.materials ON test2base.specimens.id_material = test2base.materials.id_material " +
                   "LEFT OUTER JOIN test2base.treatment ON specimens.id_treatment = treatment.id_treatment " +
                   "LEFT OUTER JOIN test2base.researches ON specimens.idspecimens = researches.id_specimen " +
                   "WHERE (specimens.id_project =  " + id_project.ToString() + ") " +
                   "AND (materials.name = '" + material + "') " +
                   //"AND (treatment.name = '" + treatment + "') " +
                   treatment +
                   "AND (researches.success = '+')");
            foreach (string direc in data_dirs)
            {
                List<string> data_ = new List<string> { direc };
                ans.Add(Math.Round(calc_atoms(data_)));
            }                                       
            return ans;
        }
        /// <summary>
        /// Получаем статус состояния (нужно ещё одно/в процессе обработки данных/готово)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private List<string> Get_statuses(System.Data.DataTable dt)
        {
            List<string> ans = new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                List<string> str = SQL_str_request("select state.name FROM test2base.materialstate_state " +
                    "LEFT OUTER JOIN test2base.state ON materialstate_state.id_state = state.id_state " +
                    "LEFT OUTER JOIN test2base.materialstate ON materialstate_state.id_materialstate = materialstate.id_materialstate " +
                    "LEFT OUTER JOIN test2base.treatment ON materialstate.id_treatment = treatment.id_treatment " +
                    "LEFT OUTER JOIN test2base.materials ON materialstate.id_material = materials.id_material " +
                    "WHERE (materials.name = '"+ dt.Rows[i].ItemArray[0] + "')  AND (treatment.name = '"+ dt.Rows[i].ItemArray[1] + "'); ");
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
        private List<int> Specs_in_queue(System.Data.DataTable dt)
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
        private System.Data.DataTable Add_colomns(System.Data.DataTable dt)
        {
            var n_dt = new System.Data.DataTable();
            var state = new DataColumn("Material");
            var treat = new DataColumn("Treatment");
            n_dt.Columns.Add(state);
            n_dt.Columns.Add(treat);
            DataColumn n_column = new DataColumn("Specimens count", Type.GetType("System.Int32"));
            n_dt.Columns.Add(n_column);
            DataColumn n_suc = new DataColumn("Success count", Type.GetType("System.Int32"));
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
                    "LEFT OUTER JOIN test2base.treatment ON specimens.id_treatment = treatment.id_treatment "," AND (specimens.id_state <> '7')");
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
                n_dt.Rows.Add(new object[] {dt.Rows[i].ItemArray[0], dt.Rows[i].ItemArray[1], Convert.ToInt32(col[i]), Convert.ToInt32(suc[i]), atoms_col[i].ToString("### ### ###"), status[i], specs_queue[i].ToString("###")});
            }
            return n_dt;
        }
        /// <summary>
        /// получение таблицы по запросу select_table типа SELECT
        /// </summary>
        /// <param name="select_table"></param>
        /// <returns></returns>
        private System.Data.DataTable GetTableFromSQL(string select_table)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            using (MySqlConnection cc = new MySqlConnection(conn_str))
            {
                try
                {
                    cc.Open();
                    MySqlDataAdapter adapter = new MySqlDataAdapter(select_table, New_connection(conn_str));

                    adapter.Fill(dt);
                    cc.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("error in GetTableFromSQL\n" + ex.ToString());

                }
            }
            return dt;
        }
        private System.Data.DataTable GetAPTdata(string selectcommand)
        {
            DataSet ds = new DataSet();
            //получаем талблицу с сырыми данными
            ds.Tables.Add(GetTableFromSQL(selectcommand));
            //добавляем столбец с количеством исследований и прочей статистикой
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
            if (splitContainer_information.SplitterDistance<=splitContainer_information.Width/2)
            {
                splitContainer_information.SplitterDistance = splitContainer_information.Width / 2;
            }
            splitContainer_infor_2.SplitterDistance = Properties.Settings.Default.project_split_graphs;
            if (splitContainer_infor_2.SplitterDistance<=splitContainer_infor_2.Height/5*4)
            {
                splitContainer_infor_2.SplitterDistance = splitContainer_infor_2.Height*4 / 5;
            }
            splitContainer_APT_table.SplitterDistance = Convert.ToInt32(this.Width * 0.60);
            lbl_progressbar_left.Text = "Now\n(remain specimens)";
            lbl_progressbar_end.Text = "End of project\n(remain days in project)";
            ch_list_box_filters.SetItemChecked(0, true);
            //грузим дефотные настройки в вкладку info
            Refresh_project_maintab();            
            //списоки в combox            
            Fill_combo(combox_projects, "SELECT Projects.name FROM projects ORDER BY name ASC");
            Fill_combo(combox_pr_new_resp, "SELECT producers.surname FROM test2base.producers ORDER BY surname ASC");
            //грузим данные АЗТ о проекте
            Refresh_DG();
            //диаграммы
            ZedGraph.GraphPane pane_specs_count = zg_spec_count.GraphPane;
            pane_specs_count.Title.Text = "test";
            pane_specs_count.CurveList.Clear();
            Refresh_diagrams();
            on_load = false;
        }
        /// <summary>
        /// проверяем - если логин=ответсвенному за проект
        /// </summary>
        /// <param name="project"></param>
        /// <param name="login"></param>
        /// <returns></returns>
        private bool Check_access_project(string login)
        {
            /*//true - если можно
            bool ans = false;
            string resp_login = SQL_str_request("SELECT surname FROM test2base.producers WHERE (user_name = '" + login + "')")[0];
            if (resp_login == response)
            {
                ans = true;
            }
            return ans;*/
            string resp_login = SQL_str_request("SELECT surname FROM test2base.producers WHERE (user_name = '" + login + "')")[0];
            List<string> id_resp = SQL_str_request("SELECT Id_responsible FROM test2base.responsible_project " +
                "WHERE (id_project = '"+id_project.ToString()+"');");
            foreach (string prod in id_resp)
            {
                string sname = SQL_str_request("SELECT surname FROM test2base.producers WHERE (id_producer = '" + prod + "');")[0];
                if (sname == resp_login)
                {
                    return true;
                }
            }
            return false;
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

            //test
            FillSpecInfo();
            //test
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
                //MessageBox.Show("ID 1 spec = "+id_specs[0]);
                //получить новый этап (комбобокс) его ИД
                string new_stage_id = SQL_str_request("SELECT id_stage FROM test2base.stages WHERE (stages.name = '"+ combox_stages.Text + "')")[0] ;
                //MessageBox.Show("new stage ID = " + new_stage_id);
                //для каждого образца проверяем есть уже запись
                foreach (string id in id_specs)
                {
                    List<string> specs_with_stages = new List<string>();
                    specs_with_stages.Clear();
                    specs_with_stages = SQL_str_request("SELECT * FROM test2base.stages_specimens WHERE (id_specimen = " + id + ")");
                    //MessageBox.Show("specs with new stage = " + specs_with_stages.Count.ToString());
                    if (specs_with_stages.Count==0 || specs_with_stages[0]=="")
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
                    filters_APT.Remove(name + obje.Items[e.Index].ToString() + "') ");
                }
                else
                {
                    //добавляем к фильтрам                   
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
        /// <summary>
        /// фильтры добавляются в строчку вида _AND (stages.name <> '+string+')_
        /// </summary>
        /// <param name="filt"></param>
        /// <returns></returns>
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
            //Specs_update("", "", id_project);
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
            if (stages[0] != "")
            {
                List<double> start_dates = new List<double>();
                List<double> end_dates = new List<double>();
                for (int i = 0; i < stages.Count; i++)
                {
                    start_dates.Add(Get_stages_end_or_start("SELECT stages.start_date FROM test2base.stages " +
                        "WHERE (id_project = '" + id_project.ToString() + "') AND (name = '" + stages[i] + "')"));
                    end_dates.Add(Get_stages_end_or_start("SELECT stages.end_date FROM test2base.stages " +
                        "WHERE (id_project = '" + id_project.ToString() + "') AND (name = '" + stages[i] + "')"));
                }

                pane.AddHiLowBar("", end_dates.ToArray(), null, start_dates.ToArray(), Color.Blue);
            }
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
                if (specs_all_prog - specs_suc >= 0 && ddd>0 && (specs_all_prog-specs_suc< Math.Round(ddd)))
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
            //комбобокс проектов
            Fill_combo(combox_projects, "SELECT Projects.name FROM projects ORDER BY name ASC");
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
            if (new_pr_id.Count > 0 && new_pr_id[0]!="")
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
            if (txtbox_new_stage.Text == "")
            {
                txtbox_new_stage.ForeColor = Color.LightGray;
                txtbox_new_stage.Text = "name of new stage";
            }
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
            List<string> ttt = SQL_str_request("SELECT name FROM test2base.projects WHERE (projects.name = '" + name + "')");
            if (ttt.Count()==1 && ttt[0] == "")
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
                int priority = 0;
                if (int.TryParse(txtbox_priority_add.Text, out int prior)) priority = prior;
                if (int.TryParse(txtbox_stages_num_add.Text, out int stages) &&
                    int.TryParse(txtbox_specs_state_add.Text, out int specs_per_state))
                {
                    SQL_com("INSERT INTO test2base.projects (name, id_respons, priority, contract, start_date, end_date, stage_count, specs_per_state, contacts) " +
                        "VALUES ('" + txtbox_pr_new_name.Text + "', " +
                        "(SELECT id_producer FROM test2base.producers WHERE (surname = '" + combox_pr_new_resp.Text + "')), " +
                        "'" + priority.ToString() + "', " +
                        "'" + txtbox_pr_new_contract.Text + "', " +
                        " '" + dateTimePicker_pr_new_start.Value.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                        " '" + dateTimePicker_pr_new_end.Value.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                        " '" + stages.ToString() + "', '" + specs_per_state.ToString() + "', '" + txtbox_contacts_add.Text + "');");
                    //получит ИД проекта
                    string NewProjectId = SQL_str_request("SELECT projects.id_project FROM test2base.projects WHERE (projects.name = '"+ txtbox_pr_new_name.Text + "')")[0];
                    //добавить ответственного
                    SQL_com("INSERT INTO test2base.responsible_project (id_project, Id_responsible) " +
                        "VALUES ('" + NewProjectId + "', (SELECT id_producer FROM test2base.producers WHERE (surname = '" + combox_pr_new_resp.Text + "')));");
                }
                else
                {
                    MessageBox.Show("Parsing proplems, check the numbers");
                }
            }
            else
            {
                MessageBox.Show("Увага, забаронена*!\nAccess level = 1\nNew name of Project\nStart_date<=End_date\n* - Это белорусский");                
            }
            //обновить всё
            Refresh_DG();
            //обновить список проектов
            Fill_combo(combox_projects, "SELECT Projects.name FROM projects ORDER BY name ASC");
        }

        private void combox_pr_new_resp_KeyUp(object sender, KeyEventArgs e)
        {
            //анти сови-продюсер
            combox_pr_new_resp.Text = "";
        }
        private string GetYesNoDataminig(string _ID, string _treat, string _mat, string _state)
        {
            string _MassRec = SQL_str_request("SELECT distinct researches.id_research " +
                " FROM test2base.researches " +
                " LEFT OUTER JOIN test2base.specimens ON researches.id_specimen = specimens.idspecimens " +
                " LEFT OUTER JOIN test2base.projects ON projects.id_project = specimens.id_project " +
                " LEFT OUTER JOIN test2base.materials ON materials.id_material = specimens.id_material " +
                " LEFT OUTER JOIN test2base.treatment ON treatment.id_treatment = specimens.id_treatment " +
                " LEFT OUTER JOIN test2base.dataminig ON dataminig.id_research = researches.id_research " +
                " LEFT OUTER JOIN test2base.state ON state.id_state = dataminig.id_status " +
                " WHERE (specimens.id_project = '" + id_project.ToString() + "') AND  (treatment.name = '" + _treat + "') AND (materials.name = '" + _mat + "') " +
                " AND ("+_state+") AND (researches.id_research = '" + _ID + "');")[0];
            if (_MassRec == "")
            {
                //_MassRec = "no";
            }
            else
            {
                _MassRec = "yes";
            }


            return _MassRec;
        }
        private System.Data.DataTable Fill_dt_specs(string Mat_sel, string Treat_sel)
        {
            System.Data.DataTable _ans = new System.Data.DataTable();
            string _filter = "";
            //номер исследования, оценка после обработки, объем (лучше в атомах, но нужно в нм), Число кластеров, Плотность объектов, Тип кластеров, Статус обработки
            _ans.Columns.Add("IDres");
            _ans.Columns.Add("Mass-rec.");
            _ans.Columns.Add("3D-rec.");
            _ans.Columns.Add("Converted");
            _ans.Columns.Add("Voxels");
            _ans.Columns.Add("Clusters");
            _ans.Columns.Add("Positioning");
            //получить список ИД образцов
            if (is_FilterCheked)
            {
                _filter = " AND (researches.success = '+')";
            }
            List<string> _SpecsID = SQL_str_request("SELECT distinct researches.id_research " +
                " FROM test2base.researches " +
                " LEFT OUTER JOIN test2base.specimens ON researches.id_specimen = specimens.idspecimens " +
                " LEFT OUTER JOIN test2base.projects ON projects.id_project = specimens.id_project " +
                " LEFT OUTER JOIN test2base.materials ON materials.id_material = specimens.id_material " +
                " LEFT OUTER JOIN test2base.treatment ON treatment.id_treatment = specimens.id_treatment " +
                " LEFT OUTER JOIN test2base.dataminig ON dataminig.id_research = researches.id_research " +
                " LEFT OUTER JOIN test2base.state ON state.id_state = dataminig.id_status " +
                " WHERE  (specimens.id_project = '"+ id_project.ToString() + "') AND  (treatment.name = '"+Treat_sel+"') AND (materials.name = '"+Mat_sel+"') "+_filter+";");
            foreach (string _IDOneSpec in  _SpecsID) 
            {
                _ans.Rows.Add(_IDOneSpec, GetYesNoDataminig(_IDOneSpec,Treat_sel,Mat_sel, "state.name = 'raw mass-spector mining'"), 
                    GetYesNoDataminig(_IDOneSpec, Treat_sel, Mat_sel, "(state.name = 'raw 3d mining') OR (state.name = 'raw approved')"),
                    GetYesNoDataminig(_IDOneSpec, Treat_sel, Mat_sel, "dataminig.comments = 'Pos conversion'"),
                    GetYesNoDataminig(_IDOneSpec, Treat_sel, Mat_sel, "deloc_param >0"), "",SQL_str_request("SELECT CONCAT(storage.name, ' ' ,storage_position.position)  " +
                    "FROM test2base.storage_position " +
                    "LEFT OUTER JOIN test2base.storage ON storage.id_storage = storage_position.id_storage" +
                    " WHERE (id_specimen = (SELECT id_specimen FROM test2base.researches WHERE (id_research = '"+ _IDOneSpec + "')));")[0]);
            }
            return _ans;
        }
        private void PaintGrid(DataGridView _tb)
        {
            if (_tb != null && _tb.RowCount > 0)
            {
                for (int i = 0; i < _tb.RowCount; i++)
                {
                    for (int j=1;j< _tb.Rows[i].Cells.Count-2;j++)
                    {
                        if (_tb.Rows[i].Cells[j].Value.ToString() == "yes")
                        {
                            _tb.Rows[i].Cells[j].Style.BackColor = Color.LightGreen;
                        }
                        else
                        {
                            _tb.Rows[i].Cells[j].Style.BackColor = Color.Salmon;
                        }
                    }
                }
            }
        }
        private void FillSpecInfo()
        {
            if (dataGridView1_APT_data.Rows.Count > 0 && dataGridView1_APT_data.SelectedRows.Count > 0)
            {
                //в Лабел выводим название состояния
                string material = dataGridView1_APT_data.Rows[dataGridView1_APT_data.SelectedRows[0].Index].Cells[0].Value.ToString();
                string treatment = dataGridView1_APT_data.Rows[dataGridView1_APT_data.SelectedRows[0].Index].Cells[1].Value.ToString();
                lbl_State_caption.Text = material + treatment;
                System.Data.DataTable dt_specs = Fill_dt_specs(material, treatment);
                dataGridView_specimens.DataSource = dt_specs;
                //раскраска?
                PaintGrid(dataGridView_specimens);
            }
            else
            {
                lbl_State_caption.Text = "";
                dataGridView_specimens.DataSource = null;
            }
            
        }
        private bool DrawZGCurve(ZedGraphControl zg, List<string> X, List<double> Y, string name)
        {
            GraphPane _pane = zg.GraphPane;
            _pane.CurveList.Clear();
            PointPairList _points = new PointPairList();            

            if (X.Count!=Y.Count) 
            {
                return false;
            }
            _pane.XAxis.Scale.TextLabels = X.ToArray();
            for (int i = 0; i < X.Count;i++)
            {
                _points.Add(new XDate(Convert.ToDateTime(X[i]).Date), Y[i]);
            }
            _pane.XAxis.Type = ZedGraph.AxisType.Text;

            _pane.AddCurve(name, _points, Color.Black);
            _pane.XAxis.Title.Text = "Date";
            _pane.YAxis.Title.Text = "Atoms count ";
            _pane.Title.Text = name;
            zg.AxisChange();
            zg.Invalidate();
            return true;
        }
        private List<string> GetDatas(string material, string treatment)
        {
            //List<string> ans = new List<string>();
            List<string> data_dirs = SQL_str_request("SELECT researches.res_date " +
                   "FROM test2base.specimens " +
                   "LEFT OUTER JOIN test2base.materials ON test2base.specimens.id_material = test2base.materials.id_material " +
                   "LEFT OUTER JOIN test2base.treatment ON specimens.id_treatment = treatment.id_treatment " +
                   "LEFT OUTER JOIN test2base.researches ON specimens.idspecimens = researches.id_specimen " +
                   "WHERE (specimens.id_project =  " + id_project.ToString() + ") " +
                   "AND (materials.name = '" + material + "') " +
                   treatment +
                   "AND (researches.success = '+')");

            return data_dirs;
        }
        private void PaintStatisctic()
        {
            try
            {
                //List<string> _dirs = new List<string>();
                //List<double> Y = Count_atoms("mat", " AND (treatment.name = '" + treatment + "')", _dirs);
                string material = dataGridView1_APT_data.Rows[dataGridView1_APT_data.SelectedRows[0].Index].Cells[0].Value.ToString();
                List<double> Y = Count_atoms(material, "");
                List<string> X = GetDatas(material, "");
                DrawZGCurve(zedGR_statistic, X, Y, material);
            }
            catch 
            {
                MessageBox.Show("some error in PaintStatisctic");
            }
        }
        private void tabcontrol_projects_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //если это вкладка Specimens, то обновляем данные по образцам
                FillSpecInfo();
                //вкладка статистики
                if (tabcontrol_projects_main.SelectedIndex == 2 && dataGridView1_APT_data!=null && dataGridView1_APT_data.Rows!=null && dataGridView1_APT_data.Rows.Count!=0) 
                {
                    PaintStatisctic();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString() , "error in tabcontrol_projects_main_SelectedIndexChanged", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
            }
        }

        private void btn_ch_prj_name_Click(object sender, EventArgs e)
        {
            if (Check_access_project(Properties.Settings.Default.default_username) && txtbox_info_name.Text!="")
            {
                //MessageBox.Show("Yes we can ch name of project");
                //UPDATE `test2base`.`projects` SET `name` = 'NO  ' WHERE (`id_project` = '21');
                SQL_com("UPDATE test2base.projects SET name = '"+txtbox_info_name.Text+"' WHERE (id_project = "+id_project.ToString()+")");
                Refresh_all();
                
            }
            else
            {
                MessageBox.Show("Changing name of project not allowed");
            }
        }

        private void btn_ch_contacts_Click(object sender, EventArgs e)
        {
            //изменить контактную информацию
            if (Check_access_project( Properties.Settings.Default.default_username))
            {
                SQL_com("UPDATE test2base.projects SET contacts = '" + richtxtbox_contacts_info.Text + "' WHERE (id_project = " + id_project.ToString() + ")");
                Refresh_all();
            }
            else
            {
                MessageBox.Show("Changing contacts info of project not allowed");
            }
        }

        private void btn_ch_contract_Click(object sender, EventArgs e)
        {
            //изменить контректную информацию
            if (Check_access_project(Properties.Settings.Default.default_username))
            {
                SQL_com("UPDATE test2base.projects SET contract = '" + txtbox_info_contract.Text + "' WHERE (id_project = " + id_project.ToString() + ")");
                Refresh_all();
            }
            else
            {
                MessageBox.Show("Changing contacts info of project not allowed");
            }
        }
        /// <summary>
        /// добавляем префикс к навзанию файла, оставляя только навзание из всего пути, len - длина пути без названия файла
        /// </summary>
        /// <param name="old_names"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        private List<string> Add_perfix(string[] old_names, int len)
        {
            List<string> ans = new List<string>();
            if (old_names.Length > 0)
            {
                for (int i = 0; i < old_names.Length; i++)
                {
                    ans.Add(DateTime.Now.ToString("yyyy-MM-dd ")+ old_names[i].Substring(len+1));
                    MessageBox.Show(ans[i]);
                }
            }
            return ans;
        }
        /// <summary>
        /// проверяем и создаем дирректорию, по умолчанию будет HOLY-BOX\APTfiles\Reports\
        /// </summary>
        /// <returns></returns>
        private string DirectoryReport_getnew()
        {
            string ans = @"\\HOLY-BOX\APTfiles\Reports\";
            try
            {
                string _directory_new = ans+SQL_str_request("SELECT name FROM test2base.projects WHERE (id_project = " + id_project.ToString() + ");")[0];
                if (!Directory.Exists(_directory_new))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(_directory_new);
                    dirInfo.Create();
                }
                ans = _directory_new+@"\";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in DirectoryReport_getnew\n"+ex.ToString());
            }
            return ans;
        }
        private void btn_report_load_Click(object sender, EventArgs e)
        {
            //проверить доступ
            if (Check_access_project(Properties.Settings.Default.default_username) && txtbox_info_name.Text != "")
            {
                //загрузка отчета
                //получить файл(ы)            
                //res =  saveFileDialog_report.ShowDialog();
                openFileDialog_report.Filter = "Report files (*.txt;*doc;*.docx;*pdf)|*.txt;*doc;*.docx;*pdf)";
                if (openFileDialog_report.ShowDialog() == DialogResult.OK)
                {
                    string[] _files = openFileDialog_report.FileNames;
                    if (_files.Length > 0)
                    {
                        //добавить к имени префикс даты
                        FileInfo _f_info = new FileInfo(_files[0]);
                        string _directory = _f_info.DirectoryName;
                        //создать/проверить на существование директории под проект
                        string _new_dir = DirectoryReport_getnew();
                        List<string> _names_new = Add_perfix(_files, _directory.Length);
                        //записать файл
                        for (int i = 0; i < _names_new.Count; i++)
                        {
                            File.Copy(_files[i], Path.Combine(_new_dir, _names_new[i]), true);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Only project master could add files");
            }
        }

        private void btn_report_show_Click(object sender, EventArgs e)
        {
            //проверить доступ
            if (Check_access_project(Properties.Settings.Default.default_username) && txtbox_info_name.Text != "")
            {
                string _path = @"\\HOLY-BOX\APTfiles\Reports\"+ SQL_str_request("SELECT name FROM test2base.projects WHERE (id_project = " + id_project.ToString() + ");")[0];
                if (Directory.Exists(_path))
                {
                    Process.Start("explorer.exe", _path);
                }
            }
            else
            {
                MessageBox.Show("Only project master could look into files");
            }
        }

        private void txtbox_info_name_KeyUp(object sender, KeyEventArgs e)
        {
            //ограничение макс размера
            if (txtbox_info_name.Text.Length>20)
            {
                MessageBox.Show("НЕ больше 20 символов");
                txtbox_info_name.Text = txtbox_info_name.Text.Remove(txtbox_info_name.Text.Length - 1, 1);
                e.Handled = false;                
            }
        }

        private void ch_list_box_filters_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!on_load)
            {
                if (e.NewValue == CheckState.Checked && ch_list_box_filters.SelectedIndex == 0)
                {
                    is_FilterCheked = true;
                }
                else
                {
                    is_FilterCheked = false;
                }
                FillSpecInfo();
            }
        }
        /// <summary>
        /// добавление нового ответственного
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddResposible_Click(object sender, EventArgs e)
        {
            if (Check_access_project(Properties.Settings.Default.default_username) && combox_pr_new_resp.Text!="")
            {
                string id_newresp = SQL_str_request("SELECT id_producer FROM test2base.producers WHERE (producers.surname = '" + combox_pr_new_resp.Text + "');")[0];
                if (id_newresp != "")
                {
                    SQL_com("INSERT INTO test2base.responsible_project (id_project, Id_responsible) VALUES ('" + id_project.ToString() + "', '"+id_newresp+"');");
                }
            }  
            else
            {
                MessageBox.Show("Только ответсвенный за проект может добавлять в проект новых ответственных");
            }
        }
    }
}
