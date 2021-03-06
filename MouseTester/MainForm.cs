﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseTester {
    public partial class MainForm : Form {
        Controller controller;

        public MainForm() {
            InitializeComponent();
            controller = new Controller(pnlMaze, txtFeedback);
            FormClosing += MainForm_Closing;
        }

        private void btnRebuild_Click(object sender, EventArgs e) {
            if (!controller.isRunning) {
                controller.rebuildMaze();
                pnlMaze.Invalidate();
            }
        }

        private void btnStart_Click(object sender, EventArgs e) {
            controller.playPause();
        }

        private void btnReset_Click(object sender, EventArgs e) {
            controller.reset();
        }

        private void MainForm_Closing(Object sender, FormClosingEventArgs e) {
            controller.reset();
        }

        private void btnDebug_Click(object sender, EventArgs e) {
            controller.printDebug();
        }
    }
}
