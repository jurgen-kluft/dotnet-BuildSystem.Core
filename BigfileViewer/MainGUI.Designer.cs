namespace BigfileViewer
{
    partial class MainGUI
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
			this.treeGridView1 = new AdvancedDataGridView.TreeGridView();
			this.Find = new System.Windows.Forms.Button();
			this.searchText = new System.Windows.Forms.TextBox();
			this.findPrevious = new System.Windows.Forms.Button();
			this.fileOffsetMode = new System.Windows.Forms.RadioButton();
			this.fileSizeMode = new System.Windows.Forms.RadioButton();
			this.fileNameMode = new System.Windows.Forms.RadioButton();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkMatchWhole = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.fileIDMode = new System.Windows.Forms.RadioButton();
			this.FileID = new AdvancedDataGridView.TreeGridColumn();
			this.FileSize = new AdvancedDataGridView.TreeGridColumn();
			this.InstancesNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.FileTime = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.Filename = new AdvancedDataGridView.TreeGridColumn();
			this.Compressed = new AdvancedDataGridView.TreeGridColumn();
			((System.ComponentModel.ISupportInitialize)(this.treeGridView1)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(1015, 663);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 0;
			this.button1.Text = "Exit";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// treeGridView1
			// 
			this.treeGridView1.AllowUserToAddRows = false;
			this.treeGridView1.AllowUserToDeleteRows = false;
			this.treeGridView1.AllowUserToResizeColumns = false;
			this.treeGridView1.AllowUserToResizeRows = false;
			this.treeGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.treeGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.FileID,
            this.FileSize,
            this.InstancesNum,
            this.FileTime,
            this.Filename,
            this.Compressed});
			this.treeGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
			this.treeGridView1.ImageList = null;
			this.treeGridView1.Location = new System.Drawing.Point(12, 12);
			this.treeGridView1.Name = "treeGridView1";
			this.treeGridView1.RowHeadersVisible = false;
			this.treeGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
			this.treeGridView1.Size = new System.Drawing.Size(1078, 645);
			this.treeGridView1.TabIndex = 1;
			// 
			// Find
			// 
			this.Find.Location = new System.Drawing.Point(813, 664);
			this.Find.Name = "Find";
			this.Find.Size = new System.Drawing.Size(87, 23);
			this.Find.TabIndex = 0;
			this.Find.Text = "Find Next";
			this.Find.UseVisualStyleBackColor = true;
			this.Find.Click += new System.EventHandler(this.findNext_Click);
			// 
			// searchText
			// 
			this.searchText.Location = new System.Drawing.Point(640, 666);
			this.searchText.Name = "searchText";
			this.searchText.Size = new System.Drawing.Size(156, 20);
			this.searchText.TabIndex = 2;
			// 
			// findPrevious
			// 
			this.findPrevious.Location = new System.Drawing.Point(914, 664);
			this.findPrevious.Name = "findPrevious";
			this.findPrevious.Size = new System.Drawing.Size(87, 23);
			this.findPrevious.TabIndex = 0;
			this.findPrevious.Text = "Find Previous";
			this.findPrevious.UseVisualStyleBackColor = true;
			this.findPrevious.Click += new System.EventHandler(this.findPrevious_Click);
			// 
			// fileOffsetMode
			// 
			this.fileOffsetMode.AutoSize = true;
			this.fileOffsetMode.Location = new System.Drawing.Point(153, 11);
			this.fileOffsetMode.Name = "fileOffsetMode";
			this.fileOffsetMode.Size = new System.Drawing.Size(53, 17);
			this.fileOffsetMode.TabIndex = 3;
			this.fileOffsetMode.Text = "Offset";
			this.fileOffsetMode.UseVisualStyleBackColor = true;
			// 
			// fileSizeMode
			// 
			this.fileSizeMode.AutoSize = true;
			this.fileSizeMode.Location = new System.Drawing.Point(225, 11);
			this.fileSizeMode.Name = "fileSizeMode";
			this.fileSizeMode.Size = new System.Drawing.Size(45, 17);
			this.fileSizeMode.TabIndex = 3;
			this.fileSizeMode.Text = "Size";
			this.fileSizeMode.UseVisualStyleBackColor = true;
			// 
			// fileNameMode
			// 
			this.fileNameMode.AutoSize = true;
			this.fileNameMode.Checked = true;
			this.fileNameMode.Location = new System.Drawing.Point(288, 11);
			this.fileNameMode.Name = "fileNameMode";
			this.fileNameMode.Size = new System.Drawing.Size(72, 17);
			this.fileNameMode.TabIndex = 3;
			this.fileNameMode.TabStop = true;
			this.fileNameMode.Text = "File Name";
			this.fileNameMode.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.checkMatchWhole);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.fileNameMode);
			this.groupBox1.Controls.Add(this.fileSizeMode);
			this.groupBox1.Controls.Add(this.fileIDMode);
			this.groupBox1.Controls.Add(this.fileOffsetMode);
			this.groupBox1.Location = new System.Drawing.Point(126, 657);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(501, 33);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			// 
			// checkMatchWhole
			// 
			this.checkMatchWhole.AutoSize = true;
			this.checkMatchWhole.Location = new System.Drawing.Point(371, 12);
			this.checkMatchWhole.Name = "checkMatchWhole";
			this.checkMatchWhole.Size = new System.Drawing.Size(119, 17);
			this.checkMatchWhole.TabIndex = 5;
			this.checkMatchWhole.Text = "Match Whole Word";
			this.checkMatchWhole.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(7, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(74, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Search Mode:";
			// 
			// fileIDMode
			// 
			this.fileIDMode.AutoSize = true;
			this.fileIDMode.Location = new System.Drawing.Point(94, 11);
			this.fileIDMode.Name = "fileIDMode";
			this.fileIDMode.Size = new System.Drawing.Size(36, 17);
			this.fileIDMode.TabIndex = 3;
			this.fileIDMode.Text = "ID";
			this.fileIDMode.UseVisualStyleBackColor = true;
			// 
			// FileID
			// 
			this.FileID.DefaultNodeImage = null;
			this.FileID.Frozen = true;
			this.FileID.HeaderText = "File ID";
			this.FileID.Name = "FileID";
			this.FileID.ReadOnly = true;
			this.FileID.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.FileID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// FileSize
			// 
			this.FileSize.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.FileSize.DefaultNodeImage = null;
			this.FileSize.FillWeight = 128F;
			this.FileSize.HeaderText = "Size";
			this.FileSize.MinimumWidth = 128;
			this.FileSize.Name = "FileSize";
			this.FileSize.ReadOnly = true;
			this.FileSize.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.FileSize.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.FileSize.Width = 128;
			// 
			// InstancesNum
			// 
			this.InstancesNum.HeaderText = "InstancesNum";
			this.InstancesNum.Name = "InstancesNum";
			this.InstancesNum.ReadOnly = true;
			this.InstancesNum.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// FileTime
			// 
			this.FileTime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.FileTime.DropDownWidth = 100;
			this.FileTime.HeaderText = "Offset";
			this.FileTime.MinimumWidth = 100;
			this.FileTime.Name = "FileTime";
			this.FileTime.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			// 
			// Filename
			// 
			this.Filename.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.Filename.DefaultNodeImage = null;
			this.Filename.FillWeight = 512F;
			this.Filename.HeaderText = "Filename";
			this.Filename.MinimumWidth = 256;
			this.Filename.Name = "Filename";
			this.Filename.ReadOnly = true;
			this.Filename.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.Filename.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.Filename.Width = 640;
			// 
			// Compressed
			// 
			this.Compressed.DefaultNodeImage = null;
			this.Compressed.HeaderText = "Compressed";
			this.Compressed.Name = "Compressed";
			this.Compressed.ReadOnly = true;
			this.Compressed.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// MainGUI
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(1102, 697);
			this.ControlBox = false;
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.searchText);
			this.Controls.Add(this.treeGridView1);
			this.Controls.Add(this.findPrevious);
			this.Controls.Add(this.Find);
			this.Controls.Add(this.button1);
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MainGUI";
			this.Text = "BigfileViewer";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
			((System.ComponentModel.ISupportInitialize)(this.treeGridView1)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private AdvancedDataGridView.TreeGridView treeGridView1;
        private System.Windows.Forms.Button Find;
        private System.Windows.Forms.TextBox searchText;
        private System.Windows.Forms.Button findPrevious;
        private System.Windows.Forms.RadioButton fileOffsetMode;
        private System.Windows.Forms.RadioButton fileSizeMode;
        private System.Windows.Forms.RadioButton fileNameMode;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton fileIDMode;
		private System.Windows.Forms.CheckBox checkMatchWhole;
		private AdvancedDataGridView.TreeGridColumn FileID;
		private AdvancedDataGridView.TreeGridColumn FileSize;
		private System.Windows.Forms.DataGridViewTextBoxColumn InstancesNum;
		private System.Windows.Forms.DataGridViewComboBoxColumn FileTime;
		private AdvancedDataGridView.TreeGridColumn Filename;
		private AdvancedDataGridView.TreeGridColumn Compressed;
    }
}

