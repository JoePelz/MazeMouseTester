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
        private float motorStrength = 1.0f;
        private float turnScale = 10.0f;
        private float maxSpeed = 2.0f;
        //TODO: this doesn't work predictably.
        private float momentumLoss = 0.01f; //0.0 means no loss of momentum (like no friction). 

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
            int length = 0;
            txtFeedback.AppendText("Mouse Data:\n===========\n");

            txtFeedback.Select(length, 11);
            txtFeedback.SelectionFont = new Font(txtFeedback.Font, FontStyle.Bold);

            txtFeedback.AppendText("Angle: " + mouse.angle + "\n");
            txtFeedback.AppendText(String.Format("Position: [{0:0.00}, {1:0.00}] \n", mouse.position.X, mouse.position.Y));
            txtFeedback.AppendText(String.Format("Direction: [{0:0.00}, {1:0.00}] \n", mouse.direction.X, mouse.direction.Y));
            txtFeedback.AppendText(String.Format("Velocity: [{0:0.00}, {1:0.00}] \n", mouse.velocity.X, mouse.velocity.Y));
            txtFeedback.AppendText("Radius: " + mouse.radius + "\n");

            txtFeedback.AppendText("\n");
            length = txtFeedback.Text.Length;
            txtFeedback.AppendText("Sensors:\n===========\n");
            
            txtFeedback.Select(length, 8);
            txtFeedback.SelectionFont = new Font(txtFeedback.Font, FontStyle.Bold);

            txtFeedback.AppendText("Forward: " + hardware.getForwardSensor() + "\n");
            txtFeedback.AppendText("Left: " + hardware.getLeftSensor() + "\n");
            txtFeedback.AppendText("Right: " + hardware.getRightSensor() + "\n");
            txtFeedback.AppendText("Power Forward: " + hardware.getForwardPower() + "\n");
            txtFeedback.AppendText("Power Turning: " + hardware.getTurnPower() + "\n");
        }

        private void resetMousePhysics() {
            mouse.position = new PointF(Mouse.StartPosition.X, Mouse.StartPosition.Y);
            mouse.direction = new PointF(Mouse.StartDirection.X, Mouse.StartDirection.Y);
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
                deltaTicks = currentTick - lastTick;
                lastTick = currentTick;

                //perform AI update
                hardware.setMillis(deltaTicks);
                ai.update(hardware);

                //update mouse physics
                mouse.angle += hardware.getTurnPower() * turnScale * deltaTicks / 1000.0f;
                mouse.direction = new PointF((float)Math.Cos(mouse.angle), (float)Math.Sin(mouse.angle));
                
                //TODO: These physics formulae (especially "friction" below) and speculative at best,
                //      but I don't know where to start to make it reliable. 
                //      Issues arise in applying friction different numbers of times per frame, like compound interest.
                float motion = hardware.getForwardPower() * motorStrength;
                mouse.velocity = new PointF(
                    mouse.velocity.X + mouse.direction.X * motion, 
                    mouse.velocity.Y + mouse.direction.Y * motion
                    );
                
                //clamp velocity at maxSpeed
                float speed = (float)Math.Sqrt(mouse.velocity.X * mouse.velocity.X + mouse.velocity.Y * mouse.velocity.Y);
                if (speed > maxSpeed) {
                    float speedMultiplier = maxSpeed / speed;
                    mouse.velocity.X *= speedMultiplier;
                    mouse.velocity.Y *= speedMultiplier;
                }


                //update mouse position, correct collisions
                mouse.position.X += mouse.velocity.X * deltaTicks / 1000.0f;
                mouse.position.Y += mouse.velocity.Y * deltaTicks / 1000.0f;
                fixCollisions();

                //apply "friction" (not physically accurate)
                mouse.velocity.X -= mouse.velocity.X * (momentumLoss * deltaTicks / 1000.0f);
                mouse.velocity.Y -= mouse.velocity.Y * (momentumLoss * deltaTicks / 1000.0f);

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
            float vDist; //distance to closest vertex
            Vertex check;
            //TODO: kill (negate?) velocity on collision

            //check if out-of-bounds
            if (posXNR > maze.sizeX - 2 || posXNR < 1 ||
                posYNR > maze.sizeY - 2 || posYNR < 1) {
                return;
            }
            
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

            //fix vertex collisions
            vDist = (posXNR - posX) * (posXNR - posX) + (posYNR - posY) * (posYNR - posY);
            if (vDist < mouse.radius * mouse.radius) {
                float factor = 1 - (float)Math.Sqrt(vDist) / mouse.radius;
                mouse.position.X += factor * (posX - posXNR);
                mouse.position.Y += factor * (posY - posYNR);
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
