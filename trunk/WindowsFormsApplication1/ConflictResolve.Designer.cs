namespace GameAnywhere
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
            this.showconflictsPanel = new System.Windows.Forms.Panel();
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
            // showconflictsPanel
            // 
            this.showconflictsPanel.AutoScroll = true;
            this.showconflictsPanel.Font = new System.Drawing.Font("Verdana", 8F);
            this.showconflictsPanel.Location = new System.Drawing.Point(43, 89);
            this.showconflictsPanel.Name = "showconflictsPanel";
            this.showconflictsPanel.Size = new System.Drawing.Size(699, 382);
            this.showconflictsPanel.TabIndex = 4;
            // 
            // ConflictResolve
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = global::GameAnywhere.Properties.Resources.GameChoicePopupBackground;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.showconflictsPanel);
            this.Controls.Add(this.banner);
            this.ForeColor = System.Drawing.Color.White;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ConflictResolve";
            this.Text = "GameAnywhere";
            ((System.ComponentModel.ISupportInitialize)(this.banner)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox banner;
        private System.Windows.Forms.Panel showconflictsPanel;
    }
}