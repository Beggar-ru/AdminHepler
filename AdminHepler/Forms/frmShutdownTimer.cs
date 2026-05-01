using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdminHelper.Scripts;
using AdminHelper.Logger;
using AdminHelper.Utils;

namespace AdminHelper.Forms
{
    public partial class frmShutdownTimer : Form
    {
        private readonly ILogger _logger;

        public frmShutdownTimer(ILogger logger)
        {
            InitializeComponent();

            _logger = logger;

            tbMinutsTimer.KeyPress += TbMinutsTimer_KeyPress;
        }

        private void TbMinutsTimer_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только цифры и клавишу Backspace
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }


        private void tbMinutsTimer_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (int.TryParse(tbMinutsTimer.Text, out int minutes) && minutes > 0)
            {
                var tools = new SystemTools(_logger);
                tools.ScheduleShutdown(minutes);
                this.Close();
            }
        }
    }
}
