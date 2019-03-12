namespace Aksyon_Project
{
    partial class MainWindow
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
            this.cmbDevices = new System.Windows.Forms.ComboBox();
            this.btnReload = new System.Windows.Forms.Button();
            this.lstUsers = new System.Windows.Forms.ListBox();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnView = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.lblGBMSGUIVersion = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label11 = new System.Windows.Forms.Label();
            this.cmb_qualifier = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.dtp_birthday = new System.Windows.Forms.DateTimePicker();
            this.button1 = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txt_alias = new System.Windows.Forms.TextBox();
            this.txt_middlename = new System.Windows.Forms.TextBox();
            this.txt_lastname = new System.Windows.Forms.TextBox();
            this.txt_firstname = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbMPSPS = new System.Windows.Forms.ComboBox();
            this.cmbPPOCPODistrict = new System.Windows.Forms.ComboBox();
            this.cmbPRORegion = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.dgvPersonalities = new System.Windows.Forms.DataGridView();
            this.colPID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPRO = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPPO = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPS = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHVTSLT = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colClassification = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDateEntry = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colListedByDI = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label9 = new System.Windows.Forms.Label();
            this.btn_loadPersonalities = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPersonalities)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbDevices
            // 
            this.cmbDevices.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbDevices.FormattingEnabled = true;
            this.cmbDevices.Location = new System.Drawing.Point(16, 3);
            this.cmbDevices.Name = "cmbDevices";
            this.cmbDevices.Size = new System.Drawing.Size(245, 28);
            this.cmbDevices.TabIndex = 0;
            this.cmbDevices.SelectedIndexChanged += new System.EventHandler(this.cmbDevices_SelectedIndexChanged);
            // 
            // btnReload
            // 
            this.btnReload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReload.Location = new System.Drawing.Point(16, 37);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(85, 30);
            this.btnReload.TabIndex = 1;
            this.btnReload.Text = "Refresh";
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // lstUsers
            // 
            this.lstUsers.BackColor = System.Drawing.Color.White;
            this.lstUsers.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstUsers.FormattingEnabled = true;
            this.lstUsers.ItemHeight = 16;
            this.lstUsers.Location = new System.Drawing.Point(416, 3);
            this.lstUsers.Name = "lstUsers";
            this.lstUsers.Size = new System.Drawing.Size(108, 84);
            this.lstUsers.TabIndex = 2;
            this.lstUsers.SelectedIndexChanged += new System.EventHandler(this.lstUsers_SelectedIndexChanged);
            this.lstUsers.DoubleClick += new System.EventHandler(this.lstUsers_DoubleClick);
            // 
            // btnNew
            // 
            this.btnNew.Location = new System.Drawing.Point(107, 37);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(85, 27);
            this.btnNew.TabIndex = 3;
            this.btnNew.Text = "New";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // btnView
            // 
            this.btnView.Location = new System.Drawing.Point(198, 37);
            this.btnView.Name = "btnView";
            this.btnView.Size = new System.Drawing.Size(85, 27);
            this.btnView.TabIndex = 4;
            this.btnView.Text = "View";
            this.btnView.UseVisualStyleBackColor = true;
            this.btnView.Click += new System.EventHandler(this.btnView_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(289, 37);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(85, 27);
            this.btnDelete.TabIndex = 5;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // lblGBMSGUIVersion
            // 
            this.lblGBMSGUIVersion.AutoSize = true;
            this.lblGBMSGUIVersion.Location = new System.Drawing.Point(205, 74);
            this.lblGBMSGUIVersion.Name = "lblGBMSGUIVersion";
            this.lblGBMSGUIVersion.Size = new System.Drawing.Size(100, 13);
            this.lblGBMSGUIVersion.TabIndex = 6;
            this.lblGBMSGUIVersion.Text = "GBMSGUI version: ";
            this.lblGBMSGUIVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.panel1.Controls.Add(this.label11);
            this.panel1.Controls.Add(this.cmb_qualifier);
            this.panel1.Controls.Add(this.label10);
            this.panel1.Controls.Add(this.dtp_birthday);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.txt_alias);
            this.panel1.Controls.Add(this.txt_middlename);
            this.panel1.Controls.Add(this.txt_lastname);
            this.panel1.Controls.Add(this.txt_firstname);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.cmbMPSPS);
            this.panel1.Controls.Add(this.cmbPPOCPODistrict);
            this.panel1.Controls.Add(this.cmbPRORegion);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 43);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1094, 226);
            this.panel1.TabIndex = 9;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.Color.White;
            this.label11.Location = new System.Drawing.Point(293, 154);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(61, 17);
            this.label11.TabIndex = 18;
            this.label11.Text = "Qualifier";
            // 
            // cmb_qualifier
            // 
            this.cmb_qualifier.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_qualifier.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmb_qualifier.FormattingEnabled = true;
            this.cmb_qualifier.Location = new System.Drawing.Point(296, 173);
            this.cmb_qualifier.Name = "cmb_qualifier";
            this.cmb_qualifier.Size = new System.Drawing.Size(248, 33);
            this.cmb_qualifier.TabIndex = 17;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.White;
            this.label10.Location = new System.Drawing.Point(29, 154);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(60, 17);
            this.label10.TabIndex = 16;
            this.label10.Text = "Birthday";
            // 
            // dtp_birthday
            // 
            this.dtp_birthday.CustomFormat = "yyyy-MM-dd";
            this.dtp_birthday.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtp_birthday.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtp_birthday.Location = new System.Drawing.Point(32, 173);
            this.dtp_birthday.Name = "dtp_birthday";
            this.dtp_birthday.Size = new System.Drawing.Size(248, 30);
            this.dtp_birthday.TabIndex = 15;
            this.dtp_birthday.Value = new System.DateTime(2019, 3, 11, 11, 11, 46, 0);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(84)))), ((int)(((byte)(7)))));
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGray;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(824, 164);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(245, 39);
            this.button1.TabIndex = 14;
            this.button1.Text = "Search Box Personalities";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.White;
            this.label8.Location = new System.Drawing.Point(824, 81);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(38, 17);
            this.label8.TabIndex = 13;
            this.label8.Text = "Alias";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(558, 81);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(90, 17);
            this.label7.TabIndex = 12;
            this.label7.Text = "Middle Name";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(293, 81);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 17);
            this.label6.TabIndex = 11;
            this.label6.Text = "Last Name";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(29, 81);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 17);
            this.label5.TabIndex = 10;
            this.label5.Text = "First Name";
            // 
            // txt_alias
            // 
            this.txt_alias.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_alias.Location = new System.Drawing.Point(824, 101);
            this.txt_alias.Name = "txt_alias";
            this.txt_alias.Size = new System.Drawing.Size(248, 30);
            this.txt_alias.TabIndex = 9;
            // 
            // txt_middlename
            // 
            this.txt_middlename.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_middlename.Location = new System.Drawing.Point(561, 101);
            this.txt_middlename.Name = "txt_middlename";
            this.txt_middlename.Size = new System.Drawing.Size(248, 30);
            this.txt_middlename.TabIndex = 8;
            // 
            // txt_lastname
            // 
            this.txt_lastname.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_lastname.Location = new System.Drawing.Point(296, 101);
            this.txt_lastname.Name = "txt_lastname";
            this.txt_lastname.Size = new System.Drawing.Size(248, 30);
            this.txt_lastname.TabIndex = 7;
            // 
            // txt_firstname
            // 
            this.txt_firstname.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_firstname.Location = new System.Drawing.Point(32, 101);
            this.txt_firstname.Name = "txt_firstname";
            this.txt_firstname.Size = new System.Drawing.Size(248, 30);
            this.txt_firstname.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(755, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(127, 17);
            this.label4.TabIndex = 5;
            this.label4.Text = "MPS/Police Station";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(395, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "PPO/CPO/District";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(29, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "PRO/Region";
            // 
            // cmbMPSPS
            // 
            this.cmbMPSPS.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMPSPS.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbMPSPS.FormattingEnabled = true;
            this.cmbMPSPS.Location = new System.Drawing.Point(758, 34);
            this.cmbMPSPS.Name = "cmbMPSPS";
            this.cmbMPSPS.Size = new System.Drawing.Size(314, 33);
            this.cmbMPSPS.TabIndex = 2;
            this.cmbMPSPS.SelectedIndexChanged += new System.EventHandler(this.cmbMPSPS_SelectedIndexChanged);
            // 
            // cmbPPOCPODistrict
            // 
            this.cmbPPOCPODistrict.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPPOCPODistrict.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbPPOCPODistrict.FormattingEnabled = true;
            this.cmbPPOCPODistrict.Location = new System.Drawing.Point(398, 33);
            this.cmbPPOCPODistrict.Name = "cmbPPOCPODistrict";
            this.cmbPPOCPODistrict.Size = new System.Drawing.Size(314, 33);
            this.cmbPPOCPODistrict.TabIndex = 1;
            this.cmbPPOCPODistrict.SelectedIndexChanged += new System.EventHandler(this.cmbPPOCPODistrict_SelectedIndexChanged);
            // 
            // cmbPRORegion
            // 
            this.cmbPRORegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPRORegion.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbPRORegion.FormattingEnabled = true;
            this.cmbPRORegion.Location = new System.Drawing.Point(32, 34);
            this.cmbPRORegion.Name = "cmbPRORegion";
            this.cmbPRORegion.Size = new System.Drawing.Size(314, 33);
            this.cmbPRORegion.TabIndex = 0;
            this.cmbPRORegion.SelectedIndexChanged += new System.EventHandler(this.cmbPRORegion_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(84)))), ((int)(((byte)(7)))));
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(1094, 40);
            this.label2.TabIndex = 0;
            this.label2.Text = "ADVANCE SEARCH";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 232F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1100, 272);
            this.tableLayoutPanel1.TabIndex = 10;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lblGBMSGUIVersion);
            this.panel2.Controls.Add(this.btnReload);
            this.panel2.Controls.Add(this.lstUsers);
            this.panel2.Controls.Add(this.cmbDevices);
            this.panel2.Controls.Add(this.btnDelete);
            this.panel2.Controls.Add(this.btnNew);
            this.panel2.Controls.Add(this.btnView);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 590);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1100, 10);
            this.panel2.TabIndex = 11;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.panel3.Controls.Add(this.btn_loadPersonalities);
            this.panel3.Controls.Add(this.dgvPersonalities);
            this.panel3.Controls.Add(this.label9);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.ForeColor = System.Drawing.Color.Coral;
            this.panel3.Location = new System.Drawing.Point(0, 272);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(2);
            this.panel3.Size = new System.Drawing.Size(1100, 318);
            this.panel3.TabIndex = 12;
            // 
            // dgvPersonalities
            // 
            this.dgvPersonalities.AllowUserToAddRows = false;
            this.dgvPersonalities.AllowUserToDeleteRows = false;
            this.dgvPersonalities.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPersonalities.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            this.dgvPersonalities.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvPersonalities.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPersonalities.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colPID,
            this.colName,
            this.colPRO,
            this.colPPO,
            this.colPS,
            this.colHVTSLT,
            this.colClassification,
            this.colStatus,
            this.colDateEntry,
            this.colListedByDI});
            this.dgvPersonalities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvPersonalities.Location = new System.Drawing.Point(2, 37);
            this.dgvPersonalities.Margin = new System.Windows.Forms.Padding(0);
            this.dgvPersonalities.MultiSelect = false;
            this.dgvPersonalities.Name = "dgvPersonalities";
            this.dgvPersonalities.ReadOnly = true;
            this.dgvPersonalities.RowHeadersVisible = false;
            this.dgvPersonalities.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgvPersonalities.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPersonalities.Size = new System.Drawing.Size(1096, 279);
            this.dgvPersonalities.TabIndex = 0;
            this.dgvPersonalities.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvPersonalities_CellContentDoubleClick);
            // 
            // colPID
            // 
            this.colPID.HeaderText = "Person ID";
            this.colPID.Name = "colPID";
            this.colPID.ReadOnly = true;
            this.colPID.Visible = false;
            // 
            // colName
            // 
            this.colName.HeaderText = "Name";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            // 
            // colPRO
            // 
            this.colPRO.HeaderText = "PRO";
            this.colPRO.Name = "colPRO";
            this.colPRO.ReadOnly = true;
            // 
            // colPPO
            // 
            this.colPPO.HeaderText = "PPO";
            this.colPPO.Name = "colPPO";
            this.colPPO.ReadOnly = true;
            // 
            // colPS
            // 
            this.colPS.HeaderText = "PS";
            this.colPS.Name = "colPS";
            this.colPS.ReadOnly = true;
            // 
            // colHVTSLT
            // 
            this.colHVTSLT.HeaderText = "HVT/SLT";
            this.colHVTSLT.Name = "colHVTSLT";
            this.colHVTSLT.ReadOnly = true;
            // 
            // colClassification
            // 
            this.colClassification.HeaderText = "Classification";
            this.colClassification.Name = "colClassification";
            this.colClassification.ReadOnly = true;
            // 
            // colStatus
            // 
            this.colStatus.HeaderText = "Status";
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            // 
            // colDateEntry
            // 
            this.colDateEntry.HeaderText = "Date of Entry";
            this.colDateEntry.Name = "colDateEntry";
            this.colDateEntry.ReadOnly = true;
            // 
            // colListedByDI
            // 
            this.colListedByDI.HeaderText = "Listed by DI";
            this.colListedByDI.Name = "colListedByDI";
            this.colListedByDI.ReadOnly = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Dock = System.Windows.Forms.DockStyle.Top;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(84)))), ((int)(((byte)(7)))));
            this.label9.Location = new System.Drawing.Point(2, 2);
            this.label9.Name = "label9";
            this.label9.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.label9.Size = new System.Drawing.Size(251, 35);
            this.label9.TabIndex = 1;
            this.label9.Text = "Filtered Data: All Data Entry";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btn_loadPersonalities
            // 
            this.btn_loadPersonalities.FlatAppearance.BorderSize = 0;
            this.btn_loadPersonalities.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
            this.btn_loadPersonalities.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_loadPersonalities.Location = new System.Drawing.Point(1016, 7);
            this.btn_loadPersonalities.Name = "btn_loadPersonalities";
            this.btn_loadPersonalities.Size = new System.Drawing.Size(75, 23);
            this.btn_loadPersonalities.TabIndex = 2;
            this.btn_loadPersonalities.Text = "Refresh";
            this.btn_loadPersonalities.UseVisualStyleBackColor = true;
            this.btn_loadPersonalities.Click += new System.EventHandler(this.btn_loadPersonalities_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 600);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainWindow";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.Shown += new System.EventHandler(this.MainWindow_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPersonalities)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbDevices;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.ListBox lstUsers;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Button btnView;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Label lblGBMSGUIVersion;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txt_alias;
        private System.Windows.Forms.TextBox txt_middlename;
        private System.Windows.Forms.TextBox txt_lastname;
        private System.Windows.Forms.TextBox txt_firstname;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbMPSPS;
        private System.Windows.Forms.ComboBox cmbPPOCPODistrict;
        private System.Windows.Forms.ComboBox cmbPRORegion;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.DataGridView dgvPersonalities;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPRO;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPPO;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPS;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHVTSLT;
        private System.Windows.Forms.DataGridViewTextBoxColumn colClassification;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDateEntry;
        private System.Windows.Forms.DataGridViewTextBoxColumn colListedByDI;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.DateTimePicker dtp_birthday;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox cmb_qualifier;
        private System.Windows.Forms.Button btn_loadPersonalities;
    }
}