namespace GameAnywhere.Process
{
    partial class Tester
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Tester));
            this.ReadButton = new System.Windows.Forms.Button();
            this.testCaseSource = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.browseButton = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.displayResult = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.commentsText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ReadButton
            // 
            resources.ApplyResources(this.ReadButton, "ReadButton");
            this.ReadButton.Name = "ReadButton";
            this.ReadButton.UseVisualStyleBackColor = true;
            this.ReadButton.Click += new System.EventHandler(this.ReadButton_Click);
            // 
            // testCaseSource
            // 
            resources.ApplyResources(this.testCaseSource, "testCaseSource");
            this.testCaseSource.Name = "testCaseSource";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // browseButton
            // 
            resources.ApplyResources(this.browseButton, "browseButton");
            this.browseButton.Name = "browseButton";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // displayResult
            // 
            this.displayResult.BackColor = System.Drawing.SystemColors.ControlText;
            this.displayResult.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            resources.ApplyResources(this.displayResult, "displayResult");
            this.displayResult.Name = "displayResult";
            this.displayResult.ReadOnly = true;
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // commentsText
            // 
            resources.ApplyResources(this.commentsText, "commentsText");
            this.commentsText.Name = "commentsText";
            // 
            // Tester
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.Controls.Add(this.commentsText);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.displayResult);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.testCaseSource);
            this.Controls.Add(this.ReadButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Tester";
            this.ShowIcon = false;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ReadButton;
        private System.Windows.Forms.TextBox testCaseSource;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.RichTextBox displayResult;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label commentsText;
    }
}

