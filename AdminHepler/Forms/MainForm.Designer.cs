namespace AdminHelper
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            rtbLogger = new RichTextBox();
            btnSaveLog = new Button();
            btnClearLog = new Button();
            btnCopyLog = new Button();
            btnOffPcTimer = new Button();
            btnCancelOffpc = new Button();
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
            colServiceMemory = new DataGridViewTextBoxColumn();
            colServiceDescription = new DataGridViewTextBoxColumn();
            colServiceStatus = new DataGridViewTextBoxColumn();
            colServiceType = new DataGridViewTextBoxColumn();
            tabPageScript = new TabPage();
            dataGridViewScripts = new DataGridView();
            colScriptName = new DataGridViewTextBoxColumn();
            colScriptDescription = new DataGridViewTextBoxColumn();
            colScriptStatus = new DataGridViewTextBoxColumn();
            colScriptType = new DataGridViewTextBoxColumn();
            colScriptControl = new DataGridViewButtonColumn();
            btnAddScript = new Button();
            btnBackupScript = new Button();
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
            SuspendLayout();
            // 
            // rtbLogger
            // 
            rtbLogger.Font = new Font("Lucida Console", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 204);
            rtbLogger.Location = new Point(12, 520);
            rtbLogger.Name = "rtbLogger";
            rtbLogger.ReadOnly = true;
            rtbLogger.Size = new Size(1060, 120);
            rtbLogger.TabIndex = 0;
            rtbLogger.Text = "";
            // 
            // btnSaveLog
            // 
            btnSaveLog.Location = new Point(997, 646);
            btnSaveLog.Name = "btnSaveLog";
            btnSaveLog.Size = new Size(75, 23);
            btnSaveLog.TabIndex = 1;
            btnSaveLog.Text = "Save Log";
            btnSaveLog.UseVisualStyleBackColor = true;
            btnSaveLog.Click += btnSaveLog_Click;
            // 
            // btnClearLog
            // 
            btnClearLog.Location = new Point(835, 646);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(75, 23);
            btnClearLog.TabIndex = 2;
            btnClearLog.Text = "Clear";
            btnClearLog.UseVisualStyleBackColor = true;
            btnClearLog.Click += btnClearLog_Click;
            // 
            // btnCopyLog
            // 
            btnCopyLog.Location = new Point(916, 646);
            btnCopyLog.Name = "btnCopyLog";
            btnCopyLog.Size = new Size(75, 23);
            btnCopyLog.TabIndex = 3;
            btnCopyLog.Text = "Copy Log";
            btnCopyLog.UseVisualStyleBackColor = true;
            btnCopyLog.Click += btnCopyLog_Click;
            // 
            // btnOffPcTimer
            // 
            btnOffPcTimer.Location = new Point(916, 12);
            btnOffPcTimer.Name = "btnOffPcTimer";
            btnOffPcTimer.Size = new Size(75, 23);
            btnOffPcTimer.TabIndex = 5;
            btnOffPcTimer.Text = "OFF PC";
            btnOffPcTimer.UseVisualStyleBackColor = true;
            btnOffPcTimer.Click += btnOffPcTimer_Click;
            // 
            // btnCancelOffpc
            // 
            btnCancelOffpc.Location = new Point(997, 12);
            btnCancelOffpc.Name = "btnCancelOffpc";
            btnCancelOffpc.Size = new Size(75, 23);
            btnCancelOffpc.TabIndex = 6;
            btnCancelOffpc.Text = "CANCEL";
            btnCancelOffpc.UseVisualStyleBackColor = true;
            btnCancelOffpc.Click += btnCancelOffpc_Click;
            // 
            // btnAdminRights
            // 
            btnAdminRights.Location = new Point(12, 646);
            btnAdminRights.Name = "btnAdminRights";
            btnAdminRights.Size = new Size(75, 23);
            btnAdminRights.TabIndex = 8;
            btnAdminRights.Text = "Admin";
            btnAdminRights.UseVisualStyleBackColor = true;
            btnAdminRights.Click += btnAdminRights_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(12, 15);
            label6.Name = "label6";
            label6.Size = new Size(46, 15);
            label6.TabIndex = 9;
            label6.Text = "Rights: ";
            // 
            // lblRights
            // 
            lblRights.AutoSize = true;
            lblRights.ForeColor = Color.Red;
            lblRights.Location = new Point(64, 15);
            lblRights.Name = "lblRights";
            lblRights.Size = new Size(34, 15);
            lblRights.TabIndex = 10;
            lblRights.Text = "none";
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPageMonitoring);
            tabControl.Controls.Add(tabPageInfoProccess);
            tabControl.Controls.Add(tabPageInfoService);
            tabControl.Controls.Add(tabPageScript);
            tabControl.Location = new Point(12, 41);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1060, 473);
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
            tabPageMonitoring.Size = new Size(1052, 445);
            tabPageMonitoring.TabIndex = 0;
            tabPageMonitoring.Text = "Monitoring";
            tabPageMonitoring.UseVisualStyleBackColor = true;
            // 
            // gbCPU
            // 
            gbCPU.Controls.Add(tbMonitorCPU);
            gbCPU.Location = new Point(6, 20);
            gbCPU.Name = "gbCPU";
            gbCPU.Size = new Size(230, 419);
            gbCPU.TabIndex = 0;
            gbCPU.TabStop = false;
            gbCPU.Text = "CPU";
            // 
            // tbMonitorCPU
            // 
            tbMonitorCPU.Dock = DockStyle.Fill;
            tbMonitorCPU.Font = new Font("Consolas", 9F);
            tbMonitorCPU.Location = new Point(3, 19);
            tbMonitorCPU.Multiline = true;
            tbMonitorCPU.Name = "tbMonitorCPU";
            tbMonitorCPU.ReadOnly = true;
            tbMonitorCPU.ScrollBars = ScrollBars.Vertical;
            tbMonitorCPU.Size = new Size(224, 397);
            tbMonitorCPU.TabIndex = 0;
            // 
            // gbRAM
            // 
            gbRAM.Controls.Add(tbMonitoringRAM);
            gbRAM.Location = new Point(478, 20);
            gbRAM.Name = "gbRAM";
            gbRAM.Size = new Size(230, 413);
            gbRAM.TabIndex = 1;
            gbRAM.TabStop = false;
            gbRAM.Text = "RAM";
            // 
            // tbMonitoringRAM
            // 
            tbMonitoringRAM.Dock = DockStyle.Fill;
            tbMonitoringRAM.Font = new Font("Consolas", 9F);
            tbMonitoringRAM.Location = new Point(3, 19);
            tbMonitoringRAM.Multiline = true;
            tbMonitoringRAM.Name = "tbMonitoringRAM";
            tbMonitoringRAM.ReadOnly = true;
            tbMonitoringRAM.ScrollBars = ScrollBars.Vertical;
            tbMonitoringRAM.Size = new Size(224, 391);
            tbMonitoringRAM.TabIndex = 1;
            // 
            // gbGPU
            // 
            gbGPU.Controls.Add(tbMonitoringGPU);
            gbGPU.Location = new Point(242, 20);
            gbGPU.Name = "gbGPU";
            gbGPU.Size = new Size(230, 416);
            gbGPU.TabIndex = 2;
            gbGPU.TabStop = false;
            gbGPU.Text = "GPU";
            // 
            // tbMonitoringGPU
            // 
            tbMonitoringGPU.Dock = DockStyle.Fill;
            tbMonitoringGPU.Font = new Font("Consolas", 9F);
            tbMonitoringGPU.Location = new Point(3, 19);
            tbMonitoringGPU.Multiline = true;
            tbMonitoringGPU.Name = "tbMonitoringGPU";
            tbMonitoringGPU.ReadOnly = true;
            tbMonitoringGPU.ScrollBars = ScrollBars.Vertical;
            tbMonitoringGPU.Size = new Size(224, 394);
            tbMonitoringGPU.TabIndex = 2;
            // 
            // gbDisk
            // 
            gbDisk.Controls.Add(tbMonitoringHDD);
            gbDisk.Location = new Point(714, 20);
            gbDisk.Name = "gbDisk";
            gbDisk.Size = new Size(230, 410);
            gbDisk.TabIndex = 3;
            gbDisk.TabStop = false;
            gbDisk.Text = "Disk Storage";
            // 
            // tbMonitoringHDD
            // 
            tbMonitoringHDD.Dock = DockStyle.Fill;
            tbMonitoringHDD.Font = new Font("Consolas", 9F);
            tbMonitoringHDD.Location = new Point(3, 19);
            tbMonitoringHDD.Multiline = true;
            tbMonitoringHDD.Name = "tbMonitoringHDD";
            tbMonitoringHDD.ReadOnly = true;
            tbMonitoringHDD.ScrollBars = ScrollBars.Vertical;
            tbMonitoringHDD.Size = new Size(224, 388);
            tbMonitoringHDD.TabIndex = 3;
            // 
            // btnStopMonitoring
            // 
            btnStopMonitoring.BackColor = Color.Red;
            btnStopMonitoring.ForeColor = Color.White;
            btnStopMonitoring.Location = new Point(947, 397);
            btnStopMonitoring.Name = "btnStopMonitoring";
            btnStopMonitoring.Size = new Size(100, 30);
            btnStopMonitoring.TabIndex = 4;
            btnStopMonitoring.Text = "Stop";
            btnStopMonitoring.UseVisualStyleBackColor = false;
            btnStopMonitoring.Click += btnStopMonitoring_Click;
            // 
            // btnStartMonitoring
            // 
            btnStartMonitoring.BackColor = Color.Green;
            btnStartMonitoring.ForeColor = Color.White;
            btnStartMonitoring.Location = new Point(947, 361);
            btnStartMonitoring.Name = "btnStartMonitoring";
            btnStartMonitoring.Size = new Size(100, 30);
            btnStartMonitoring.TabIndex = 5;
            btnStartMonitoring.Text = "Start";
            btnStartMonitoring.UseVisualStyleBackColor = false;
            btnStartMonitoring.Click += btnStartMonitoring_Click;
            // 
            // tabPageInfoProccess
            // 
            tabPageInfoProccess.Controls.Add(dataGridViewProcesses);
            tabPageInfoProccess.Location = new Point(4, 24);
            tabPageInfoProccess.Name = "tabPageInfoProccess";
            tabPageInfoProccess.Padding = new Padding(3);
            tabPageInfoProccess.Size = new Size(1052, 445);
            tabPageInfoProccess.TabIndex = 1;
            tabPageInfoProccess.Text = "Processes";
            tabPageInfoProccess.UseVisualStyleBackColor = true;
            // 
            // dataGridViewProcesses
            // 
            dataGridViewProcesses.AllowUserToAddRows = false;
            dataGridViewProcesses.AllowUserToDeleteRows = false;
            dataGridViewProcesses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewProcesses.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewProcesses.Columns.AddRange(new DataGridViewColumn[] { colProcessName, colProcessMemory, colProcessDescription, colProcessStatus, colProcessType });
            dataGridViewProcesses.Dock = DockStyle.Fill;
            dataGridViewProcesses.Location = new Point(3, 3);
            dataGridViewProcesses.Name = "dataGridViewProcesses";
            dataGridViewProcesses.ReadOnly = true;
            dataGridViewProcesses.RowHeadersVisible = false;
            dataGridViewProcesses.Size = new Size(1046, 439);
            dataGridViewProcesses.TabIndex = 0;
            // 
            // colProcessName
            // 
            colProcessName.HeaderText = "Name";
            colProcessName.Name = "colProcessName";
            colProcessName.ReadOnly = true;
            // 
            // colProcessMemory
            // 
            colProcessMemory.HeaderText = "Memory (MB)";
            colProcessMemory.Name = "colProcessMemory";
            colProcessMemory.ReadOnly = true;
            // 
            // colProcessDescription
            // 
            colProcessDescription.HeaderText = "Description";
            colProcessDescription.Name = "colProcessDescription";
            colProcessDescription.ReadOnly = true;
            // 
            // colProcessStatus
            // 
            colProcessStatus.HeaderText = "Status";
            colProcessStatus.Name = "colProcessStatus";
            colProcessStatus.ReadOnly = true;
            // 
            // colProcessType
            // 
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
            tabPageInfoService.Size = new Size(1052, 445);
            tabPageInfoService.TabIndex = 2;
            tabPageInfoService.Text = "Services";
            tabPageInfoService.UseVisualStyleBackColor = true;
            // 
            // dataGridViewServices
            // 
            dataGridViewServices.AllowUserToAddRows = false;
            dataGridViewServices.AllowUserToDeleteRows = false;
            dataGridViewServices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewServices.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewServices.Columns.AddRange(new DataGridViewColumn[] { colServiceName, colServiceMemory, colServiceDescription, colServiceStatus, colServiceType });
            dataGridViewServices.Dock = DockStyle.Fill;
            dataGridViewServices.Location = new Point(3, 3);
            dataGridViewServices.Name = "dataGridViewServices";
            dataGridViewServices.ReadOnly = true;
            dataGridViewServices.RowHeadersVisible = false;
            dataGridViewServices.Size = new Size(1046, 439);
            dataGridViewServices.TabIndex = 0;
            // 
            // colServiceName
            // 
            colServiceName.HeaderText = "Name";
            colServiceName.Name = "colServiceName";
            colServiceName.ReadOnly = true;
            // 
            // colServiceMemory
            // 
            colServiceMemory.HeaderText = "Memory (MB)";
            colServiceMemory.Name = "colServiceMemory";
            colServiceMemory.ReadOnly = true;
            // 
            // colServiceDescription
            // 
            colServiceDescription.HeaderText = "Display Name";
            colServiceDescription.Name = "colServiceDescription";
            colServiceDescription.ReadOnly = true;
            // 
            // colServiceStatus
            // 
            colServiceStatus.HeaderText = "Status";
            colServiceStatus.Name = "colServiceStatus";
            colServiceStatus.ReadOnly = true;
            // 
            // colServiceType
            // 
            colServiceType.HeaderText = "Start Type";
            colServiceType.Name = "colServiceType";
            colServiceType.ReadOnly = true;
            // 
            // tabPageScript
            // 
            tabPageScript.Controls.Add(dataGridViewScripts);
            tabPageScript.Controls.Add(btnAddScript);
            tabPageScript.Controls.Add(btnBackupScript);
            tabPageScript.Location = new Point(4, 24);
            tabPageScript.Name = "tabPageScript";
            tabPageScript.Padding = new Padding(3);
            tabPageScript.Size = new Size(1052, 445);
            tabPageScript.TabIndex = 3;
            tabPageScript.Text = "Scripts";
            tabPageScript.UseVisualStyleBackColor = true;
            // 
            // dataGridViewScripts
            // 
            dataGridViewScripts.AllowUserToAddRows = false;
            dataGridViewScripts.AllowUserToDeleteRows = false;
            dataGridViewScripts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewScripts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewScripts.Columns.AddRange(new DataGridViewColumn[] { colScriptName, colScriptDescription, colScriptStatus, colScriptType, colScriptControl });
            dataGridViewScripts.Dock = DockStyle.Fill;
            dataGridViewScripts.Location = new Point(3, 3);
            dataGridViewScripts.Name = "dataGridViewScripts";
            dataGridViewScripts.ReadOnly = true;
            dataGridViewScripts.RowHeadersVisible = false;
            dataGridViewScripts.Size = new Size(1046, 439);
            dataGridViewScripts.TabIndex = 0;
            dataGridViewScripts.CellContentClick += DataGridViewScripts_CellContentClick;
            // 
            // colScriptName
            // 
            colScriptName.HeaderText = "Name";
            colScriptName.Name = "colScriptName";
            colScriptName.ReadOnly = true;
            // 
            // colScriptDescription
            // 
            colScriptDescription.HeaderText = "Description";
            colScriptDescription.Name = "colScriptDescription";
            colScriptDescription.ReadOnly = true;
            // 
            // colScriptStatus
            // 
            colScriptStatus.HeaderText = "Status";
            colScriptStatus.Name = "colScriptStatus";
            colScriptStatus.ReadOnly = true;
            // 
            // colScriptType
            // 
            colScriptType.HeaderText = "Type";
            colScriptType.Name = "colScriptType";
            colScriptType.ReadOnly = true;
            // 
            // colScriptControl
            // 
            colScriptControl.HeaderText = "Control";
            colScriptControl.Name = "colScriptControl";
            colScriptControl.ReadOnly = true;
            colScriptControl.Text = "";
            // 
            // btnAddScript
            // 
            btnAddScript.Location = new Point(3, 408);
            btnAddScript.Name = "btnAddScript";
            btnAddScript.Size = new Size(100, 30);
            btnAddScript.TabIndex = 1;
            btnAddScript.Text = "Add Script";
            btnAddScript.UseVisualStyleBackColor = true;
            btnAddScript.Click += BtnAddScript_Click;
            // 
            // btnBackupScript
            // 
            btnBackupScript.Location = new Point(120, 408);
            btnBackupScript.Name = "btnBackupScript";
            btnBackupScript.Size = new Size(120, 30);
            btnBackupScript.TabIndex = 2;
            btnBackupScript.Text = "📁 Backup Script";
            btnBackupScript.UseVisualStyleBackColor = true;
            btnBackupScript.Click += BtnBackupScript_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1084, 681);
            Controls.Add(tabControl);
            Controls.Add(lblRights);
            Controls.Add(label6);
            Controls.Add(btnAdminRights);
            Controls.Add(btnCancelOffpc);
            Controls.Add(btnOffPcTimer);
            Controls.Add(btnCopyLog);
            Controls.Add(btnClearLog);
            Controls.Add(btnSaveLog);
            Controls.Add(rtbLogger);
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
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion

        // Элементы управления
        private RichTextBox rtbLogger;
        private Button btnSaveLog;
        private Button btnClearLog;
        private Button btnCopyLog;
        private Button btnOffPcTimer;
        private Button btnCancelOffpc;
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
        private Button btnStopMonitoring;
        private Button btnStartMonitoring;

        // Вкладка Processes
        private TabPage tabPageInfoProccess;
        private DataGridView dataGridViewProcesses;
        private DataGridViewTextBoxColumn colProcessName;
        private DataGridViewTextBoxColumn colProcessMemory;
        private DataGridViewTextBoxColumn colProcessDescription;
        private DataGridViewTextBoxColumn colProcessStatus;
        private DataGridViewTextBoxColumn colProcessType;

        // Вкладка Services
        private TabPage tabPageInfoService;
        private DataGridView dataGridViewServices;
        private DataGridViewTextBoxColumn colServiceName;
        private DataGridViewTextBoxColumn colServiceMemory;
        private DataGridViewTextBoxColumn colServiceDescription;
        private DataGridViewTextBoxColumn colServiceStatus;
        private DataGridViewTextBoxColumn colServiceType;

        // Вкладка Scripts
        private TabPage tabPageScript;
        private DataGridView dataGridViewScripts;
        private DataGridViewTextBoxColumn colScriptName;
        private DataGridViewTextBoxColumn colScriptDescription;
        private DataGridViewTextBoxColumn colScriptStatus;
        private DataGridViewTextBoxColumn colScriptType;
        private DataGridViewButtonColumn colScriptControl;
        private Button btnAddScript;
        private Button btnBackupScript;
    }
}