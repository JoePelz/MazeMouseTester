using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseTester {
    public class MazePanel : Panel {
        private Maze maze;
        private Matrix trans;
        private Mouse mouse;
        private Pen mousePen = new Pen(Color.Orange, 0.15f);
        public Bitmap bg;

        public MazePanel() {
            DoubleBuffered = true;
        }

        public void setMaze(Maze m) {
            maze = m;
            updateMazeBackground();
        }

        public void setMouse(Mouse m) {
            mouse = m;
            Invalidate();
        }

        private void updateMazeBackground() {
            if (maze == null) {
                return;
            }

            bg = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);

            //draw the maze onto the background.
            Graphics g = Graphics.FromImage(bg);

            int size = Math.Min(ClientRectangle.Width, ClientRectangle.Height);
            Rectangle mazeRect = new Rectangle(
                (ClientRectangle.Width >> 1) - (size >> 1) + ClientRectangle.X,
                (ClientRectangle.Height >> 1) - (size >> 1) + ClientRectangle.Y,
                size, size);

            trans = g.Transform;
            trans.Translate(mazeRect.X, mazeRect.Y);
            trans.Translate(0, mazeRect.Height);
            trans.Scale(mazeRect.Width / (maze.sizeX - 1), -mazeRect.Height / (maze.sizeY - 1));
            g.Transform = trans;

            maze.drawMaze(g);

            g.Dispose();

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            if (bg != null) {
                g.DrawImage(bg, 0, 0);
                g.Transform = trans;
            }


            //draw other stuff here
            if (mouse != null) {
                g.DrawEllipse(mousePen, 
                    mouse.position.X - 0.2f, 
                    mouse.position.Y - 0.2f,
                    0.4f, 
                    0.4f);
                g.DrawLine(mousePen,
                    mouse.position.X,
                    mouse.position.Y,
                    mouse.position.X + mouse.direction.X * 0.5f,
                    mouse.position.Y + mouse.direction.Y * 0.5f);
            }
        }

        protected override void OnResize(EventArgs eventargs) {
            base.OnResize(eventargs);
            updateMazeBackground();
        }
    }
}
