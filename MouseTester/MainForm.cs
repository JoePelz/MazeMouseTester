using System;
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
        Maze maze;

        public MainForm() {
            InitializeComponent();

            maze = new Maze(10, 10);
        }

        private void btnRebuild_Click(object sender, EventArgs e) {
            maze.rebuildMaze();
            pnlMaze.Invalidate();
        }

        private void btnStart_Click(object sender, EventArgs e) {

        }

        private void btnReset_Click(object sender, EventArgs e) {

        }

        private void pnlMaze_Paint(object sender, PaintEventArgs e) {
            maze.drawMaze(e.Graphics, pnlMaze.ClientRectangle);
        }
    }
}
