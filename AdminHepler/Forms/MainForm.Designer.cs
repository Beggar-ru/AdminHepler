
namespace AdminHepler
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            rtbLogger = new RichTextBox();
            btnSaveLog = new Button();
            btnClearLog = new Button();
            btnCopyLog = new Button();
            btnTestLogger = new Button();
            btnOffPcTimer = new Button();
            btnCancelOffpc = new Button();
            groupBox1 = new GroupBox();
            label5 = new Label();
            btnStopMonitoring = new Button();
            btnStartMonitoring = new Button();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            tbMonitoringHDD = new TextBox();
            tbMonitoringRAM = new TextBox();
            tbMonitoringGPU = new TextBox();
            tbMonitorCPU = new TextBox();
            btnAdminRights = new Button();
            label6 = new Label();
            lblRights = new Label();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // rtbLogger
            // 
            rtbLogger.Font = new Font("Lucida Console", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 204);
            rtbLogger.Location = new Point(12, 478);
            rtbLogger.Name = "rtbLogger";
            rtbLogger.ReadOnly = true;
            rtbLogger.Size = new Size(760, 142);
            rtbLogger.TabIndex = 0;
            rtbLogger.Text = "";
            rtbLogger.TextChanged += rtbLogger_TextChanged;
            // 
            // btnSaveLog
            // 
            btnSaveLog.Location = new Point(697, 626);
            btnSaveLog.Name = "btnSaveLog";
            btnSaveLog.Size = new Size(75, 23);
            btnSaveLog.TabIndex = 1;
            btnSaveLog.Text = "Save Log";
            btnSaveLog.UseVisualStyleBackColor = true;
            btnSaveLog.Click += btnSaveLog_Click;
            // 
            // btnClearLog
            // 
            btnClearLog.Location = new Point(535, 626);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(75, 23);
            btnClearLog.TabIndex = 2;
            btnClearLog.Text = "Clear";
            btnClearLog.UseVisualStyleBackColor = true;
            btnClearLog.Click += btnClearLog_Click;
            // 
            // btnCopyLog
            // 
            btnCopyLog.Location = new Point(616, 626);
            btnCopyLog.Name = "btnCopyLog";
            btnCopyLog.Size = new Size(75, 23);
            btnCopyLog.TabIndex = 3;
            btnCopyLog.Text = "Copy Log";
            btnCopyLog.UseVisualStyleBackColor = true;
            btnCopyLog.Click += btnCopyLog_Click;
            // 
            // btnTestLogger
            // 
            btnTestLogger.Location = new Point(580, 12);
            btnTestLogger.Name = "btnTestLogger";
            btnTestLogger.Size = new Size(75, 23);
            btnTestLogger.TabIndex = 4;
            btnTestLogger.Text = "btnTestLog";
            btnTestLogger.UseVisualStyleBackColor = true;
            btnTestLogger.Click += btnTestLogger_Click;
            // 
            // btnOffPcTimer
            // 
            btnOffPcTimer.Location = new Point(697, 12);
            btnOffPcTimer.Name = "btnOffPcTimer";
            btnOffPcTimer.Size = new Size(75, 23);
            btnOffPcTimer.TabIndex = 5;
            btnOffPcTimer.Text = "OFF PC";
            btnOffPcTimer.UseVisualStyleBackColor = true;
            btnOffPcTimer.Click += btnOffPcTimer_Click;
            // 
            // btnCancelOffpc
            // 
            btnCancelOffpc.Location = new Point(697, 41);
            btnCancelOffpc.Name = "btnCancelOffpc";
            btnCancelOffpc.Size = new Size(75, 23);
            btnCancelOffpc.TabIndex = 6;
            btnCancelOffpc.Text = "CANCEL";
            btnCancelOffpc.UseVisualStyleBackColor = true;
            btnCancelOffpc.Click += btnCancelOffpc_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(btnStopMonitoring);
            groupBox1.Controls.Add(btnStartMonitoring);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(tbMonitoringHDD);
            groupBox1.Controls.Add(tbMonitoringRAM);
            groupBox1.Controls.Add(tbMonitoringGPU);
            groupBox1.Controls.Add(tbMonitorCPU);
            groupBox1.Location = new Point(12, 264);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(220, 208);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "monitoring resources";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(6, 157);
            label5.Name = "label5";
            label5.Size = new Size(143, 15);
            label5.TabIndex = 11;
            label5.Text = "Start and stop monitoring";
            label5.Click += label5_Click;
            // 
            // btnStopMonitoring
            // 
            btnStopMonitoring.Location = new Point(5, 175);
            btnStopMonitoring.Name = "btnStopMonitoring";
            btnStopMonitoring.Size = new Size(75, 23);
            btnStopMonitoring.TabIndex = 10;
            btnStopMonitoring.Text = "Stop";
            btnStopMonitoring.UseVisualStyleBackColor = true;
            btnStopMonitoring.Click += btnStopMonitoring_Click;
            // 
            // btnStartMonitoring
            // 
            btnStartMonitoring.Location = new Point(137, 175);
            btnStartMonitoring.Name = "btnStartMonitoring";
            btnStartMonitoring.Size = new Size(75, 23);
            btnStartMonitoring.TabIndex = 9;
            btnStartMonitoring.Text = "Start";
            btnStartMonitoring.UseVisualStyleBackColor = true;
            btnStartMonitoring.Click += btnStartMonitoring_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(111, 25);
            label4.Name = "label4";
            label4.Size = new Size(74, 15);
            label4.TabIndex = 8;
            label4.Text = "Disk storage:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(5, 113);
            label3.Name = "label3";
            label3.Size = new Size(39, 15);
            label3.TabIndex = 7;
            label3.Text = "RAM: ";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(5, 69);
            label2.Name = "label2";
            label2.Size = new Size(36, 15);
            label2.TabIndex = 6;
            label2.Text = "GPU: ";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 25);
            label1.Name = "label1";
            label1.Size = new Size(36, 15);
            label1.TabIndex = 5;
            label1.Text = "CPU: ";
            // 
            // tbMonitoringHDD
            // 
            tbMonitoringHDD.Location = new Point(112, 43);
            tbMonitoringHDD.Multiline = true;
            tbMonitoringHDD.Name = "tbMonitoringHDD";
            tbMonitoringHDD.Size = new Size(100, 111);
            tbMonitoringHDD.TabIndex = 3;
            // 
            // tbMonitoringRAM
            // 
            tbMonitoringRAM.Location = new Point(6, 131);
            tbMonitoringRAM.Name = "tbMonitoringRAM";
            tbMonitoringRAM.Size = new Size(100, 23);
            tbMonitoringRAM.TabIndex = 2;
            tbMonitoringRAM.TextChanged += tbMonitoringRAM_TextChanged;
            // 
            // tbMonitoringGPU
            // 
            tbMonitoringGPU.Location = new Point(6, 87);
            tbMonitoringGPU.Name = "tbMonitoringGPU";
            tbMonitoringGPU.Size = new Size(100, 23);
            tbMonitoringGPU.TabIndex = 1;
            // 
            // tbMonitorCPU
            // 
            tbMonitorCPU.Location = new Point(6, 43);
            tbMonitorCPU.Name = "tbMonitorCPU";
            tbMonitorCPU.Size = new Size(100, 23);
            tbMonitorCPU.TabIndex = 0;
            // 
            // btnAdminRights
            // 
            btnAdminRights.Location = new Point(12, 626);
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
            label6.Location = new Point(12, 9);
            label6.Name = "label6";
            label6.Size = new Size(46, 15);
            label6.TabIndex = 9;
            label6.Text = "Rights: ";
            // 
            // lblRights
            // 
            lblRights.AutoSize = true;
            lblRights.Location = new Point(64, 9);
            lblRights.Name = "lblRights";
            lblRights.Size = new Size(34, 15);
            lblRights.TabIndex = 10;
            lblRights.Text = "none";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 661);
            Controls.Add(lblRights);
            Controls.Add(label6);
            Controls.Add(btnAdminRights);
            Controls.Add(groupBox1);
            Controls.Add(btnCancelOffpc);
            Controls.Add(btnOffPcTimer);
            Controls.Add(btnTestLogger);
            Controls.Add(btnCopyLog);
            Controls.Add(btnClearLog);
            Controls.Add(btnSaveLog);
            Controls.Add(rtbLogger);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            Text = "Admin Helper";
            Load += MainForm_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox rtbLogger;
        private Button btnSaveLog;
        private Button btnClearLog;
        private Button btnCopyLog;
        private Button btnTestLogger;
        private Button btnOffPcTimer;
        private Button btnCancelOffpc;
        private GroupBox groupBox1;
        private Label label4;
        private Label label3;
        private Label label2;
        private Label label1;
        private TextBox tbMonitoringHDD;
        private TextBox tbMonitoringRAM;
        private TextBox tbMonitoringGPU;
        private TextBox tbMonitorCPU;
        private Button btnStopMonitoring;
        private Button btnStartMonitoring;
        private Label label5;
        private Button btnAdminRights;
        private Label label6;
        private Label lblRights;
    }
}
