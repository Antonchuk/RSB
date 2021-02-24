namespace RSB
{
    partial class from_conn_prop
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtbox_server = new System.Windows.Forms.TextBox();
            this.txtbox_database = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtbox_port = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35.49107F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 64.50893F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtbox_server, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtbox_database, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.txtbox_port, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.button1, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.button2, 0, 4);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(20, 21);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(448, 180);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(5, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(148, 41);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(5, 41);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(148, 41);
            this.label2.TabIndex = 1;
            this.label2.Text = "Database";
            // 
            // txtbox_server
            // 
            this.txtbox_server.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtbox_server.Location = new System.Drawing.Point(163, 5);
            this.txtbox_server.Margin = new System.Windows.Forms.Padding(5);
            this.txtbox_server.Name = "txtbox_server";
            this.txtbox_server.Size = new System.Drawing.Size(280, 31);
            this.txtbox_server.TabIndex = 2;
            this.txtbox_server.Text = "172.16.0.151";
            // 
            // txtbox_database
            // 
            this.txtbox_database.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtbox_database.Location = new System.Drawing.Point(163, 46);
            this.txtbox_database.Margin = new System.Windows.Forms.Padding(5);
            this.txtbox_database.Name = "txtbox_database";
            this.txtbox_database.Size = new System.Drawing.Size(280, 31);
            this.txtbox_database.TabIndex = 3;
            this.txtbox_database.Text = "test2base";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 82);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 37);
            this.label3.TabIndex = 4;
            this.label3.Text = "Port";
            // 
            // txtbox_port
            // 
            this.txtbox_port.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtbox_port.Location = new System.Drawing.Point(161, 85);
            this.txtbox_port.Name = "txtbox_port";
            this.txtbox_port.Size = new System.Drawing.Size(284, 31);
            this.txtbox_port.TabIndex = 5;
            this.txtbox_port.Text = "3306";
            this.txtbox_port.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Txtbox_port_KeyPress);
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Top;
            this.button1.Location = new System.Drawing.Point(161, 122);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(284, 51);
            this.button1.TabIndex = 6;
            this.button1.Text = "Denay";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // button2
            // 
            this.button2.Dock = System.Windows.Forms.DockStyle.Top;
            this.button2.Location = new System.Drawing.Point(3, 122);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(152, 51);
            this.button2.TabIndex = 7;
            this.button2.Text = "Accept";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Button2_Click);
            // 
            // from_conn_prop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(483, 206);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "from_conn_prop";
            this.Text = "Connection properties";
            this.Load += new System.EventHandler(this.From_conn_prop_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtbox_server;
        private System.Windows.Forms.TextBox txtbox_database;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtbox_port;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}