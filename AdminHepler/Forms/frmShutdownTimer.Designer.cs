namespace AdminHelper.Forms
{
    partial class frmShutdownTimer
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
            label1 = new Label();
            tbMinutsTimer = new TextBox();
            btnStart = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(118, 9);
            label1.Name = "label1";
            label1.Size = new Size(44, 15);
            label1.TabIndex = 0;
            label1.Text = "minuts";
            // 
            // tbMinutsTimer
            // 
            tbMinutsTimer.Location = new Point(91, 29);
            tbMinutsTimer.Name = "tbMinutsTimer";
            tbMinutsTimer.Size = new Size(100, 23);
            tbMinutsTimer.TabIndex = 1;
            tbMinutsTimer.TextChanged += tbMinutsTimer_TextChanged;
            // 
            // btnStart
            // 
            btnStart.Location = new Point(197, 76);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(75, 23);
            btnStart.TabIndex = 2;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(12, 76);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // frmShutdownTimer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(284, 111);
            Controls.Add(btnCancel);
            Controls.Add(btnStart);
            Controls.Add(tbMinutsTimer);
            Controls.Add(label1);
            MaximizeBox = false;
            MaximumSize = new Size(300, 150);
            MinimizeBox = false;
            MinimumSize = new Size(300, 150);
            Name = "frmShutdownTimer";
            ShowInTaskbar = false;
            Text = "frmShutdownTimer";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox tbMinutsTimer;
        private Button btnStart;
        private Button btnCancel;
    }
}