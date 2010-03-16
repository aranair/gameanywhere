namespace GameAnywhere
{
    partial class SyncErrorDisplay
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SyncErrorDisplay));
            this.doneErrorDisplayButton = new System.Windows.Forms.Button();
            this.errorDisplayPanel = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // doneErrorDisplayButton
            // 
            this.doneErrorDisplayButton.BackgroundImage = global::GameAnywhere.Properties.Resources.doneResultPanelButton;
            this.doneErrorDisplayButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.doneErrorDisplayButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.doneErrorDisplayButton.FlatAppearance.BorderSize = 0;
            this.doneErrorDisplayButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.doneErrorDisplayButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.doneErrorDisplayButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.doneErrorDisplayButton.Location = new System.Drawing.Point(625, 563);
            this.doneErrorDisplayButton.Name = "doneErrorDisplayButton";
            this.doneErrorDisplayButton.Size = new System.Drawing.Size(84, 25);
            this.doneErrorDisplayButton.TabIndex = 9;
            this.doneErrorDisplayButton.MouseLeave += new System.EventHandler(this.doneErrorDisplayButton_MouseLeave);
            this.doneErrorDisplayButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.doneErrorDisplayButton_MouseClick);
            this.doneErrorDisplayButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.doneErrorDisplayButton_MouseDown);
            this.doneErrorDisplayButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.doneErrorDisplayButton_MouseUp);
            this.doneErrorDisplayButton.MouseEnter += new System.EventHandler(this.doneErrorDisplayButton_MouseEnter);
            // 
            // errorDisplayPanel
            // 
            this.errorDisplayPanel.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.errorDisplayPanel.ForeColor = System.Drawing.Color.White;
            this.errorDisplayPanel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.errorDisplayPanel.Location = new System.Drawing.Point(30, 103);
            this.errorDisplayPanel.Name = "errorDisplayPanel";
            this.errorDisplayPanel.Size = new System.Drawing.Size(680, 420);
            this.errorDisplayPanel.TabIndex = 10;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = global::GameAnywhere.Properties.Resources.bannerSyncErrors;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox1.Location = new System.Drawing.Point(281, 51);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(350, 33);
            this.pictureBox1.TabIndex = 11;
            this.pictureBox1.TabStop = false;
            // 
            // SyncErrorDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = global::GameAnywhere.Properties.Resources.GameChoicePopupBackground;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(734, 614);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.errorDisplayPanel);
            this.Controls.Add(this.doneErrorDisplayButton);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SyncErrorDisplay";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SyncErrorDisplay";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button doneErrorDisplayButton;
        private System.Windows.Forms.Panel errorDisplayPanel;
        private System.Windows.Forms.PictureBox pictureBox1;

    }
}