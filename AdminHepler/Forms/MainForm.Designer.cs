
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
            tbMonitorCPU = new TextBox();
            tbMonitoringGPU = new TextBox();
            tbMonitoringRAM = new TextBox();
            tbMonitoringHDD = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            btnStartMonitoring = new Button();
            btnStopMonitoring = new Button();
            label5 = new Label();
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
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(195, 186);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "monitoring resources";
            // 
            // tbMonitorCPU
            // 
            tbMonitorCPU.Location = new Point(86, 22);
            tbMonitorCPU.Name = "tbMonitorCPU";
            tbMonitorCPU.Size = new Size(100, 23);
            tbMonitorCPU.TabIndex = 0;
            // 
            // tbMonitoringGPU
            // 
            tbMonitoringGPU.Location = new Point(86, 51);
            tbMonitoringGPU.Name = "tbMonitoringGPU";
            tbMonitoringGPU.Size = new Size(100, 23);
            tbMonitoringGPU.TabIndex = 1;
            // 
            // tbMonitoringRAM
            // 
            tbMonitoringRAM.Location = new Point(86, 80);
            tbMonitoringRAM.Name = "tbMonitoringRAM";
            tbMonitoringRAM.Size = new Size(100, 23);
            tbMonitoringRAM.TabIndex = 2;
            // 
            // tbMonitoringHDD
            // 
            tbMonitoringHDD.Location = new Point(86, 109);
            tbMonitoringHDD.Name = "tbMonitoringHDD";
            tbMonitoringHDD.Size = new Size(100, 23);
            tbMonitoringHDD.TabIndex = 3;
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
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 54);
            label2.Name = "label2";
            label2.Size = new Size(36, 15);
            label2.TabIndex = 6;
            label2.Text = "GPU: ";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 83);
            label3.Name = "label3";
            label3.Size = new Size(39, 15);
            label3.TabIndex = 7;
            label3.Text = "RAM: ";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 112);
            label4.Name = "label4";
            label4.Size = new Size(74, 15);
            label4.TabIndex = 8;
            label4.Text = "Disk storage:";
            // 
            // btnStartMonitoring
            // 
            btnStartMonitoring.Location = new Point(111, 157);
            btnStartMonitoring.Name = "btnStartMonitoring";
            btnStartMonitoring.Size = new Size(75, 23);
            btnStartMonitoring.TabIndex = 9;
            btnStartMonitoring.Text = "Start";
            btnStartMonitoring.UseVisualStyleBackColor = true;
            btnStartMonitoring.Click += btnStartMonitoring_Click;
            // 
            // btnStopMonitoring
            // 
            btnStopMonitoring.Location = new Point(6, 157);
            btnStopMonitoring.Name = "btnStopMonitoring";
            btnStopMonitoring.Size = new Size(75, 23);
            btnStopMonitoring.TabIndex = 10;
            btnStopMonitoring.Text = "Stop";
            btnStopMonitoring.UseVisualStyleBackColor = true;
            btnStopMonitoring.Click += btnStopMonitoring_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(7, 139);
            label5.Name = "label5";
            label5.Size = new Size(143, 15);
            label5.TabIndex = 11;
            label5.Text = "Start and stop monitoring";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 661);
            Controls.Add(groupBox1);
            Controls.Add(btnCancelOffpc);
            Controls.Add(btnOffPcTimer);
            Controls.Add(btnTestLogger);
            Controls.Add(btnCopyLog);
            Controls.Add(btnClearLog);
            Controls.Add(btnSaveLog);
            Controls.Add(rtbLogger);
            Name = "MainForm";
            Text = "Admin Helper";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
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
    }
}
