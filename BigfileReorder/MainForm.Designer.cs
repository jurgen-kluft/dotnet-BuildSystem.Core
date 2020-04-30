namespace BigfileFileReorder
{
    partial class MainForm
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
			this.BigfileFilenameTB = new System.Windows.Forms.TextBox();
			this.OpenBigfileButton = new System.Windows.Forms.Button();
			this.OpenReorderFileButton = new System.Windows.Forms.Button();
			this.ReorderFilenameTB = new System.Windows.Forms.TextBox();
			this.SaveProgressBar = new System.Windows.Forms.ProgressBar();
			this.SaveButton = new System.Windows.Forms.Button();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.textBox4 = new System.Windows.Forms.TextBox();
			this.textBox5 = new System.Windows.Forms.TextBox();
			this.OpenMFTButton = new System.Windows.Forms.Button();
			this.MFTFilenameTB = new System.Windows.Forms.TextBox();
			this.ViewButton = new System.Windows.Forms.Button();
			this.PlatformPCRadio = new System.Windows.Forms.RadioButton();
			this.PlatformPS2Radio = new System.Windows.Forms.RadioButton();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BigfileFilenameTB
			// 
			this.BigfileFilenameTB.AllowDrop = true;
			this.BigfileFilenameTB.Location = new System.Drawing.Point(155, 12);
			this.BigfileFilenameTB.Name = "BigfileFilenameTB";
			this.BigfileFilenameTB.Size = new System.Drawing.Size(453, 20);
			this.BigfileFilenameTB.TabIndex = 0;
			this.BigfileFilenameTB.TextChanged += new System.EventHandler(this.OnBigFileChanged);
			// 
			// OpenBigfileButton
			// 
			this.OpenBigfileButton.Location = new System.Drawing.Point(614, 12);
			this.OpenBigfileButton.Name = "OpenBigfileButton";
			this.OpenBigfileButton.Size = new System.Drawing.Size(22, 20);
			this.OpenBigfileButton.TabIndex = 1;
			this.OpenBigfileButton.Text = "..";
			this.OpenBigfileButton.UseVisualStyleBackColor = true;
			this.OpenBigfileButton.Click += new System.EventHandler(this.OnBrowseBigFile);
			// 
			// OpenReorderFileButton
			// 
			this.OpenReorderFileButton.Location = new System.Drawing.Point(614, 64);
			this.OpenReorderFileButton.Name = "OpenReorderFileButton";
			this.OpenReorderFileButton.Size = new System.Drawing.Size(22, 20);
			this.OpenReorderFileButton.TabIndex = 3;
			this.OpenReorderFileButton.Text = "..";
			this.OpenReorderFileButton.UseVisualStyleBackColor = true;
			this.OpenReorderFileButton.Click += new System.EventHandler(this.OnBrowseOrderFile);
			// 
			// ReorderFilenameTB
			// 
			this.ReorderFilenameTB.AllowDrop = true;
			this.ReorderFilenameTB.Location = new System.Drawing.Point(155, 64);
			this.ReorderFilenameTB.Name = "ReorderFilenameTB";
			this.ReorderFilenameTB.Size = new System.Drawing.Size(453, 20);
			this.ReorderFilenameTB.TabIndex = 2;
			this.ReorderFilenameTB.TextChanged += new System.EventHandler(this.OnOrderFileChanged);
			// 
			// SaveProgressBar
			// 
			this.SaveProgressBar.Location = new System.Drawing.Point(11, 127);
			this.SaveProgressBar.Name = "SaveProgressBar";
			this.SaveProgressBar.Size = new System.Drawing.Size(624, 23);
			this.SaveProgressBar.TabIndex = 4;
			// 
			// SaveButton
			// 
			this.SaveButton.Enabled = false;
			this.SaveButton.Location = new System.Drawing.Point(11, 98);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = new System.Drawing.Size(75, 23);
			this.SaveButton.TabIndex = 5;
			this.SaveButton.Text = "Save";
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.OnSaveButtonClicked);
			// 
			// textBox3
			// 
			this.textBox3.Cursor = System.Windows.Forms.Cursors.Default;
			this.textBox3.Enabled = false;
			this.textBox3.Location = new System.Drawing.Point(13, 11);
			this.textBox3.Name = "textBox3";
			this.textBox3.ReadOnly = true;
			this.textBox3.Size = new System.Drawing.Size(136, 20);
			this.textBox3.TabIndex = 6;
			this.textBox3.TabStop = false;
			this.textBox3.Text = "Bigfile";
			this.textBox3.WordWrap = false;
			// 
			// textBox4
			// 
			this.textBox4.Cursor = System.Windows.Forms.Cursors.Default;
			this.textBox4.Enabled = false;
			this.textBox4.Location = new System.Drawing.Point(13, 63);
			this.textBox4.Name = "textBox4";
			this.textBox4.ReadOnly = true;
			this.textBox4.Size = new System.Drawing.Size(136, 20);
			this.textBox4.TabIndex = 7;
			this.textBox4.TabStop = false;
			this.textBox4.Text = "Reordering info file";
			this.textBox4.WordWrap = false;
			// 
			// textBox5
			// 
			this.textBox5.Cursor = System.Windows.Forms.Cursors.Default;
			this.textBox5.Enabled = false;
			this.textBox5.Location = new System.Drawing.Point(13, 37);
			this.textBox5.Name = "textBox5";
			this.textBox5.ReadOnly = true;
			this.textBox5.Size = new System.Drawing.Size(136, 20);
			this.textBox5.TabIndex = 10;
			this.textBox5.TabStop = false;
			this.textBox5.Text = "Master file table";
			this.textBox5.WordWrap = false;
			// 
			// OpenMFTButton
			// 
			this.OpenMFTButton.Location = new System.Drawing.Point(614, 38);
			this.OpenMFTButton.Name = "OpenMFTButton";
			this.OpenMFTButton.Size = new System.Drawing.Size(22, 20);
			this.OpenMFTButton.TabIndex = 9;
			this.OpenMFTButton.Text = "..";
			this.OpenMFTButton.UseVisualStyleBackColor = true;
			this.OpenMFTButton.Click += new System.EventHandler(this.OnBrowseTocFile);
			// 
			// MFTFilenameTB
			// 
			this.MFTFilenameTB.AllowDrop = true;
			this.MFTFilenameTB.Location = new System.Drawing.Point(155, 38);
			this.MFTFilenameTB.Name = "MFTFilenameTB";
			this.MFTFilenameTB.Size = new System.Drawing.Size(453, 20);
			this.MFTFilenameTB.TabIndex = 8;
			this.MFTFilenameTB.TextChanged += new System.EventHandler(this.OnTocFileChanged);
			// 
			// ViewButton
			// 
			this.ViewButton.Location = new System.Drawing.Point(92, 98);
			this.ViewButton.Name = "ViewButton";
			this.ViewButton.Size = new System.Drawing.Size(75, 23);
			this.ViewButton.TabIndex = 11;
			this.ViewButton.Text = "View";
			this.ViewButton.UseVisualStyleBackColor = true;
			this.ViewButton.Click += new System.EventHandler(this.OnView);
			// 
			// PlatformPCRadio
			// 
			this.PlatformPCRadio.AutoSize = true;
			this.PlatformPCRadio.Checked = true;
			this.PlatformPCRadio.Location = new System.Drawing.Point(63, 14);
			this.PlatformPCRadio.Name = "PlatformPCRadio";
			this.PlatformPCRadio.Size = new System.Drawing.Size(39, 17);
			this.PlatformPCRadio.TabIndex = 12;
			this.PlatformPCRadio.TabStop = true;
			this.PlatformPCRadio.Text = "PC";
			this.PlatformPCRadio.UseVisualStyleBackColor = true;
			// 
			// PlatformPS2Radio
			// 
			this.PlatformPS2Radio.AutoSize = true;
			this.PlatformPS2Radio.Location = new System.Drawing.Point(108, 14);
			this.PlatformPS2Radio.Name = "PlatformPS2Radio";
			this.PlatformPS2Radio.Size = new System.Drawing.Size(45, 17);
			this.PlatformPS2Radio.TabIndex = 12;
			this.PlatformPS2Radio.Text = "PS2";
			this.PlatformPS2Radio.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.PlatformPS2Radio);
			this.groupBox1.Controls.Add(this.PlatformPCRadio);
			this.groupBox1.Location = new System.Drawing.Point(192, 86);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(162, 35);
			this.groupBox1.TabIndex = 13;
			this.groupBox1.TabStop = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(48, 13);
			this.label1.TabIndex = 13;
			this.label1.Text = "Platform:";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(647, 160);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.ViewButton);
			this.Controls.Add(this.textBox5);
			this.Controls.Add(this.OpenMFTButton);
			this.Controls.Add(this.MFTFilenameTB);
			this.Controls.Add(this.textBox4);
			this.Controls.Add(this.textBox3);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.SaveProgressBar);
			this.Controls.Add(this.OpenReorderFileButton);
			this.Controls.Add(this.ReorderFilenameTB);
			this.Controls.Add(this.OpenBigfileButton);
			this.Controls.Add(this.BigfileFilenameTB);
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.Text = "BigfileReorder";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox BigfileFilenameTB;
        private System.Windows.Forms.Button OpenBigfileButton;
        private System.Windows.Forms.Button OpenReorderFileButton;
        private System.Windows.Forms.TextBox ReorderFilenameTB;
        private System.Windows.Forms.ProgressBar SaveProgressBar;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Button OpenMFTButton;
        private System.Windows.Forms.TextBox MFTFilenameTB;
        private System.Windows.Forms.Button ViewButton;
		private System.Windows.Forms.RadioButton PlatformPCRadio;
		private System.Windows.Forms.RadioButton PlatformPS2Radio;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
    }
}

