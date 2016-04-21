using System;
using System.Data;
using System.Windows.Forms;
using System.Threading;

namespace Heren.HDPMP.Upgrade
{
    public partial class ProgressForm : Form
    {
        public int CurPrecent
        {
            set
            {
                MethodInvoker setBar = delegate()
                {
                    if (value < 100)
                        progressBar.Value = value;
                };

                if (this.progressBar.InvokeRequired)
                    this.progressBar.Invoke(setBar);
                else
                    setBar();

            }
        }

        private string operation = string.Empty;
        public string SyncOperation
        {
            set
            {
                MethodInvoker invoker = delegate()
                {
                    if (!string.IsNullOrEmpty(value))
                        lblStatus.Text = value;
                };

                if (this.lblStatus.InvokeRequired)
                    this.lblStatus.Invoke(invoker);
                else
                    invoker();
            }
        }

        public void CloseForm()
        {
            MethodInvoker invoker = delegate { this.Close(); };
            if (this.InvokeRequired)
                this.Invoke(invoker);
            else
                invoker();
        }

        public ProgressForm()
        {
            InitializeComponent();
        }
    }
}