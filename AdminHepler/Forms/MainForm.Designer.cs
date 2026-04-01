
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
            btnTestLogger.Location = new Point(12, 12);
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
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 661);
            Controls.Add(btnCancelOffpc);
            Controls.Add(btnOffPcTimer);
            Controls.Add(btnTestLogger);
            Controls.Add(btnCopyLog);
            Controls.Add(btnClearLog);
            Controls.Add(btnSaveLog);
            Controls.Add(rtbLogger);
            Name = "MainForm";
            Text = "Admin Helper";
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
    }
}
