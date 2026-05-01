namespace AdminHelper
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            rtbLogger = new RichTextBox();
            btnSaveLog = new Button();
            btnClearLog = new Button();
            btnCopyLog = new Button();
            btnAdminRights = new Button();
            label6 = new Label();
            lblRights = new Label();
            tabControl = new TabControl();
            tabPageMonitoring = new TabPage();
            gbCPU = new GroupBox();
            tbMonitorCPU = new TextBox();
            gbRAM = new GroupBox();
            tbMonitoringRAM = new TextBox();
            gbGPU = new GroupBox();
            tbMonitoringGPU = new TextBox();
            gbDisk = new GroupBox();
            tbMonitoringHDD = new TextBox();
            btnStopMonitoring = new Button();
            btnStartMonitoring = new Button();
            tabPageInfoProccess = new TabPage();
            dataGridViewProcesses = new DataGridView();
            colProcessName = new DataGridViewTextBoxColumn();
            colProcessMemory = new DataGridViewTextBoxColumn();
            colProcessDescription = new DataGridViewTextBoxColumn();
            colProcessStatus = new DataGridViewTextBoxColumn();
            colProcessType = new DataGridViewTextBoxColumn();
            tabPageInfoService = new TabPage();
            dataGridViewServices = new DataGridView();
            colServiceName = new DataGridViewTextBoxColumn();
            colServiceDisplayName = new DataGridViewTextBoxColumn();
            colServiceStatus = new DataGridViewTextBoxColumn();
            colServiceType = new DataGridViewTextBoxColumn();
            tabPageScript = new TabPage();
            dataGridViewScripts = new DataGridView();
            colScriptName = new DataGridViewTextBoxColumn();
            colScriptDescription = new DataGridViewTextBoxColumn();
            colScriptStatus = new DataGridViewTextBoxColumn();
            colScriptType = new DataGridViewTextBoxColumn();
            colScriptControl = new DataGridViewButtonColumn();
            pnlScriptBottom = new Panel();
            btnAddScript = new Button();
            btnBackupScript = new Button();
            gbShutdown = new GroupBox();
            nudShutdownMinutes = new NumericUpDown();
            lblShutdownMins = new Label();
            btnOffPc = new Button();
            btnCancelShutdown = new Button();
            lblShutdownStatus = new Label();
            tabControl.SuspendLayout();
            tabPageMonitoring.SuspendLayout();
            gbCPU.SuspendLayout();
            gbRAM.SuspendLayout();
            gbGPU.SuspendLayout();
            gbDisk.SuspendLayout();
            tabPageInfoProccess.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewProcesses).BeginInit();
            tabPageInfoService.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewServices).BeginInit();
            tabPageScript.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewScripts).BeginInit();
            pnlScriptBottom.SuspendLayout();
            gbShutdown.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudShutdownMinutes).BeginInit();
            SuspendLayout();
            // 
            // rtbLogger
            // 
            rtbLogger.BackColor = Color.FromArgb(20, 20, 20);
            rtbLogger.BorderStyle = BorderStyle.FixedSingle;
            rtbLogger.Font = new Font("Lucida Console", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 204);
            rtbLogger.ForeColor = Color.LightGreen;
            rtbLogger.Location = new Point(12, 486);
            rtbLogger.Name = "rtbLogger";
            rtbLogger.ReadOnly = true;
            rtbLogger.Size = new Size(1060, 156);
            rtbLogger.TabIndex = 0;
            rtbLogger.Text = "";
            // 
            // btnSaveLog
            // 
            btnSaveLog.Location = new Point(997, 648);
            btnSaveLog.Name = "btnSaveLog";
            btnSaveLog.Size = new Size(75, 26);
            btnSaveLog.TabIndex = 1;
            btnSaveLog.Text = "💾 Save";
            btnSaveLog.UseVisualStyleBackColor = true;
            btnSaveLog.Click += btnSaveLog_Click;
            // 
            // btnClearLog
            // 
            btnClearLog.Location = new Point(836, 648);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(75, 26);
            btnClearLog.TabIndex = 2;
            btnClearLog.Text = "🗑 Clear";
            btnClearLog.UseVisualStyleBackColor = true;
            btnClearLog.Click += btnClearLog_Click;
            // 
            // btnCopyLog
            // 
            btnCopyLog.Location = new Point(917, 648);
            btnCopyLog.Name = "btnCopyLog";
            btnCopyLog.Size = new Size(75, 26);
            btnCopyLog.TabIndex = 3;
            btnCopyLog.Text = "📋 Copy";
            btnCopyLog.UseVisualStyleBackColor = true;
            btnCopyLog.Click += btnCopyLog_Click;
            // 
            // btnAdminRights
            // 
            btnAdminRights.BackColor = Color.SteelBlue;
            btnAdminRights.ForeColor = Color.White;
            btnAdminRights.Location = new Point(12, 648);
            btnAdminRights.Name = "btnAdminRights";
            btnAdminRights.Size = new Size(120, 26);
            btnAdminRights.TabIndex = 8;
            btnAdminRights.Text = "🔑 Run as Admin";
            btnAdminRights.UseVisualStyleBackColor = false;
            btnAdminRights.Click += btnAdminRights_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label6.Location = new Point(12, 16);
            label6.Name = "label6";
            label6.Size = new Size(45, 15);
            label6.TabIndex = 9;
            label6.Text = "Rights:";
            // 
            // lblRights
            // 
            lblRights.AutoSize = true;
            lblRights.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblRights.ForeColor = Color.Gray;
            lblRights.Location = new Point(65, 16);
            lblRights.Name = "lblRights";
            lblRights.Size = new Size(67, 15);
            lblRights.TabIndex = 10;
            lblRights.Text = "Checking...";
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPageMonitoring);
            tabControl.Controls.Add(tabPageInfoProccess);
            tabControl.Controls.Add(tabPageInfoService);
            tabControl.Controls.Add(tabPageScript);
            tabControl.Font = new Font("Segoe UI", 9F);
            tabControl.Location = new Point(12, 41);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1060, 439);
            tabControl.TabIndex = 11;
            // 
            // tabPageMonitoring
            // 
            tabPageMonitoring.Controls.Add(gbCPU);
            tabPageMonitoring.Controls.Add(gbRAM);
            tabPageMonitoring.Controls.Add(gbGPU);
            tabPageMonitoring.Controls.Add(gbDisk);
            tabPageMonitoring.Controls.Add(btnStopMonitoring);
            tabPageMonitoring.Controls.Add(btnStartMonitoring);
            tabPageMonitoring.Location = new Point(4, 24);
            tabPageMonitoring.Name = "tabPageMonitoring";
            tabPageMonitoring.Padding = new Padding(3);
            tabPageMonitoring.Size = new Size(1052, 411);
            tabPageMonitoring.TabIndex = 0;
            tabPageMonitoring.Text = "📊 Monitoring";
            tabPageMonitoring.UseVisualStyleBackColor = true;
            // 
            // gbCPU
            // 
            gbCPU.Controls.Add(tbMonitorCPU);
            gbCPU.Location = new Point(6, 6);
            gbCPU.Name = "gbCPU";
            gbCPU.Size = new Size(230, 400);
            gbCPU.TabIndex = 0;
            gbCPU.TabStop = false;
            gbCPU.Text = "⚙ CPU";
            // 
            // tbMonitorCPU
            // 
            tbMonitorCPU.BackColor = Color.FromArgb(240, 240, 240);
            tbMonitorCPU.Dock = DockStyle.Fill;
            tbMonitorCPU.Font = new Font("Consolas", 9.5F);
            tbMonitorCPU.Location = new Point(3, 19);
            tbMonitorCPU.Multiline = true;
            tbMonitorCPU.Name = "tbMonitorCPU";
            tbMonitorCPU.ReadOnly = true;
            tbMonitorCPU.ScrollBars = ScrollBars.Vertical;
            tbMonitorCPU.Size = new Size(224, 378);
            tbMonitorCPU.TabIndex = 0;
            tbMonitorCPU.Text = "— Мониторинг не запущен —";
            // 
            // gbRAM
            // 
            gbRAM.Controls.Add(tbMonitoringRAM);
            gbRAM.Location = new Point(478, 6);
            gbRAM.Name = "gbRAM";
            gbRAM.Size = new Size(230, 400);
            gbRAM.TabIndex = 1;
            gbRAM.TabStop = false;
            gbRAM.Text = "\U0001f9e0 RAM";
            // 
            // tbMonitoringRAM
            // 
            tbMonitoringRAM.BackColor = Color.FromArgb(240, 240, 240);
            tbMonitoringRAM.Dock = DockStyle.Fill;
            tbMonitoringRAM.Font = new Font("Consolas", 9.5F);
            tbMonitoringRAM.Location = new Point(3, 19);
            tbMonitoringRAM.Multiline = true;
            tbMonitoringRAM.Name = "tbMonitoringRAM";
            tbMonitoringRAM.ReadOnly = true;
            tbMonitoringRAM.ScrollBars = ScrollBars.Vertical;
            tbMonitoringRAM.Size = new Size(224, 378);
            tbMonitoringRAM.TabIndex = 1;
            tbMonitoringRAM.Text = "— Мониторинг не запущен —";
            // 
            // gbGPU
            // 
            gbGPU.Controls.Add(tbMonitoringGPU);
            gbGPU.Location = new Point(242, 6);
            gbGPU.Name = "gbGPU";
            gbGPU.Size = new Size(230, 400);
            gbGPU.TabIndex = 2;
            gbGPU.TabStop = false;
            gbGPU.Text = "🎮 GPU";
            // 
            // tbMonitoringGPU
            // 
            tbMonitoringGPU.BackColor = Color.FromArgb(240, 240, 240);
            tbMonitoringGPU.Dock = DockStyle.Fill;
            tbMonitoringGPU.Font = new Font("Consolas", 9.5F);
            tbMonitoringGPU.Location = new Point(3, 19);
            tbMonitoringGPU.Multiline = true;
            tbMonitoringGPU.Name = "tbMonitoringGPU";
            tbMonitoringGPU.ReadOnly = true;
            tbMonitoringGPU.ScrollBars = ScrollBars.Vertical;
            tbMonitoringGPU.Size = new Size(224, 378);
            tbMonitoringGPU.TabIndex = 2;
            tbMonitoringGPU.Text = "— Мониторинг не запущен —";
            // 
            // gbDisk
            // 
            gbDisk.Controls.Add(tbMonitoringHDD);
            gbDisk.Location = new Point(714, 6);
            gbDisk.Name = "gbDisk";
            gbDisk.Size = new Size(230, 400);
            gbDisk.TabIndex = 3;
            gbDisk.TabStop = false;
            gbDisk.Text = "💾 Disk";
            // 
            // tbMonitoringHDD
            // 
            tbMonitoringHDD.BackColor = Color.FromArgb(240, 240, 240);
            tbMonitoringHDD.Dock = DockStyle.Fill;
            tbMonitoringHDD.Font = new Font("Consolas", 9.5F);
            tbMonitoringHDD.Location = new Point(3, 19);
            tbMonitoringHDD.Multiline = true;
            tbMonitoringHDD.Name = "tbMonitoringHDD";
            tbMonitoringHDD.ReadOnly = true;
            tbMonitoringHDD.ScrollBars = ScrollBars.Vertical;
            tbMonitoringHDD.Size = new Size(224, 378);
            tbMonitoringHDD.TabIndex = 3;
            tbMonitoringHDD.Text = "— Мониторинг не запущен —";
            // 
            // btnStopMonitoring
            // 
            btnStopMonitoring.BackColor = Color.Gray;
            btnStopMonitoring.Enabled = false;
            btnStopMonitoring.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnStopMonitoring.ForeColor = Color.White;
            btnStopMonitoring.Location = new Point(950, 336);
            btnStopMonitoring.Name = "btnStopMonitoring";
            btnStopMonitoring.Size = new Size(96, 32);
            btnStopMonitoring.TabIndex = 4;
            btnStopMonitoring.Text = "⏹ Stop";
            btnStopMonitoring.UseVisualStyleBackColor = false;
            btnStopMonitoring.Click += btnStopMonitoring_Click;
            // 
            // btnStartMonitoring
            // 
            btnStartMonitoring.BackColor = Color.SeaGreen;
            btnStartMonitoring.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnStartMonitoring.ForeColor = Color.White;
            btnStartMonitoring.Location = new Point(950, 371);
            btnStartMonitoring.Name = "btnStartMonitoring";
            btnStartMonitoring.Size = new Size(96, 32);
            btnStartMonitoring.TabIndex = 5;
            btnStartMonitoring.Text = "▶ Start";
            btnStartMonitoring.UseVisualStyleBackColor = false;
            btnStartMonitoring.Click += btnStartMonitoring_Click;
            // 
            // tabPageInfoProccess
            // 
            tabPageInfoProccess.Controls.Add(dataGridViewProcesses);
            tabPageInfoProccess.Location = new Point(4, 24);
            tabPageInfoProccess.Name = "tabPageInfoProccess";
            tabPageInfoProccess.Padding = new Padding(3);
            tabPageInfoProccess.Size = new Size(1052, 449);
            tabPageInfoProccess.TabIndex = 1;
            tabPageInfoProccess.Text = "⚙ Processes";
            tabPageInfoProccess.UseVisualStyleBackColor = true;
            // 
            // dataGridViewProcesses
            // 
            dataGridViewProcesses.AllowUserToAddRows = false;
            dataGridViewProcesses.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(248, 248, 255);
            dataGridViewProcesses.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridViewProcesses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Control;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dataGridViewProcesses.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridViewProcesses.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewProcesses.Columns.AddRange(new DataGridViewColumn[] { colProcessName, colProcessMemory, colProcessDescription, colProcessStatus, colProcessType });
            dataGridViewProcesses.Dock = DockStyle.Fill;
            dataGridViewProcesses.Font = new Font("Consolas", 8.5F);
            dataGridViewProcesses.Location = new Point(3, 3);
            dataGridViewProcesses.Name = "dataGridViewProcesses";
            dataGridViewProcesses.ReadOnly = true;
            dataGridViewProcesses.RowHeadersVisible = false;
            dataGridViewProcesses.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewProcesses.Size = new Size(1046, 443);
            dataGridViewProcesses.TabIndex = 0;
            // 
            // colProcessName
            // 
            colProcessName.FillWeight = 25F;
            colProcessName.HeaderText = "Process Name";
            colProcessName.Name = "colProcessName";
            colProcessName.ReadOnly = true;
            // 
            // colProcessMemory
            // 
            colProcessMemory.FillWeight = 15F;
            colProcessMemory.HeaderText = "Memory (MB)";
            colProcessMemory.Name = "colProcessMemory";
            colProcessMemory.ReadOnly = true;
            // 
            // colProcessDescription
            // 
            colProcessDescription.FillWeight = 35F;
            colProcessDescription.HeaderText = "Description";
            colProcessDescription.Name = "colProcessDescription";
            colProcessDescription.ReadOnly = true;
            // 
            // colProcessStatus
            // 
            colProcessStatus.FillWeight = 15F;
            colProcessStatus.HeaderText = "Status";
            colProcessStatus.Name = "colProcessStatus";
            colProcessStatus.ReadOnly = true;
            // 
            // colProcessType
            // 
            colProcessType.FillWeight = 10F;
            colProcessType.HeaderText = "Type";
            colProcessType.Name = "colProcessType";
            colProcessType.ReadOnly = true;
            // 
            // tabPageInfoService
            // 
            tabPageInfoService.Controls.Add(dataGridViewServices);
            tabPageInfoService.Location = new Point(4, 24);
            tabPageInfoService.Name = "tabPageInfoService";
            tabPageInfoService.Padding = new Padding(3);
            tabPageInfoService.Size = new Size(1052, 449);
            tabPageInfoService.TabIndex = 2;
            tabPageInfoService.Text = "🔧 Services";
            tabPageInfoService.UseVisualStyleBackColor = true;
            // 
            // dataGridViewServices
            // 
            dataGridViewServices.AllowUserToAddRows = false;
            dataGridViewServices.AllowUserToDeleteRows = false;
            dataGridViewCellStyle3.BackColor = Color.FromArgb(248, 248, 255);
            dataGridViewServices.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            dataGridViewServices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = SystemColors.Control;
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle4.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            dataGridViewServices.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridViewServices.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewServices.Columns.AddRange(new DataGridViewColumn[] { colServiceName, colServiceDisplayName, colServiceStatus, colServiceType });
            dataGridViewServices.Dock = DockStyle.Fill;
            dataGridViewServices.Font = new Font("Consolas", 8.5F);
            dataGridViewServices.Location = new Point(3, 3);
            dataGridViewServices.Name = "dataGridViewServices";
            dataGridViewServices.ReadOnly = true;
            dataGridViewServices.RowHeadersVisible = false;
            dataGridViewServices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewServices.Size = new Size(1046, 443);
            dataGridViewServices.TabIndex = 0;
            // 
            // colServiceName
            // 
            colServiceName.FillWeight = 25F;
            colServiceName.HeaderText = "Service Name";
            colServiceName.Name = "colServiceName";
            colServiceName.ReadOnly = true;
            // 
            // colServiceDisplayName
            // 
            colServiceDisplayName.FillWeight = 40F;
            colServiceDisplayName.HeaderText = "Display Name";
            colServiceDisplayName.Name = "colServiceDisplayName";
            colServiceDisplayName.ReadOnly = true;
            // 
            // colServiceStatus
            // 
            colServiceStatus.FillWeight = 20F;
            colServiceStatus.HeaderText = "Status";
            colServiceStatus.Name = "colServiceStatus";
            colServiceStatus.ReadOnly = true;
            // 
            // colServiceType
            // 
            colServiceType.FillWeight = 15F;
            colServiceType.HeaderText = "Start Type";
            colServiceType.Name = "colServiceType";
            colServiceType.ReadOnly = true;
            // 
            // tabPageScript
            // 
            tabPageScript.Controls.Add(dataGridViewScripts);
            tabPageScript.Controls.Add(pnlScriptBottom);
            tabPageScript.Location = new Point(4, 24);
            tabPageScript.Name = "tabPageScript";
            tabPageScript.Padding = new Padding(3);
            tabPageScript.Size = new Size(1052, 449);
            tabPageScript.TabIndex = 3;
            tabPageScript.Text = "📜 Scripts";
            tabPageScript.UseVisualStyleBackColor = true;
            // 
            // dataGridViewScripts
            // 
            dataGridViewScripts.AllowUserToAddRows = false;
            dataGridViewScripts.AllowUserToDeleteRows = false;
            dataGridViewCellStyle5.BackColor = Color.FromArgb(248, 248, 255);
            dataGridViewScripts.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
            dataGridViewScripts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = SystemColors.Control;
            dataGridViewCellStyle6.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle6.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = DataGridViewTriState.True;
            dataGridViewScripts.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle6;
            dataGridViewScripts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewScripts.Columns.AddRange(new DataGridViewColumn[] { colScriptName, colScriptDescription, colScriptStatus, colScriptType, colScriptControl });
            dataGridViewScripts.Font = new Font("Segoe UI", 9F);
            dataGridViewScripts.Location = new Point(3, 3);
            dataGridViewScripts.Name = "dataGridViewScripts";
            dataGridViewScripts.ReadOnly = true;
            dataGridViewScripts.RowHeadersVisible = false;
            dataGridViewScripts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewScripts.Size = new Size(1046, 358);
            dataGridViewScripts.TabIndex = 0;
            dataGridViewScripts.CellContentClick += DataGridViewScripts_CellContentClick;
            // 
            // colScriptName
            // 
            colScriptName.FillWeight = 18F;
            colScriptName.HeaderText = "Script Name";
            colScriptName.Name = "colScriptName";
            colScriptName.ReadOnly = true;
            // 
            // colScriptDescription
            // 
            colScriptDescription.FillWeight = 40F;
            colScriptDescription.HeaderText = "Description";
            colScriptDescription.Name = "colScriptDescription";
            colScriptDescription.ReadOnly = true;
            // 
            // colScriptStatus
            // 
            colScriptStatus.FillWeight = 12F;
            colScriptStatus.HeaderText = "Status";
            colScriptStatus.Name = "colScriptStatus";
            colScriptStatus.ReadOnly = true;
            // 
            // colScriptType
            // 
            colScriptType.FillWeight = 12F;
            colScriptType.HeaderText = "Type";
            colScriptType.Name = "colScriptType";
            colScriptType.ReadOnly = true;
            // 
            // colScriptControl
            // 
            colScriptControl.FillWeight = 18F;
            colScriptControl.HeaderText = "Action";
            colScriptControl.Name = "colScriptControl";
            colScriptControl.ReadOnly = true;
            // 
            // pnlScriptBottom
            // 
            pnlScriptBottom.Controls.Add(btnAddScript);
            pnlScriptBottom.Controls.Add(btnBackupScript);
            pnlScriptBottom.Controls.Add(gbShutdown);
            pnlScriptBottom.Location = new Point(3, 364);
            pnlScriptBottom.Name = "pnlScriptBottom";
            pnlScriptBottom.Size = new Size(1046, 82);
            pnlScriptBottom.TabIndex = 1;
            // 
            // btnAddScript
            // 
            btnAddScript.Font = new Font("Segoe UI", 9F);
            btnAddScript.Location = new Point(0, 10);
            btnAddScript.Name = "btnAddScript";
            btnAddScript.Size = new Size(110, 32);
            btnAddScript.TabIndex = 0;
            btnAddScript.Text = "➕ Add Script";
            btnAddScript.UseVisualStyleBackColor = true;
            btnAddScript.Click += BtnAddScript_Click;
            // 
            // btnBackupScript
            // 
            btnBackupScript.Font = new Font("Segoe UI", 9F);
            btnBackupScript.Location = new Point(118, 10);
            btnBackupScript.Name = "btnBackupScript";
            btnBackupScript.Size = new Size(130, 32);
            btnBackupScript.TabIndex = 1;
            btnBackupScript.Text = "📁 Backup Folder";
            btnBackupScript.UseVisualStyleBackColor = true;
            btnBackupScript.Click += BtnBackupScript_Click;
            // 
            // gbShutdown
            // 
            gbShutdown.Controls.Add(nudShutdownMinutes);
            gbShutdown.Controls.Add(lblShutdownMins);
            gbShutdown.Controls.Add(btnOffPc);
            gbShutdown.Controls.Add(btnCancelShutdown);
            gbShutdown.Controls.Add(lblShutdownStatus);
            gbShutdown.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gbShutdown.Location = new Point(256, 0);
            gbShutdown.Name = "gbShutdown";
            gbShutdown.Size = new Size(420, 82);
            gbShutdown.TabIndex = 2;
            gbShutdown.TabStop = false;
            gbShutdown.Text = "\u23fb Shutdown Control";
            // 
            // nudShutdownMinutes
            // 
            nudShutdownMinutes.Font = new Font("Segoe UI", 9F);
            nudShutdownMinutes.Location = new Point(10, 22);
            nudShutdownMinutes.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            nudShutdownMinutes.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudShutdownMinutes.Name = "nudShutdownMinutes";
            nudShutdownMinutes.Size = new Size(50, 23);
            nudShutdownMinutes.TabIndex = 0;
            nudShutdownMinutes.Value = new decimal(new int[] { 5, 0, 0, 0 });
            nudShutdownMinutes.ValueChanged += nudShutdownMinutes_ValueChanged;
            // 
            // lblShutdownMins
            // 
            lblShutdownMins.AutoSize = true;
            lblShutdownMins.Font = new Font("Segoe UI", 9F);
            lblShutdownMins.Location = new Point(65, 25);
            lblShutdownMins.Name = "lblShutdownMins";
            lblShutdownMins.Size = new Size(30, 15);
            lblShutdownMins.TabIndex = 1;
            lblShutdownMins.Text = "мин";
            // 
            // btnOffPc
            // 
            btnOffPc.BackColor = Color.DarkOrange;
            btnOffPc.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnOffPc.ForeColor = Color.White;
            btnOffPc.Location = new Point(96, 18);
            btnOffPc.Name = "btnOffPc";
            btnOffPc.Size = new Size(140, 34);
            btnOffPc.TabIndex = 1;
            btnOffPc.Text = "\u23fb OFF PC (5 мин)";
            btnOffPc.UseVisualStyleBackColor = false;
            btnOffPc.Click += BtnOffPc_Click;
            // 
            // btnCancelShutdown
            // 
            btnCancelShutdown.BackColor = Color.Gray;
            btnCancelShutdown.Enabled = false;
            btnCancelShutdown.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCancelShutdown.ForeColor = Color.White;
            btnCancelShutdown.Location = new Point(244, 18);
            btnCancelShutdown.Name = "btnCancelShutdown";
            btnCancelShutdown.Size = new Size(110, 34);
            btnCancelShutdown.TabIndex = 2;
            btnCancelShutdown.Text = "✖ CANCEL";
            btnCancelShutdown.UseVisualStyleBackColor = false;
            btnCancelShutdown.Click += BtnCancelShutdown_Click;
            // 
            // lblShutdownStatus
            // 
            lblShutdownStatus.Font = new Font("Segoe UI", 8.5F);
            lblShutdownStatus.ForeColor = Color.SeaGreen;
            lblShutdownStatus.Location = new Point(96, 56);
            lblShutdownStatus.Name = "lblShutdownStatus";
            lblShutdownStatus.Size = new Size(260, 18);
            lblShutdownStatus.TabIndex = 3;
            lblShutdownStatus.Text = "Нет активного таймера";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1084, 686);
            Controls.Add(tabControl);
            Controls.Add(lblRights);
            Controls.Add(label6);
            Controls.Add(btnAdminRights);
            Controls.Add(btnCopyLog);
            Controls.Add(btnClearLog);
            Controls.Add(btnSaveLog);
            Controls.Add(rtbLogger);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            Text = "Admin Helper";
            Load += MainForm_Load;
            tabControl.ResumeLayout(false);
            tabPageMonitoring.ResumeLayout(false);
            gbCPU.ResumeLayout(false);
            gbCPU.PerformLayout();
            gbRAM.ResumeLayout(false);
            gbRAM.PerformLayout();
            gbGPU.ResumeLayout(false);
            gbGPU.PerformLayout();
            gbDisk.ResumeLayout(false);
            gbDisk.PerformLayout();
            tabPageInfoProccess.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewProcesses).EndInit();
            tabPageInfoService.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewServices).EndInit();
            tabPageScript.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewScripts).EndInit();
            pnlScriptBottom.ResumeLayout(false);
            gbShutdown.ResumeLayout(false);
            gbShutdown.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudShutdownMinutes).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion

        // ── Поля ─────────────────────────────────────────────────────────
        private RichTextBox rtbLogger;
        private Button btnSaveLog;
        private Button btnClearLog;
        private Button btnCopyLog;
        private Button btnAdminRights;
        private Label label6;
        private Label lblRights;
        private TabControl tabControl;

        // Вкладка Monitoring
        private TabPage tabPageMonitoring;
        private GroupBox gbCPU;
        private TextBox tbMonitorCPU;
        private GroupBox gbRAM;
        private TextBox tbMonitoringRAM;
        private GroupBox gbGPU;
        private TextBox tbMonitoringGPU;
        private GroupBox gbDisk;
        private TextBox tbMonitoringHDD;
        private Button btnStartMonitoring;
        private Button btnStopMonitoring;

        // Вкладка Processes
        private TabPage tabPageInfoProccess;
        private DataGridView dataGridViewProcesses;
        private DataGridViewTextBoxColumn colProcessName;
        private DataGridViewTextBoxColumn colProcessMemory;
        private DataGridViewTextBoxColumn colProcessDescription;
        private DataGridViewTextBoxColumn colProcessStatus;
        private DataGridViewTextBoxColumn colProcessType;

        // Вкладка Services
        // BugFix: colServiceMemory убрана — в логике всегда 0, не несёт смысла
        private TabPage tabPageInfoService;
        private DataGridView dataGridViewServices;
        private DataGridViewTextBoxColumn colServiceName;
        private DataGridViewTextBoxColumn colServiceDisplayName;
        private DataGridViewTextBoxColumn colServiceStatus;
        private DataGridViewTextBoxColumn colServiceType;

        // Вкладка Scripts
        private TabPage tabPageScript;
        private Panel pnlScriptBottom;
        private DataGridView dataGridViewScripts;
        private DataGridViewTextBoxColumn colScriptName;
        private DataGridViewTextBoxColumn colScriptDescription;
        private DataGridViewTextBoxColumn colScriptStatus;
        private DataGridViewTextBoxColumn colScriptType;
        private DataGridViewButtonColumn colScriptControl;
        private Button btnAddScript;
        private Button btnBackupScript;

        // Группа выключения
        private GroupBox gbShutdown;
        private Button btnOffPc;
        private Button btnCancelShutdown;
        private Label lblShutdownStatus;
        private NumericUpDown nudShutdownMinutes;
        private Label lblShutdownMins;
    }
}