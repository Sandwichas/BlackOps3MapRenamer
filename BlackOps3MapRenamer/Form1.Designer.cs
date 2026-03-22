namespace BlackOps3MapRenamer {
	partial class Form1 {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.browseButton = new System.Windows.Forms.Button();
            this.mapFolderLabel = new System.Windows.Forms.Label();
            this.pathTextBox = new System.Windows.Forms.TextBox();
            this.detailsLabel = new System.Windows.Forms.Label();
            this.detailsRichTextBox = new System.Windows.Forms.RichTextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.renameButton = new System.Windows.Forms.Button();
            this.newNameTextBox = new System.Windows.Forms.TextBox();
            this.newNameLabel = new System.Windows.Forms.Label();
            this.newNamePrefixLabel = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.dupeCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // browseButton
            // 
            this.browseButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.browseButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.browseButton.Location = new System.Drawing.Point(1076, 7);
            this.browseButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(121, 45);
            this.browseButton.TabIndex = 0;
            this.browseButton.Text = "Browse...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // mapFolderLabel
            // 
            this.mapFolderLabel.AutoSize = true;
            this.mapFolderLabel.ForeColor = System.Drawing.Color.Silver;
            this.mapFolderLabel.Location = new System.Drawing.Point(14, 17);
            this.mapFolderLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.mapFolderLabel.Name = "mapFolderLabel";
            this.mapFolderLabel.Size = new System.Drawing.Size(130, 24);
            this.mapFolderLabel.TabIndex = 1;
            this.mapFolderLabel.Text = "Map Folder:";
            this.mapFolderLabel.Click += new System.EventHandler(this.mapFolderLabel_Click);
            this.mapFolderLabel.DoubleClick += new System.EventHandler(this.mapFolderLabel_DoubleClick);
            // 
            // pathTextBox
            // 
            this.pathTextBox.BackColor = System.Drawing.Color.Silver;
            this.pathTextBox.Location = new System.Drawing.Point(155, 7);
            this.pathTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pathTextBox.Name = "pathTextBox";
            this.pathTextBox.Size = new System.Drawing.Size(912, 32);
            this.pathTextBox.TabIndex = 2;
            // 
            // detailsLabel
            // 
            this.detailsLabel.AutoSize = true;
            this.detailsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.detailsLabel.Location = new System.Drawing.Point(15, 144);
            this.detailsLabel.Name = "detailsLabel";
            this.detailsLabel.Size = new System.Drawing.Size(86, 24);
            this.detailsLabel.TabIndex = 3;
            this.detailsLabel.Text = "Details:";
            // 
            // detailsRichTextBox
            // 
            this.detailsRichTextBox.BackColor = System.Drawing.Color.Silver;
            this.detailsRichTextBox.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.detailsRichTextBox.Location = new System.Drawing.Point(11, 171);
            this.detailsRichTextBox.Name = "detailsRichTextBox";
            this.detailsRichTextBox.ReadOnly = true;
            this.detailsRichTextBox.Size = new System.Drawing.Size(1187, 252);
            this.detailsRichTextBox.TabIndex = 4;
            this.detailsRichTextBox.Text = "";
            // 
            // renameButton
            // 
            this.renameButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.renameButton.FlatAppearance.BorderSize = 2;
            this.renameButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.renameButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.renameButton.Location = new System.Drawing.Point(688, 60);
            this.renameButton.Name = "renameButton";
            this.renameButton.Size = new System.Drawing.Size(509, 106);
            this.renameButton.TabIndex = 5;
            this.renameButton.Text = "Rename";
            this.renameButton.UseVisualStyleBackColor = true;
            this.renameButton.Click += new System.EventHandler(this.renameButton_Click);
            // 
            // newNameTextBox
            // 
            this.newNameTextBox.BackColor = System.Drawing.Color.Silver;
            this.newNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
            this.newNameTextBox.Location = new System.Drawing.Point(230, 58);
            this.newNameTextBox.Name = "newNameTextBox";
            this.newNameTextBox.Size = new System.Drawing.Size(437, 32);
            this.newNameTextBox.TabIndex = 6;
            this.newNameTextBox.TextChanged += new System.EventHandler(this.newNameTextBox_TextChanged);
            // 
            // newNameLabel
            // 
            this.newNameLabel.AutoSize = true;
            this.newNameLabel.ForeColor = System.Drawing.Color.Silver;
            this.newNameLabel.Location = new System.Drawing.Point(14, 66);
            this.newNameLabel.Name = "newNameLabel";
            this.newNameLabel.Size = new System.Drawing.Size(126, 24);
            this.newNameLabel.TabIndex = 7;
            this.newNameLabel.Text = "New Name:";
            // 
            // newNamePrefixLabel
            // 
            this.newNamePrefixLabel.AutoSize = true;
            this.newNamePrefixLabel.Font = new System.Drawing.Font("Adobe Gothic Std B", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.newNamePrefixLabel.ForeColor = System.Drawing.Color.Silver;
            this.newNamePrefixLabel.Location = new System.Drawing.Point(162, 66);
            this.newNamePrefixLabel.Name = "newNamePrefixLabel";
            this.newNamePrefixLabel.Size = new System.Drawing.Size(51, 25);
            this.newNamePrefixLabel.TabIndex = 8;
            this.newNamePrefixLabel.Text = "zm_";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(11, 440);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(1187, 22);
            this.progressBar1.TabIndex = 9;
            // 
            // dupeCheckBox
            // 
            this.dupeCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.dupeCheckBox.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.dupeCheckBox.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            this.dupeCheckBox.FlatAppearance.BorderSize = 2;
            this.dupeCheckBox.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
            this.dupeCheckBox.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.dupeCheckBox.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
            this.dupeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dupeCheckBox.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dupeCheckBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.dupeCheckBox.Location = new System.Drawing.Point(8, 92);
            this.dupeCheckBox.Name = "dupeCheckBox";
            this.dupeCheckBox.Size = new System.Drawing.Size(145, 45);
            this.dupeCheckBox.TabIndex = 10;
            this.dupeCheckBox.Text = "⬜ Duplicate";
            this.dupeCheckBox.UseVisualStyleBackColor = false;
            this.dupeCheckBox.CheckedChanged += new System.EventHandler(this.dupeCheckBox_CheckedChanged);
            this.dupeCheckBox.MouseHover += new System.EventHandler(this.dupeCheckBox_MouseHover);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(1211, 473);
            this.Controls.Add(this.dupeCheckBox);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.newNamePrefixLabel);
            this.Controls.Add(this.newNameLabel);
            this.Controls.Add(this.newNameTextBox);
            this.Controls.Add(this.renameButton);
            this.Controls.Add(this.detailsRichTextBox);
            this.Controls.Add(this.detailsLabel);
            this.Controls.Add(this.pathTextBox);
            this.Controls.Add(this.mapFolderLabel);
            this.Controls.Add(this.browseButton);
            this.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1229, 520);
            this.MinimumSize = new System.Drawing.Size(1229, 520);
            this.Name = "Form1";
            this.Text = "Black Ops 3 Map Renamer 1.2";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.Label mapFolderLabel;
		private System.Windows.Forms.TextBox pathTextBox;
		private System.Windows.Forms.Label detailsLabel;
		private System.Windows.Forms.RichTextBox detailsRichTextBox;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.Button renameButton;
		private System.Windows.Forms.TextBox newNameTextBox;
		private System.Windows.Forms.Label newNameLabel;
		private System.Windows.Forms.Label newNamePrefixLabel;
		private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.CheckBox dupeCheckBox;
    }
}

