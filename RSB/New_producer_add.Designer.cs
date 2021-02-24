namespace RSB
{
    partial class New_producer_add
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
            this.button1 = new System.Windows.Forms.Button();
            this.btn_accept = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtbox_name = new System.Windows.Forms.TextBox();
            this.txtbox_surname = new System.Windows.Forms.TextBox();
            this.lbl_access = new System.Windows.Forms.Label();
            this.combox_access = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(204, 149);
            this.button1.Margin = new System.Windows.Forms.Padding(5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(140, 54);
            this.button1.TabIndex = 0;
            this.button1.Text = "Denay";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // btn_accept
            // 
            this.btn_accept.Location = new System.Drawing.Point(14, 149);
            this.btn_accept.Margin = new System.Windows.Forms.Padding(5);
            this.btn_accept.Name = "btn_accept";
            this.btn_accept.Size = new System.Drawing.Size(142, 54);
            this.btn_accept.TabIndex = 1;
            this.btn_accept.Text = "Accept";
            this.btn_accept.UseVisualStyleBackColor = true;
            this.btn_accept.Click += new System.EventHandler(this.Btn_accept_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 23);
            this.label1.TabIndex = 2;
            this.label1.Text = "Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 23);
            this.label2.TabIndex = 3;
            this.label2.Text = "Surname";
            // 
            // txtbox_name
            // 
            this.txtbox_name.Location = new System.Drawing.Point(129, 19);
            this.txtbox_name.Name = "txtbox_name";
            this.txtbox_name.Size = new System.Drawing.Size(215, 31);
            this.txtbox_name.TabIndex = 4;
            // 
            // txtbox_surname
            // 
            this.txtbox_surname.Location = new System.Drawing.Point(129, 63);
            this.txtbox_surname.Name = "txtbox_surname";
            this.txtbox_surname.Size = new System.Drawing.Size(215, 31);
            this.txtbox_surname.TabIndex = 5;
            // 
            // lbl_access
            // 
            this.lbl_access.AutoSize = true;
            this.lbl_access.Location = new System.Drawing.Point(15, 110);
            this.lbl_access.Name = "lbl_access";
            this.lbl_access.Size = new System.Drawing.Size(62, 23);
            this.lbl_access.TabIndex = 6;
            this.lbl_access.Text = "Access";
            // 
            // combox_access
            // 
            this.combox_access.FormattingEnabled = true;
            this.combox_access.Location = new System.Drawing.Point(129, 110);
            this.combox_access.Name = "combox_access";
            this.combox_access.Size = new System.Drawing.Size(215, 31);
            this.combox_access.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(372, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(181, 23);
            this.label3.TabIndex = 8;
            this.label3.Text = "1 - user, 2 - super user";
            // 
            // New_producer_add
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(572, 228);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.combox_access);
            this.Controls.Add(this.lbl_access);
            this.Controls.Add(this.txtbox_surname);
            this.Controls.Add(this.txtbox_name);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_accept);
            this.Controls.Add(this.button1);
            this.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "New_producer_add";
            this.Text = "New Producer";
            this.Load += new System.EventHandler(this.New_producer_add_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btn_accept;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtbox_name;
        private System.Windows.Forms.TextBox txtbox_surname;
        private System.Windows.Forms.Label lbl_access;
        private System.Windows.Forms.ComboBox combox_access;
        private System.Windows.Forms.Label label3;
    }
}