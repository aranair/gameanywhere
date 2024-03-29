﻿//Boa Ho Man
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GameAnywhere.Interface
{
    /// <summary>
    /// This Form handles the Waiting Dialog during all waits.
    /// </summary>
    /// 

    public partial class WaitingDialog : Form
    {
        public bool continueRun = true;
        public bool ContinueRun
        {
            get { return continueRun; }
            set { continueRun = value; }
        }
        public WaitingDialog()
        {
            InitializeComponent();
        }

        public WaitingDialog(string text)
        {
            InitializeComponent();
            this.label1.Text = text;
        }

        public void startUp()
        {
            this.Show();
            this.Refresh();
            while (continueRun) { Application.DoEvents(); }
        }

    }
}
