namespace Heren.HDPMP.Upgrade
{
    partial class ProgressForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgressForm));
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblCurPrecent = new System.Windows.Forms.Label();
            this.rtbOperation = new System.Windows.Forms.RichTextBox();
            this.picBck = new System.Windows.Forms.PictureBox();
            this.lblStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picBck)).BeginInit();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.ForeColor = System.Drawing.Color.RoyalBlue;
            this.progressBar.Location = new System.Drawing.Point(16, 240);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(453, 16);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 0;
            // 
            // lblCurPrecent
            // 
            this.lblCurPrecent.AutoSize = true;
            this.lblCurPrecent.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCurPrecent.Location = new System.Drawing.Point(591, 266);
            this.lblCurPrecent.Name = "lblCurPrecent";
            this.lblCurPrecent.Size = new System.Drawing.Size(14, 14);
            this.lblCurPrecent.TabIndex = 2;
            this.lblCurPrecent.Text = "%";
            this.lblCurPrecent.Visible = false;
            // 
            // rtbOperation
            // 
            this.rtbOperation.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rtbOperation.Location = new System.Drawing.Point(46, 82);
            this.rtbOperation.Name = "rtbOperation";
            this.rtbOperation.ReadOnly = true;
            this.rtbOperation.Size = new System.Drawing.Size(92, 22);
            this.rtbOperation.TabIndex = 3;
            this.rtbOperation.Text = "";
            // 
            // picBck
            // 
            this.picBck.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picBck.BackgroundImage")));
            this.picBck.InitialImage = null;
            this.picBck.Location = new System.Drawing.Point(0, 0);
            this.picBck.Name = "picBck";
            this.picBck.Size = new System.Drawing.Size(483, 189);
            this.picBck.TabIndex = 4;
            this.picBck.TabStop = false;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(20, 209);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 12);
            this.lblStatus.TabIndex = 5;
            // 
            // ProgressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(483, 268);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.picBck);
            this.Controls.Add(this.rtbOperation);
            this.Controls.Add(this.lblCurPrecent);
            this.Controls.Add(this.progressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "升级中...";
            ((System.ComponentModel.ISupportInitialize)(this.picBck)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblCurPrecent;
        private System.Windows.Forms.RichTextBox rtbOperation;
        private System.Windows.Forms.PictureBox picBck;
        private System.Windows.Forms.Label lblStatus;
    }
}