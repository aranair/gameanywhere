using System.Collections;

namespace GameAnywhere.Interface
{
    partial class ChooseGame
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


        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChooseGame));
            this.showGamePanel = new System.Windows.Forms.Panel();
            this.summaryPanel = new System.Windows.Forms.Panel();
            this.banner = new System.Windows.Forms.PictureBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.resultPanel = new System.Windows.Forms.Panel();
            this.confirmButton = new System.Windows.Forms.Button();
            this.doneResultPanelButton = new System.Windows.Forms.Button();
            this.errorButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.banner)).BeginInit();
            this.SuspendLayout();
            // 
            // showGamePanel
            // 
            this.showGamePanel.AutoScroll = true;
            this.showGamePanel.Font = new System.Drawing.Font("Verdana", 8F);
            this.showGamePanel.Location = new System.Drawing.Point(43, 89);
            this.showGamePanel.Name = "showGamePanel";
            this.showGamePanel.Size = new System.Drawing.Size(710, 405);
            this.showGamePanel.TabIndex = 0;
            // 
            // summaryPanel
            // 
            this.summaryPanel.AutoScroll = true;
            this.summaryPanel.Enabled = false;
            this.summaryPanel.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.summaryPanel.Location = new System.Drawing.Point(43, 89);
            this.summaryPanel.Name = "summaryPanel";
            this.summaryPanel.Size = new System.Drawing.Size(710, 405);
            this.summaryPanel.TabIndex = 1;
            this.summaryPanel.Visible = false;
            // 
            // banner
            // 
            this.banner.BackgroundImage = global::GameAnywhere.Properties.Resources.bannerChooseGamesToSync;
            this.banner.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.banner.Location = new System.Drawing.Point(324, 45);
            this.banner.Name = "banner";
            this.banner.Size = new System.Drawing.Size(390, 38);
            this.banner.TabIndex = 2;
            this.banner.TabStop = false;
            // 
            // cancelButton
            // 
            this.cancelButton.BackgroundImage = global::GameAnywhere.Properties.Resources.cancelLoginPanelButton;
            this.cancelButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.cancelButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cancelButton.FlatAppearance.BorderSize = 0;
            this.cancelButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.cancelButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.Location = new System.Drawing.Point(688, 524);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(84, 25);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.MouseLeave += new System.EventHandler(this.cancelButton_MouseLeave);
            this.cancelButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.cancelButton_MouseClick);
            this.cancelButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cancelButton_MouseDown);
            this.cancelButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.cancelButton_MouseUp);
            this.cancelButton.MouseEnter += new System.EventHandler(this.cancelButton_MouseEnter);
            // 
            // resultPanel
            // 
            this.resultPanel.Enabled = false;
            this.resultPanel.Font = new System.Drawing.Font("Verdana", 8F);
            this.resultPanel.Location = new System.Drawing.Point(43, 89);
            this.resultPanel.Name = "resultPanel";
            this.resultPanel.Size = new System.Drawing.Size(710, 405);
            this.resultPanel.TabIndex = 5;
            this.resultPanel.Visible = false;
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
            this.confirmButton.Location = new System.Drawing.Point(589, 524);
            this.confirmButton.Name = "confirmButton";
            this.confirmButton.Size = new System.Drawing.Size(84, 25);
            this.confirmButton.TabIndex = 6;
            this.confirmButton.MouseLeave += new System.EventHandler(this.confirmButton_MouseLeave);
            this.confirmButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.confirmButton_MouseClick);
            this.confirmButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.confirmButton_MouseDown);
            this.confirmButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.confirmButton_MouseUp);
            this.confirmButton.MouseEnter += new System.EventHandler(this.confirmButton_MouseEnter);
            // 
            // doneResultPanelButton
            // 
            this.doneResultPanelButton.BackgroundImage = global::GameAnywhere.Properties.Resources.doneResultPanelButton;
            this.doneResultPanelButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.doneResultPanelButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.doneResultPanelButton.Enabled = false;
            this.doneResultPanelButton.FlatAppearance.BorderSize = 0;
            this.doneResultPanelButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.doneResultPanelButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.doneResultPanelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.doneResultPanelButton.Location = new System.Drawing.Point(697, 524);
            this.doneResultPanelButton.Name = "doneResultPanelButton";
            this.doneResultPanelButton.Size = new System.Drawing.Size(84, 25);
            this.doneResultPanelButton.TabIndex = 8;
            this.doneResultPanelButton.Visible = false;
            this.doneResultPanelButton.MouseLeave += new System.EventHandler(this.doneResultPanelButton_MouseLeave);
            this.doneResultPanelButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.doneResultPanelButton_MouseClick);
            this.doneResultPanelButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.doneResultPanelButton_MouseDown);
            this.doneResultPanelButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.doneResultPanelButton_MouseUp);
            this.doneResultPanelButton.MouseEnter += new System.EventHandler(this.doneResultPanelButton_MouseEnter);
            // 
            // errorButton
            // 
            this.errorButton.BackgroundImage = global::GameAnywhere.Properties.Resources.errorsButton;
            this.errorButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.errorButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.errorButton.FlatAppearance.BorderSize = 0;
            this.errorButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.errorButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.errorButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.errorButton.Font = new System.Drawing.Font("Verdana", 8.25F);
            this.errorButton.ForeColor = System.Drawing.Color.Lime;
            this.errorButton.Location = new System.Drawing.Point(589, 521);
            this.errorButton.Name = "errorButton";
            this.errorButton.Size = new System.Drawing.Size(84, 25);
            this.errorButton.TabIndex = 9;
            this.errorButton.UseVisualStyleBackColor = true;
            this.errorButton.Visible = false;
            this.errorButton.MouseLeave += new System.EventHandler(this.errorButton_MouseLeave);
            this.errorButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.errorButton_MouseClick);
            this.errorButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.errorButton_MouseDown);
            this.errorButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.errorButton_MouseUp);
            this.errorButton.MouseEnter += new System.EventHandler(this.errorButton_MouseEnter);
            // 
            // ChooseGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = global::GameAnywhere.Properties.Resources.GameChoicePopupBackground;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(794, 576);
            this.Controls.Add(this.confirmButton);
            this.Controls.Add(this.banner);
            this.Controls.Add(this.showGamePanel);
            this.Controls.Add(this.summaryPanel);
            this.Controls.Add(this.resultPanel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.errorButton);
            this.Controls.Add(this.doneResultPanelButton);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChooseGame";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GameAnywhere";
            ((System.ComponentModel.ISupportInitialize)(this.banner)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion


        private System.Windows.Forms.Panel showGamePanel;
        private System.Windows.Forms.Panel summaryPanel;
        private System.Windows.Forms.PictureBox banner;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Panel resultPanel;
        private System.Windows.Forms.Button confirmButton;
        private System.Windows.Forms.Button doneResultPanelButton;
        private System.Windows.Forms.Button errorButton;




    }
}