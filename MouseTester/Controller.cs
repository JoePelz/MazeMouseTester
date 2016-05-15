using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace MouseTester {
    public class Controller {
        Maze maze;
        Hardware hardware;
        MouseAI ai;
        RichTextBox txtFeedback;
        MazePanel mazeDisplay;


        //Mouse physics parameters
        private Mouse mouse;
        private float speedScale = 5.0f;
        private float turnScale = 10.0f;
        //private float maxSpeed = 1.0f;
        //private float friction = 0.8f;

        //Threading controls
        private Thread gameThread;
        private bool paused;
        private Semaphore pause;
        private bool alive;
        private int deltaTicks;
        private int currentTick;
        private int lastTick;

        public bool isRunning
        {
            get { return !paused && alive; }
        }

        public Controller(MazePanel display, RichTextBox feedback) {
            txtFeedback = feedback;
            mazeDisplay = display;
            maze = new Maze(10, 10);
            hardware = new Hardware(this);
            mouse = new Mouse();
            ai = new MouseRandom();
            alive = true;
            pause = new Semaphore(1, 1);
            paused = false;

            display.setMaze(maze);
            display.setMouse(mouse);
        }

        internal void printDebug() {
            txtFeedback.Clear();
            txtFeedback.AppendText("Mouse Data:\n===========\n");
            txtFeedback.AppendText("Angle: " + mouse.angle + "\n");
            txtFeedback.AppendText("Position: " + mouse.position + "\n");
            txtFeedback.AppendText("Direction: " + mouse.direction + "\n");

            txtFeedback.AppendText("\n");
            txtFeedback.AppendText("Sensors:\n===========\n");
            txtFeedback.AppendText("Forward: " + hardware.getForwardSensor() + "\n");
            txtFeedback.AppendText("Left: " + hardware.getLeftSensor() + "\n");
            txtFeedback.AppendText("Right: " + hardware.getRightSensor() + "\n");
            txtFeedback.AppendText("Power Forward: " + hardware.getForwardPower() + "\n");
            txtFeedback.AppendText("Power Turning: " + hardware.getTurnPower() + "\n");
        }

        private void resetMousePhysics() {
            mouse.position = new PointF(1.5f, 1.5f);
            mouse.direction = new PointF(0.0f, 1.0f);
            mouse.speed = 0.0f;
            mouse.angle = (float)(Math.PI / 2.0);
        }

        public void mainLoop() {
            Console.WriteLine("Starting!");
            while (alive) {
                //handle pausing
                pause.WaitOne();
                pause.Release();

                //set deltaTime
                currentTick = System.Environment.TickCount; //this is milliseconds
                deltaTicks = currentTick - lastTick; //This _should_ be milliseconds.
                lastTick = currentTick;

                //perform AI update
                ai.update(hardware);

                //update mouse physics
                //TODO: should use velocity and acceleration instead of direct position updates.
                //TODO: momentum
                mouse.angle += hardware.getTurnPower() * turnScale * deltaTicks / 1000.0f;
                mouse.direction = new PointF((float)Math.Cos(mouse.angle), (float)Math.Sin(mouse.angle));

                float motion = hardware.getForwardPower() * speedScale * deltaTicks / 1000.0f;
                mouse.position.X += mouse.direction.X * motion;
                mouse.position.Y += mouse.direction.Y * motion;
                fixCollisions();

                //update display
                mazeDisplay.setMouse(mouse);
            }
            Console.WriteLine("Dead.");
        }

        internal float CastRayFromMouse(float angle) {
            //build ray pointing correct direction
            //TODO: build direction from angle
            PointF direction = new PointF((float)Math.Cos(-angle + mouse.angle), (float)Math.Sin(-angle + mouse.angle));
            Ray ray = new Ray(mouse.position, direction);

            //determine ray collision point
            PointF col = maze.RayCast(ray);

            if (col.X >= 0) {
                //distance from mouse to ray collision
                double distance = Math.Sqrt(
                    (col.X - mouse.position.X) * (col.X - mouse.position.X) + 
                    (col.Y - mouse.position.Y) * (col.Y - mouse.position.Y));
                return (float)distance;
            } else {
                //no wall detected
                return float.PositiveInfinity;
            }
        }

        private void fixCollisions() {
            float posX = mouse.position.X;
            int posXN = (int)posX;
            int posXNR = (int)Math.Round(posX);
            float posY = mouse.position.Y;
            int posYN = (int)posY;
            int posYNR = (int)Math.Round(posY);
            Vertex check;
            //TODO: handle vertices, not just edges.
            //  (doesn't work correctly if hitting edge from beside, 
            //   rather than straight on.)


            //fix X collisions
            check = maze.getVertex(posXNR, posYN);
            if (check.up == Maze.WALL &&
                Math.Abs(posX - posXNR) < mouse.radius) { //if overlapping a vertical wall
                //move out of it.
                if (posX < posXNR) { //if left of wall
                    mouse.position.X -= mouse.radius - (posXNR - posX);
                } else { //if right of wall
                    mouse.position.X += mouse.radius - (posX - posXNR);
                }
            }

            //fix Y collisions
            check = maze.getVertex(posXN, posYNR);
            if (check.right == Maze.WALL &&
                Math.Abs(posYNR - posY) < mouse.radius) {  //if overlapping a horizontal wall
                //move out of it.
                if (posY < posYNR) { //if below wall
                    mouse.position.Y -= mouse.radius - (posYNR - posY);
                } else { //if above wall
                    mouse.position.Y += mouse.radius - (posY - posYNR);
                }
            }
        }

        public void playPause() {
            if (gameThread == null) {
                alive = true;
                gameThread = new Thread(mainLoop);
                lastTick = System.Environment.TickCount; //so first frame doesn't explode.
                gameThread.Start();
                return;
            }

            if (paused) {
                Console.WriteLine("Unpausing.");
                lastTick = System.Environment.TickCount;
                pause.Release();
                paused = false;
            } else {
                pause.WaitOne();
                Console.WriteLine("Paused.");
                paused = true;
            }
        }

        public void reset() {
            Console.WriteLine("Killing thread.");
            if (paused) {
                playPause(); //unpause
            }
            alive = false;
            if (gameThread != null) {
                gameThread.Join();
                Console.WriteLine("Thread collected.");
                gameThread = null;
            } else {
                Console.WriteLine("Thread already dead.");
            }
            resetMousePhysics();
            mazeDisplay.setMouse(mouse);
        }

        public void rebuildMaze() {
            maze.rebuildMaze();
            mazeDisplay.setMaze(maze);
        }
        
    }
}
