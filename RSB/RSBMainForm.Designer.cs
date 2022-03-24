namespace RSB
{
    partial class RSBMainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_specimen = new System.Windows.Forms.Button();
            this.chbox_save_pass = new System.Windows.Forms.CheckBox();
            this.lbl_save_pass = new System.Windows.Forms.Label();
            this.txtbox_pass = new System.Windows.Forms.TextBox();
            this.lbl_pass = new System.Windows.Forms.Label();
            this.lbl_username = new System.Windows.Forms.Label();
            this.cbox_username = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btn_connect = new System.Windows.Forms.Button();
            this.btn_remind_pass = new System.Windows.Forms.Button();
            this.btn_exit = new System.Windows.Forms.Button();
            this.btn_research = new System.Windows.Forms.Button();
            this.btn_reports = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.howToBaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lbl_status = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_specimen
            // 
            this.btn_specimen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btn_specimen.Enabled = false;
            this.btn_specimen.Location = new System.Drawing.Point(784, 33);
            this.btn_specimen.Margin = new System.Windows.Forms.Padding(5);
            this.btn_specimen.Name = "btn_specimen";
            this.btn_specimen.Size = new System.Drawing.Size(190, 70);
            this.btn_specimen.TabIndex = 0;
            this.btn_specimen.Text = "Specimens";
            this.btn_specimen.UseVisualStyleBackColor = false;
            this.btn_specimen.Click += new System.EventHandler(this.Btn_specimen_Click);
            // 
            // chbox_save_pass
            // 
            this.chbox_save_pass.AutoSize = true;
            this.chbox_save_pass.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chbox_save_pass.Location = new System.Drawing.Point(235, 85);
            this.chbox_save_pass.Margin = new System.Windows.Forms.Padding(5);
            this.chbox_save_pass.Name = "chbox_save_pass";
            this.chbox_save_pass.Size = new System.Drawing.Size(441, 14);
            this.chbox_save_pass.TabIndex = 5;
            this.chbox_save_pass.UseVisualStyleBackColor = true;
            // 
            // lbl_save_pass
            // 
            this.lbl_save_pass.AutoSize = true;
            this.lbl_save_pass.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_save_pass.Location = new System.Drawing.Point(5, 80);
            this.lbl_save_pass.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lbl_save_pass.Name = "lbl_save_pass";
            this.lbl_save_pass.Size = new System.Drawing.Size(220, 24);
            this.lbl_save_pass.TabIndex = 4;
            this.lbl_save_pass.Text = "Save password";
            // 
            // txtbox_pass
            // 
            this.txtbox_pass.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtbox_pass.Location = new System.Drawing.Point(235, 45);
            this.txtbox_pass.Margin = new System.Windows.Forms.Padding(5);
            this.txtbox_pass.Name = "txtbox_pass";
            this.txtbox_pass.Size = new System.Drawing.Size(441, 30);
            this.txtbox_pass.TabIndex = 3;
            this.txtbox_pass.TextChanged += new System.EventHandler(this.Tbox_pass_TextChanged);
            // 
            // lbl_pass
            // 
            this.lbl_pass.AutoSize = true;
            this.lbl_pass.Dock = System.Windows.Forms.DockStyle.Top;
            this.lbl_pass.Location = new System.Drawing.Point(5, 40);
            this.lbl_pass.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lbl_pass.Name = "lbl_pass";
            this.lbl_pass.Size = new System.Drawing.Size(220, 22);
            this.lbl_pass.TabIndex = 2;
            this.lbl_pass.Text = "Password";
            // 
            // lbl_username
            // 
            this.lbl_username.AutoSize = true;
            this.lbl_username.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_username.Location = new System.Drawing.Point(5, 0);
            this.lbl_username.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lbl_username.Name = "lbl_username";
            this.lbl_username.Size = new System.Drawing.Size(220, 40);
            this.lbl_username.TabIndex = 1;
            this.lbl_username.Text = "User Name";
            // 
            // cbox_username
            // 
            this.cbox_username.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbox_username.FormattingEnabled = true;
            this.cbox_username.Location = new System.Drawing.Point(235, 5);
            this.cbox_username.Margin = new System.Windows.Forms.Padding(5);
            this.cbox_username.Name = "cbox_username";
            this.cbox_username.Size = new System.Drawing.Size(441, 30);
            this.cbox_username.TabIndex = 0;
            this.cbox_username.SelectedIndexChanged += new System.EventHandler(this.Cbox_username_SelectedIndexChanged);
            this.cbox_username.TextChanged += new System.EventHandler(this.Cbox_username_TextChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.80282F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66.19718F));
            this.tableLayoutPanel1.Controls.Add(this.btn_connect, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.btn_remind_pass, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.cbox_username, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lbl_username, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtbox_pass, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lbl_save_pass, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.chbox_save_pass, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.lbl_pass, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(14, 33);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(681, 154);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // btn_connect
            // 
            this.btn_connect.Dock = System.Windows.Forms.DockStyle.Right;
            this.btn_connect.Location = new System.Drawing.Point(458, 107);
            this.btn_connect.Name = "btn_connect";
            this.btn_connect.Size = new System.Drawing.Size(220, 44);
            this.btn_connect.TabIndex = 5;
            this.btn_connect.Text = "Connect";
            this.btn_connect.UseVisualStyleBackColor = true;
            this.btn_connect.Click += new System.EventHandler(this.Btn_connect_Click);
            // 
            // btn_remind_pass
            // 
            this.btn_remind_pass.Dock = System.Windows.Forms.DockStyle.Top;
            this.btn_remind_pass.Location = new System.Drawing.Point(5, 109);
            this.btn_remind_pass.Margin = new System.Windows.Forms.Padding(5);
            this.btn_remind_pass.Name = "btn_remind_pass";
            this.btn_remind_pass.Size = new System.Drawing.Size(220, 40);
            this.btn_remind_pass.TabIndex = 1;
            this.btn_remind_pass.Text = "Remind Password";
            this.btn_remind_pass.UseVisualStyleBackColor = true;
            this.btn_remind_pass.Click += new System.EventHandler(this.Btn_remind_pass_Click);
            // 
            // btn_exit
            // 
            this.btn_exit.Location = new System.Drawing.Point(549, 203);
            this.btn_exit.Margin = new System.Windows.Forms.Padding(5);
            this.btn_exit.Name = "btn_exit";
            this.btn_exit.Size = new System.Drawing.Size(143, 39);
            this.btn_exit.TabIndex = 6;
            this.btn_exit.Text = "Exit";
            this.btn_exit.UseVisualStyleBackColor = true;
            this.btn_exit.Click += new System.EventHandler(this.Btn_exit_Click);
            // 
            // btn_research
            // 
            this.btn_research.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btn_research.Enabled = false;
            this.btn_research.Location = new System.Drawing.Point(784, 111);
            this.btn_research.Name = "btn_research";
            this.btn_research.Size = new System.Drawing.Size(190, 70);
            this.btn_research.TabIndex = 1;
            this.btn_research.Text = "Researchs";
            this.btn_research.UseVisualStyleBackColor = false;
            this.btn_research.Click += new System.EventHandler(this.btn_research_Click);
            // 
            // btn_reports
            // 
            this.btn_reports.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btn_reports.Enabled = false;
            this.btn_reports.Location = new System.Drawing.Point(784, 187);
            this.btn_reports.Name = "btn_reports";
            this.btn_reports.Size = new System.Drawing.Size(190, 70);
            this.btn_reports.TabIndex = 2;
            this.btn_reports.Text = "Projects";
            this.btn_reports.UseVisualStyleBackColor = false;
            this.btn_reports.Click += new System.EventHandler(this.btn_reports_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.connectionToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(988, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(93, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // connectionToolStripMenuItem
            // 
            this.connectionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.propertiesToolStripMenuItem});
            this.connectionToolStripMenuItem.Name = "connectionToolStripMenuItem";
            this.connectionToolStripMenuItem.Size = new System.Drawing.Size(81, 20);
            this.connectionToolStripMenuItem.Text = "Connection";
            // 
            // propertiesToolStripMenuItem
            // 
            this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
            this.propertiesToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.propertiesToolStripMenuItem.Text = "Properties";
            this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.PropertiesToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.howToBaseToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // howToBaseToolStripMenuItem
            // 
            this.howToBaseToolStripMenuItem.Name = "howToBaseToolStripMenuItem";
            this.howToBaseToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.howToBaseToolStripMenuItem.Text = "HowTo base";
            this.howToBaseToolStripMenuItem.Click += new System.EventHandler(this.howToBaseToolStripMenuItem_Click);
            // 
            // lbl_status
            // 
            this.lbl_status.AutoSize = true;
            this.lbl_status.Location = new System.Drawing.Point(19, 244);
            this.lbl_status.Name = "lbl_status";
            this.lbl_status.Size = new System.Drawing.Size(126, 22);
            this.lbl_status.TabIndex = 4;
            this.lbl_status.Text = "No connection";
            // 
            // RSBMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.ClientSize = new System.Drawing.Size(988, 275);
            this.Controls.Add(this.lbl_status);
            this.Controls.Add(this.btn_reports);
            this.Controls.Add(this.btn_research);
            this.Controls.Add(this.btn_specimen);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.btn_exit);
            this.Font = new System.Drawing.Font("Cambria", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "RSBMainForm";
            this.Text = "RSB Main";
            this.Load += new System.EventHandler(this.RSBMainForm_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_specimen;
        public System.Windows.Forms.CheckBox chbox_save_pass;
        private System.Windows.Forms.Label lbl_save_pass;
        private System.Windows.Forms.Label lbl_pass;
        private System.Windows.Forms.Label lbl_username;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btn_remind_pass;
        private System.Windows.Forms.Button btn_exit;
        private System.Windows.Forms.Button btn_research;
        private System.Windows.Forms.Button btn_reports;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
        public System.Windows.Forms.TextBox txtbox_pass;
        public System.Windows.Forms.ComboBox cbox_username;
        private System.Windows.Forms.Label lbl_status;
        private System.Windows.Forms.Button btn_connect;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem howToBaseToolStripMenuItem;
    }
}

