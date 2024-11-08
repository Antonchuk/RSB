namespace RSB
{
    partial class SpecimenProperties
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
            this.grpBoxCommon = new System.Windows.Forms.GroupBox();
            this.radioButtonHard = new System.Windows.Forms.RadioButton();
            this.radioButtonEasy = new System.Windows.Forms.RadioButton();
            this.grpBoxTEMSEM = new System.Windows.Forms.GroupBox();
            this.radioButtonAttemptsHigh = new System.Windows.Forms.RadioButton();
            this.radioButtonAttemptsLow = new System.Windows.Forms.RadioButton();
            this.grpBoxFragile = new System.Windows.Forms.GroupBox();
            this.radioButtonFragileYes = new System.Windows.Forms.RadioButton();
            this.radioButtonFragileNo = new System.Windows.Forms.RadioButton();
            this.grpBoxSelectiveEtching = new System.Windows.Forms.GroupBox();
            this.radioButtonSelectiveYes = new System.Windows.Forms.RadioButton();
            this.radioButtonSelectiveNo = new System.Windows.Forms.RadioButton();
            this.grpBoxLongSection = new System.Windows.Forms.GroupBox();
            this.radioButtonLongThinYes = new System.Windows.Forms.RadioButton();
            this.radioButtonLongThinNo = new System.Windows.Forms.RadioButton();
            this.btnAccept = new System.Windows.Forms.Button();
            this.picboxSelectiveEtching = new System.Windows.Forms.PictureBox();
            this.lblSpecimenInfoCaption = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.grpBoxCommon.SuspendLayout();
            this.grpBoxTEMSEM.SuspendLayout();
            this.grpBoxFragile.SuspendLayout();
            this.grpBoxSelectiveEtching.SuspendLayout();
            this.grpBoxLongSection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picboxSelectiveEtching)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 41.52893F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58.47107F));
            this.tableLayoutPanel1.Controls.Add(this.grpBoxCommon, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.grpBoxTEMSEM, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.grpBoxFragile, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.grpBoxSelectiveEtching, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.grpBoxLongSection, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.btnAccept, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.picboxSelectiveEtching, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblSpecimenInfoCaption, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(384, 361);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // grpBoxCommon
            // 
            this.grpBoxCommon.Controls.Add(this.radioButtonHard);
            this.grpBoxCommon.Controls.Add(this.radioButtonEasy);
            this.grpBoxCommon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpBoxCommon.Location = new System.Drawing.Point(3, 53);
            this.grpBoxCommon.MinimumSize = new System.Drawing.Size(0, 40);
            this.grpBoxCommon.Name = "grpBoxCommon";
            this.grpBoxCommon.Size = new System.Drawing.Size(153, 44);
            this.grpBoxCommon.TabIndex = 0;
            this.grpBoxCommon.TabStop = false;
            this.grpBoxCommon.Text = "General characteristic";
            // 
            // radioButtonHard
            // 
            this.radioButtonHard.AutoSize = true;
            this.radioButtonHard.Dock = System.Windows.Forms.DockStyle.Right;
            this.radioButtonHard.Location = new System.Drawing.Point(102, 16);
            this.radioButtonHard.Name = "radioButtonHard";
            this.radioButtonHard.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.radioButtonHard.Size = new System.Drawing.Size(48, 25);
            this.radioButtonHard.TabIndex = 1;
            this.radioButtonHard.TabStop = true;
            this.radioButtonHard.Text = "Hard";
            this.radioButtonHard.UseVisualStyleBackColor = true;
            // 
            // radioButtonEasy
            // 
            this.radioButtonEasy.AutoSize = true;
            this.radioButtonEasy.Dock = System.Windows.Forms.DockStyle.Left;
            this.radioButtonEasy.Location = new System.Drawing.Point(3, 16);
            this.radioButtonEasy.Name = "radioButtonEasy";
            this.radioButtonEasy.Size = new System.Drawing.Size(48, 25);
            this.radioButtonEasy.TabIndex = 0;
            this.radioButtonEasy.TabStop = true;
            this.radioButtonEasy.Text = "Easy";
            this.radioButtonEasy.UseVisualStyleBackColor = true;
            // 
            // grpBoxTEMSEM
            // 
            this.grpBoxTEMSEM.Controls.Add(this.radioButtonAttemptsHigh);
            this.grpBoxTEMSEM.Controls.Add(this.radioButtonAttemptsLow);
            this.grpBoxTEMSEM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpBoxTEMSEM.Location = new System.Drawing.Point(3, 103);
            this.grpBoxTEMSEM.MinimumSize = new System.Drawing.Size(0, 40);
            this.grpBoxTEMSEM.Name = "grpBoxTEMSEM";
            this.grpBoxTEMSEM.Size = new System.Drawing.Size(153, 44);
            this.grpBoxTEMSEM.TabIndex = 1;
            this.grpBoxTEMSEM.TabStop = false;
            this.grpBoxTEMSEM.Text = "TEM/SEM attempts";
            // 
            // radioButtonAttemptsHigh
            // 
            this.radioButtonAttemptsHigh.AutoSize = true;
            this.radioButtonAttemptsHigh.Dock = System.Windows.Forms.DockStyle.Right;
            this.radioButtonAttemptsHigh.Location = new System.Drawing.Point(107, 16);
            this.radioButtonAttemptsHigh.Name = "radioButtonAttemptsHigh";
            this.radioButtonAttemptsHigh.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.radioButtonAttemptsHigh.Size = new System.Drawing.Size(43, 25);
            this.radioButtonAttemptsHigh.TabIndex = 1;
            this.radioButtonAttemptsHigh.TabStop = true;
            this.radioButtonAttemptsHigh.Text = "10<";
            this.radioButtonAttemptsHigh.UseVisualStyleBackColor = true;
            // 
            // radioButtonAttemptsLow
            // 
            this.radioButtonAttemptsLow.AutoSize = true;
            this.radioButtonAttemptsLow.Dock = System.Windows.Forms.DockStyle.Left;
            this.radioButtonAttemptsLow.Location = new System.Drawing.Point(3, 16);
            this.radioButtonAttemptsLow.Name = "radioButtonAttemptsLow";
            this.radioButtonAttemptsLow.Size = new System.Drawing.Size(40, 25);
            this.radioButtonAttemptsLow.TabIndex = 0;
            this.radioButtonAttemptsLow.TabStop = true;
            this.radioButtonAttemptsLow.Text = "1-3";
            this.radioButtonAttemptsLow.UseVisualStyleBackColor = true;
            // 
            // grpBoxFragile
            // 
            this.grpBoxFragile.Controls.Add(this.radioButtonFragileYes);
            this.grpBoxFragile.Controls.Add(this.radioButtonFragileNo);
            this.grpBoxFragile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpBoxFragile.Location = new System.Drawing.Point(3, 153);
            this.grpBoxFragile.MinimumSize = new System.Drawing.Size(0, 40);
            this.grpBoxFragile.Name = "grpBoxFragile";
            this.grpBoxFragile.Size = new System.Drawing.Size(153, 44);
            this.grpBoxFragile.TabIndex = 2;
            this.grpBoxFragile.TabStop = false;
            this.grpBoxFragile.Text = "Fragile";
            // 
            // radioButtonFragileYes
            // 
            this.radioButtonFragileYes.AutoSize = true;
            this.radioButtonFragileYes.Dock = System.Windows.Forms.DockStyle.Right;
            this.radioButtonFragileYes.Location = new System.Drawing.Point(107, 16);
            this.radioButtonFragileYes.Name = "radioButtonFragileYes";
            this.radioButtonFragileYes.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.radioButtonFragileYes.Size = new System.Drawing.Size(43, 25);
            this.radioButtonFragileYes.TabIndex = 1;
            this.radioButtonFragileYes.TabStop = true;
            this.radioButtonFragileYes.Text = "Yes";
            this.radioButtonFragileYes.UseVisualStyleBackColor = true;
            // 
            // radioButtonFragileNo
            // 
            this.radioButtonFragileNo.AutoSize = true;
            this.radioButtonFragileNo.Dock = System.Windows.Forms.DockStyle.Left;
            this.radioButtonFragileNo.Location = new System.Drawing.Point(3, 16);
            this.radioButtonFragileNo.Name = "radioButtonFragileNo";
            this.radioButtonFragileNo.Size = new System.Drawing.Size(39, 25);
            this.radioButtonFragileNo.TabIndex = 0;
            this.radioButtonFragileNo.TabStop = true;
            this.radioButtonFragileNo.Text = "No";
            this.radioButtonFragileNo.UseVisualStyleBackColor = true;
            // 
            // grpBoxSelectiveEtching
            // 
            this.grpBoxSelectiveEtching.Controls.Add(this.radioButtonSelectiveYes);
            this.grpBoxSelectiveEtching.Controls.Add(this.radioButtonSelectiveNo);
            this.grpBoxSelectiveEtching.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpBoxSelectiveEtching.Location = new System.Drawing.Point(3, 203);
            this.grpBoxSelectiveEtching.MinimumSize = new System.Drawing.Size(0, 40);
            this.grpBoxSelectiveEtching.Name = "grpBoxSelectiveEtching";
            this.grpBoxSelectiveEtching.Size = new System.Drawing.Size(153, 44);
            this.grpBoxSelectiveEtching.TabIndex = 3;
            this.grpBoxSelectiveEtching.TabStop = false;
            this.grpBoxSelectiveEtching.Text = "Selective etching";
            // 
            // radioButtonSelectiveYes
            // 
            this.radioButtonSelectiveYes.AutoSize = true;
            this.radioButtonSelectiveYes.Dock = System.Windows.Forms.DockStyle.Right;
            this.radioButtonSelectiveYes.Location = new System.Drawing.Point(107, 16);
            this.radioButtonSelectiveYes.Name = "radioButtonSelectiveYes";
            this.radioButtonSelectiveYes.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.radioButtonSelectiveYes.Size = new System.Drawing.Size(43, 25);
            this.radioButtonSelectiveYes.TabIndex = 1;
            this.radioButtonSelectiveYes.TabStop = true;
            this.radioButtonSelectiveYes.Text = "Yes";
            this.radioButtonSelectiveYes.UseVisualStyleBackColor = true;
            // 
            // radioButtonSelectiveNo
            // 
            this.radioButtonSelectiveNo.AutoSize = true;
            this.radioButtonSelectiveNo.Dock = System.Windows.Forms.DockStyle.Left;
            this.radioButtonSelectiveNo.Location = new System.Drawing.Point(3, 16);
            this.radioButtonSelectiveNo.Name = "radioButtonSelectiveNo";
            this.radioButtonSelectiveNo.Size = new System.Drawing.Size(39, 25);
            this.radioButtonSelectiveNo.TabIndex = 0;
            this.radioButtonSelectiveNo.TabStop = true;
            this.radioButtonSelectiveNo.Text = "No";
            this.radioButtonSelectiveNo.UseVisualStyleBackColor = true;
            // 
            // grpBoxLongSection
            // 
            this.grpBoxLongSection.Controls.Add(this.radioButtonLongThinYes);
            this.grpBoxLongSection.Controls.Add(this.radioButtonLongThinNo);
            this.grpBoxLongSection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpBoxLongSection.Location = new System.Drawing.Point(3, 253);
            this.grpBoxLongSection.MinimumSize = new System.Drawing.Size(0, 40);
            this.grpBoxLongSection.Name = "grpBoxLongSection";
            this.grpBoxLongSection.Size = new System.Drawing.Size(153, 44);
            this.grpBoxLongSection.TabIndex = 4;
            this.grpBoxLongSection.TabStop = false;
            this.grpBoxLongSection.Text = "Long thin 10 micron section";
            // 
            // radioButtonLongThinYes
            // 
            this.radioButtonLongThinYes.AutoSize = true;
            this.radioButtonLongThinYes.Dock = System.Windows.Forms.DockStyle.Left;
            this.radioButtonLongThinYes.Location = new System.Drawing.Point(3, 16);
            this.radioButtonLongThinYes.Name = "radioButtonLongThinYes";
            this.radioButtonLongThinYes.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.radioButtonLongThinYes.Size = new System.Drawing.Size(43, 25);
            this.radioButtonLongThinYes.TabIndex = 1;
            this.radioButtonLongThinYes.TabStop = true;
            this.radioButtonLongThinYes.Text = "Yes";
            this.radioButtonLongThinYes.UseVisualStyleBackColor = true;
            // 
            // radioButtonLongThinNo
            // 
            this.radioButtonLongThinNo.AutoSize = true;
            this.radioButtonLongThinNo.Dock = System.Windows.Forms.DockStyle.Right;
            this.radioButtonLongThinNo.Location = new System.Drawing.Point(111, 16);
            this.radioButtonLongThinNo.Name = "radioButtonLongThinNo";
            this.radioButtonLongThinNo.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.radioButtonLongThinNo.Size = new System.Drawing.Size(39, 25);
            this.radioButtonLongThinNo.TabIndex = 0;
            this.radioButtonLongThinNo.TabStop = true;
            this.radioButtonLongThinNo.Text = "No";
            this.radioButtonLongThinNo.UseVisualStyleBackColor = true;
            // 
            // btnAccept
            // 
            this.btnAccept.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAccept.Location = new System.Drawing.Point(3, 303);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(153, 55);
            this.btnAccept.TabIndex = 5;
            this.btnAccept.Text = "OK";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.btnAccept_Click);
            // 
            // picboxSelectiveEtching
            // 
            this.picboxSelectiveEtching.Image = global::RSB.Properties.Resources._2024_11_08_17_45_12;
            this.picboxSelectiveEtching.Location = new System.Drawing.Point(162, 203);
            this.picboxSelectiveEtching.Name = "picboxSelectiveEtching";
            this.picboxSelectiveEtching.Size = new System.Drawing.Size(219, 44);
            this.picboxSelectiveEtching.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picboxSelectiveEtching.TabIndex = 6;
            this.picboxSelectiveEtching.TabStop = false;
            // 
            // lblSpecimenInfoCaption
            // 
            this.lblSpecimenInfoCaption.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblSpecimenInfoCaption, 2);
            this.lblSpecimenInfoCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSpecimenInfoCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblSpecimenInfoCaption.Location = new System.Drawing.Point(3, 0);
            this.lblSpecimenInfoCaption.Name = "lblSpecimenInfoCaption";
            this.lblSpecimenInfoCaption.Size = new System.Drawing.Size(378, 50);
            this.lblSpecimenInfoCaption.TabIndex = 7;
            this.lblSpecimenInfoCaption.Text = "Enter information about the quality of the specimen you prepared.";
            // 
            // SpecimenProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 361);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(400, 400);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 400);
            this.Name = "SpecimenProperties";
            this.Text = "SpecimenProperties";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.grpBoxCommon.ResumeLayout(false);
            this.grpBoxCommon.PerformLayout();
            this.grpBoxTEMSEM.ResumeLayout(false);
            this.grpBoxTEMSEM.PerformLayout();
            this.grpBoxFragile.ResumeLayout(false);
            this.grpBoxFragile.PerformLayout();
            this.grpBoxSelectiveEtching.ResumeLayout(false);
            this.grpBoxSelectiveEtching.PerformLayout();
            this.grpBoxLongSection.ResumeLayout(false);
            this.grpBoxLongSection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picboxSelectiveEtching)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox grpBoxCommon;
        private System.Windows.Forms.RadioButton radioButtonHard;
        private System.Windows.Forms.RadioButton radioButtonEasy;
        private System.Windows.Forms.GroupBox grpBoxTEMSEM;
        private System.Windows.Forms.RadioButton radioButtonAttemptsLow;
        private System.Windows.Forms.RadioButton radioButtonAttemptsHigh;
        private System.Windows.Forms.GroupBox grpBoxFragile;
        private System.Windows.Forms.GroupBox grpBoxSelectiveEtching;
        private System.Windows.Forms.RadioButton radioButtonFragileYes;
        private System.Windows.Forms.RadioButton radioButtonFragileNo;
        private System.Windows.Forms.GroupBox grpBoxLongSection;
        private System.Windows.Forms.RadioButton radioButtonSelectiveYes;
        private System.Windows.Forms.RadioButton radioButtonSelectiveNo;
        private System.Windows.Forms.RadioButton radioButtonLongThinYes;
        private System.Windows.Forms.RadioButton radioButtonLongThinNo;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.PictureBox picboxSelectiveEtching;
        private System.Windows.Forms.Label lblSpecimenInfoCaption;
    }
}