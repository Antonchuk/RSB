namespace RSB
{
    partial class Materials_new
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
            this.btn_denay = new System.Windows.Forms.Button();
            this.btn_add_new = new System.Windows.Forms.Button();
            this.lbl_name = new System.Windows.Forms.Label();
            this.txtbox_name = new System.Windows.Forms.TextBox();
            this.lbl_composition = new System.Windows.Forms.Label();
            this.txtbox_composition = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btn_denay
            // 
            this.btn_denay.Location = new System.Drawing.Point(226, 117);
            this.btn_denay.Margin = new System.Windows.Forms.Padding(5);
            this.btn_denay.Name = "btn_denay";
            this.btn_denay.Size = new System.Drawing.Size(145, 54);
            this.btn_denay.TabIndex = 0;
            this.btn_denay.Text = "Denay";
            this.btn_denay.UseVisualStyleBackColor = true;
            this.btn_denay.Click += new System.EventHandler(this.Btn_denay_Click);
            // 
            // btn_add_new
            // 
            this.btn_add_new.Location = new System.Drawing.Point(16, 117);
            this.btn_add_new.Name = "btn_add_new";
            this.btn_add_new.Size = new System.Drawing.Size(146, 54);
            this.btn_add_new.TabIndex = 1;
            this.btn_add_new.Text = "Add New";
            this.btn_add_new.UseVisualStyleBackColor = true;
            this.btn_add_new.Click += new System.EventHandler(this.Btn_add_new_Click);
            // 
            // lbl_name
            // 
            this.lbl_name.AutoSize = true;
            this.lbl_name.Location = new System.Drawing.Point(12, 23);
            this.lbl_name.Name = "lbl_name";
            this.lbl_name.Size = new System.Drawing.Size(121, 23);
            this.lbl_name.TabIndex = 2;
            this.lbl_name.Text = "Material name";
            // 
            // txtbox_name
            // 
            this.txtbox_name.Location = new System.Drawing.Point(155, 15);
            this.txtbox_name.Name = "txtbox_name";
            this.txtbox_name.Size = new System.Drawing.Size(194, 31);
            this.txtbox_name.TabIndex = 3;
            // 
            // lbl_composition
            // 
            this.lbl_composition.AutoSize = true;
            this.lbl_composition.Location = new System.Drawing.Point(12, 70);
            this.lbl_composition.Name = "lbl_composition";
            this.lbl_composition.Size = new System.Drawing.Size(108, 23);
            this.lbl_composition.TabIndex = 4;
            this.lbl_composition.Text = "Composition";
            // 
            // txtbox_composition
            // 
            this.txtbox_composition.Location = new System.Drawing.Point(155, 62);
            this.txtbox_composition.Name = "txtbox_composition";
            this.txtbox_composition.Size = new System.Drawing.Size(194, 31);
            this.txtbox_composition.TabIndex = 5;
            // 
            // Materials_new
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(385, 183);
            this.Controls.Add(this.txtbox_composition);
            this.Controls.Add(this.lbl_composition);
            this.Controls.Add(this.txtbox_name);
            this.Controls.Add(this.lbl_name);
            this.Controls.Add(this.btn_add_new);
            this.Controls.Add(this.btn_denay);
            this.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "Materials_new";
            this.Text = "Materials";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Materials_new_FormClosed);
            this.Load += new System.EventHandler(this.Materials_new_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_denay;
        private System.Windows.Forms.Button btn_add_new;
        private System.Windows.Forms.Label lbl_name;
        private System.Windows.Forms.TextBox txtbox_name;
        private System.Windows.Forms.Label lbl_composition;
        private System.Windows.Forms.TextBox txtbox_composition;
    }
}