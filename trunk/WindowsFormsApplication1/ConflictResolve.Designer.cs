namespace GameAnywhere.Interface
{
    partial class ConflictResolve
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConflictResolve));
            this.banner = new System.Windows.Forms.PictureBox();
            this.showConflictsPanel = new System.Windows.Forms.Panel();
            this.confirmButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.banner)).BeginInit();
            this.SuspendLayout();
            // 
            // banner
            // 
            this.banner.BackgroundImage = global::GameAnywhere.Properties.Resources.bannerConflictResolve;
            this.banner.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.banner.Location = new System.Drawing.Point(311, 46);
            this.banner.Name = "banner";
            this.banner.Size = new System.Drawing.Size(338, 35);
            this.banner.TabIndex = 3;
            this.banner.TabStop = false;
            // 
            // showConflictsPanel
            // 
            this.showConflictsPanel.AutoScroll = true;
            this.showConflictsPanel.Font = new System.Drawing.Font("Verdana", 8F);
            this.showConflictsPanel.Location = new System.Drawing.Point(57, 89);
            this.showConflictsPanel.Name = "showConflictsPanel";
            this.showConflictsPanel.Size = new System.Drawing.Size(685, 382);
            this.showConflictsPanel.TabIndex = 4;
            // 
            // confirmButton
            // 
            this.confirmButton.BackgroundImage = global::GameAnywhere.Properties.Resources.confirmGameChoicePopupButtonDulled;
            this.confirmButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.confirmButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.confirmButton.Enabled = false;
            this.confirmButton.FlatAppearance.BorderSize = 0;
            this.confirmButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.confirmButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.confirmButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.confirmButton.Location = new System.Drawing.Point(671, 512);
            this.confirmButton.Name = "confirmButton";
            this.confirmButton.Size = new System.Drawing.Size(84, 25);
            this.confirmButton.TabIndex = 7;
            this.confirmButton.MouseLeave += new System.EventHandler(this.confirmButton_MouseLeave);
            this.confirmButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.confirmButton_MouseClick);
            this.confirmButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.confirmButton_MouseDown);
            this.confirmButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.confirmButton_MouseUp);
            this.confirmButton.MouseEnter += new System.EventHandler(this.confirmButton_MouseEnter);
            // 
            // ConflictResolve
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = global::GameAnywhere.Properties.Resources.GameChoicePopupBackground;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.confirmButton);
            this.Controls.Add(this.showConflictsPanel);
            this.Controls.Add(this.banner);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ConflictResolve";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GameAnywhere";
            ((System.ComponentModel.ISupportInitialize)(this.banner)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox banner;
        private System.Windows.Forms.Panel showConflictsPanel;
        private System.Windows.Forms.Button confirmButton;
    }
}