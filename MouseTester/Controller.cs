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
        MazePanel mazeDisplay;


        //Mouse physics parameters
        private Mouse mouse;
        private float speedScale = 5.0f;
        private float turnScale = 10.0f;
        //private float maxSpeed = 1.0f;
        //private float friction = 0.8f;

        //Threading controls
        Thread gameThread;
        bool paused;
        public bool isRunning
        {
            get { return !paused && alive; }
        }
        Semaphore pause;
        public bool alive;
        public int deltaTicks;
        public int currentTick;
        public int lastTick;

        public Controller(MazePanel display) {
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

        private void fixCollisions() {
            float posX = mouse.position.X;
            int posXN = (int)posX;
            int posXNR = (int)Math.Round(posX);
            float posY = mouse.position.Y;
            int posYN = (int)posY;
            int posYNR = (int)Math.Round(posY);
            Maze.Vertex check;
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
