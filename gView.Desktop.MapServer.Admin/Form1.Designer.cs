namespace gView.Desktop.MapServer.Admin
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnInstall = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbStartType = new System.Windows.Forms.ComboBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.picStatus = new System.Windows.Forms.PictureBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btnRefreshConfig = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.txtOnlineresource = new System.Windows.Forms.TextBox();
            this.txtTilecachePath = new System.Windows.Forms.TextBox();
            this.txtOutputPath = new System.Windows.Forms.TextBox();
            this.txtOutputUrl = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.chkLogErrors = new System.Windows.Forms.CheckBox();
            this.chkLogRequestDetails = new System.Windows.Forms.CheckBox();
            this.chkLogRequests = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnChangeStartType = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picStatus)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnInstall
            // 
            this.btnInstall.Location = new System.Drawing.Point(21, 144);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(115, 38);
            this.btnInstall.TabIndex = 0;
            this.btnInstall.Text = "Install Windows Service";
            this.btnInstall.UseVisualStyleBackColor = true;
            this.btnInstall.Visible = false;
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(55, 105);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(16, 13);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "...";
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(166, 355);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(156, 25);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(-2, 125);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(324, 224);
            this.tabControl1.TabIndex = 6;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.btnChangeStartType);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.cmbStartType);
            this.tabPage1.Controls.Add(this.btnInstall);
            this.tabPage1.Controls.Add(this.btnStart);
            this.tabPage1.Controls.Add(this.lblStatus);
            this.tabPage1.Controls.Add(this.btnRefresh);
            this.tabPage1.Controls.Add(this.picStatus);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(316, 198);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Status";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(18, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Starttype:";
            // 
            // cmbStartType
            // 
            this.cmbStartType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStartType.FormattingEnabled = true;
            this.cmbStartType.Location = new System.Drawing.Point(21, 31);
            this.cmbStartType.Name = "cmbStartType";
            this.cmbStartType.Size = new System.Drawing.Size(166, 21);
            this.cmbStartType.TabIndex = 6;
            // 
            // btnStart
            // 
            this.btnStart.Image = global::gView.Desktop.MapServer.Admin.Properties.Resources._16_arrow_right;
            this.btnStart.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStart.Location = new System.Drawing.Point(21, 58);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(166, 25);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start Service";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Image = global::gView.Desktop.MapServer.Admin.Properties.Resources.refresh;
            this.btnRefresh.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRefresh.Location = new System.Drawing.Point(199, 159);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(106, 23);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // picStatus
            // 
            this.picStatus.Image = global::gView.Desktop.MapServer.Admin.Properties.Resources._16_message_warn;
            this.picStatus.Location = new System.Drawing.Point(30, 102);
            this.picStatus.Name = "picStatus";
            this.picStatus.Size = new System.Drawing.Size(19, 19);
            this.picStatus.TabIndex = 3;
            this.picStatus.TabStop = false;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.btnRefreshConfig);
            this.tabPage3.Controls.Add(this.button1);
            this.tabPage3.Controls.Add(this.txtOnlineresource);
            this.tabPage3.Controls.Add(this.txtTilecachePath);
            this.tabPage3.Controls.Add(this.txtOutputPath);
            this.tabPage3.Controls.Add(this.txtOutputUrl);
            this.tabPage3.Controls.Add(this.label4);
            this.tabPage3.Controls.Add(this.label3);
            this.tabPage3.Controls.Add(this.label2);
            this.tabPage3.Controls.Add(this.label1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(316, 198);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Config";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btnRefreshConfig
            // 
            this.btnRefreshConfig.Image = global::gView.Desktop.MapServer.Admin.Properties.Resources.refresh;
            this.btnRefreshConfig.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRefreshConfig.Location = new System.Drawing.Point(139, 117);
            this.btnRefreshConfig.Name = "btnRefreshConfig";
            this.btnRefreshConfig.Size = new System.Drawing.Size(87, 23);
            this.btnRefreshConfig.TabIndex = 9;
            this.btnRefreshConfig.Text = "Refresh";
            this.btnRefreshConfig.UseVisualStyleBackColor = true;
            this.btnRefreshConfig.Click += new System.EventHandler(this.btnRefreshConfig_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(232, 117);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(78, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Edit...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtOnlineresource
            // 
            this.txtOnlineresource.Enabled = false;
            this.txtOnlineresource.Location = new System.Drawing.Point(85, 91);
            this.txtOnlineresource.Name = "txtOnlineresource";
            this.txtOnlineresource.Size = new System.Drawing.Size(225, 20);
            this.txtOnlineresource.TabIndex = 7;
            // 
            // txtTilecachePath
            // 
            this.txtTilecachePath.Location = new System.Drawing.Point(85, 65);
            this.txtTilecachePath.Name = "txtTilecachePath";
            this.txtTilecachePath.ReadOnly = true;
            this.txtTilecachePath.Size = new System.Drawing.Size(225, 20);
            this.txtTilecachePath.TabIndex = 6;
            // 
            // txtOutputPath
            // 
            this.txtOutputPath.Enabled = false;
            this.txtOutputPath.Location = new System.Drawing.Point(85, 39);
            this.txtOutputPath.Name = "txtOutputPath";
            this.txtOutputPath.Size = new System.Drawing.Size(225, 20);
            this.txtOutputPath.TabIndex = 5;
            // 
            // txtOutputUrl
            // 
            this.txtOutputUrl.Enabled = false;
            this.txtOutputUrl.Location = new System.Drawing.Point(85, 11);
            this.txtOutputUrl.Name = "txtOutputUrl";
            this.txtOutputUrl.Size = new System.Drawing.Size(225, 20);
            this.txtOutputUrl.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 94);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(81, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Onlineresource:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Tilecache Path:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Output Url:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Output Path:";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.chkLogErrors);
            this.tabPage2.Controls.Add(this.chkLogRequestDetails);
            this.tabPage2.Controls.Add(this.chkLogRequests);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(316, 198);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Logging";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // chkLogErrors
            // 
            this.chkLogErrors.AutoSize = true;
            this.chkLogErrors.Location = new System.Drawing.Point(15, 61);
            this.chkLogErrors.Name = "chkLogErrors";
            this.chkLogErrors.Size = new System.Drawing.Size(74, 17);
            this.chkLogErrors.TabIndex = 2;
            this.chkLogErrors.Text = "Log Errors";
            this.chkLogErrors.UseVisualStyleBackColor = true;
            // 
            // chkLogRequestDetails
            // 
            this.chkLogRequestDetails.AutoSize = true;
            this.chkLogRequestDetails.Location = new System.Drawing.Point(15, 38);
            this.chkLogRequestDetails.Name = "chkLogRequestDetails";
            this.chkLogRequestDetails.Size = new System.Drawing.Size(122, 17);
            this.chkLogRequestDetails.TabIndex = 1;
            this.chkLogRequestDetails.Text = "Log Request Details";
            this.chkLogRequestDetails.UseVisualStyleBackColor = true;
            // 
            // chkLogRequests
            // 
            this.chkLogRequests.AutoSize = true;
            this.chkLogRequests.Location = new System.Drawing.Point(15, 15);
            this.chkLogRequests.Name = "chkLogRequests";
            this.chkLogRequests.Size = new System.Drawing.Size(92, 17);
            this.chkLogRequests.TabIndex = 0;
            this.chkLogRequests.Text = "Log Requests";
            this.chkLogRequests.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = global::gView.Desktop.MapServer.Admin.Properties.Resources.gViewOS1;
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel1.Location = new System.Drawing.Point(-1, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(324, 118);
            this.panel1.TabIndex = 7;
            // 
            // btnChangeStartType
            // 
            this.btnChangeStartType.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnChangeStartType.Location = new System.Drawing.Point(199, 31);
            this.btnChangeStartType.Name = "btnChangeStartType";
            this.btnChangeStartType.Size = new System.Drawing.Size(106, 23);
            this.btnChangeStartType.TabIndex = 8;
            this.btnChangeStartType.Text = "Change";
            this.btnChangeStartType.UseVisualStyleBackColor = true;
            this.btnChangeStartType.Click += new System.EventHandler(this.btnChangeStartType_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(324, 392);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MapServer Administrator";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picStatus)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.PictureBox picStatus;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.CheckBox chkLogRequests;
        private System.Windows.Forms.CheckBox chkLogErrors;
        private System.Windows.Forms.CheckBox chkLogRequestDetails;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TextBox txtOnlineresource;
        private System.Windows.Forms.TextBox txtTilecachePath;
        private System.Windows.Forms.TextBox txtOutputPath;
        private System.Windows.Forms.TextBox txtOutputUrl;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnRefreshConfig;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbStartType;
        private System.Windows.Forms.Button btnChangeStartType;
    }
}

