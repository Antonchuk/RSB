
namespace RSB
{
    partial class Special_message
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
            this.picbox_special = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lbl_text = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picbox_special)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // picbox_special
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.picbox_special, 2);
            this.picbox_special.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picbox_special.Image = global::RSB.Properties.Resources.hypno_g;
            this.picbox_special.Location = new System.Drawing.Point(3, 3);
            this.picbox_special.Name = "picbox_special";
            this.picbox_special.Size = new System.Drawing.Size(1348, 322);
            this.picbox_special.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picbox_special.TabIndex = 0;
            this.picbox_special.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.Controls.Add(this.picbox_special, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lbl_text, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1354, 411);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // lbl_text
            // 
            this.lbl_text.AutoSize = true;
            this.lbl_text.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_text.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lbl_text.Location = new System.Drawing.Point(3, 328);
            this.lbl_text.Name = "lbl_text";
            this.lbl_text.Size = new System.Drawing.Size(1212, 83);
            this.lbl_text.TabIndex = 1;
            this.lbl_text.Text = "ggggg";
            this.lbl_text.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Special_message
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1354, 411);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Special_message";
            this.Text = "Special_message";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Special_message_FormClosing);
            this.Load += new System.EventHandler(this.Special_message_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picbox_special)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picbox_special;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lbl_text;
    }
}