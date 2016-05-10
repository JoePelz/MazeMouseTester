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
        Hardware mouse;
        MouseAI ai;
        Panel display;

        //Mouse physics parameters
        private PointF mousePosition;
        private PointF mouseVelocity;
        private PointF mouseDirection;
        private float mouseAngle;     //in radians. 0 is right, pi/2 is up.
        private float terminalVelocity = 1.0f;
        private float friction = 0.8f;

        //Threading controls
        Thread gameThread;
        bool paused;
        Semaphore pause;
        public bool alive;
        public int deltaTicks;
        public int currentTick;
        public int lastTick;

        public Controller(Panel display) {
            this.display = display;
            maze = new Maze(10, 10);
            mouse = new Hardware(this);
            ai = new MouseRandom();
            alive = true;
            pause = new Semaphore(1, 1);
            paused = false;
            resetMousePhysics();
        }

        private void resetMousePhysics() {
            mousePosition = new PointF(1.5f, 1.5f);
            mouseVelocity = new PointF(0.0f, 0.0f);
            mouseDirection = new PointF(0.0f, 1.0f);
            mouseAngle = (float)(Math.PI / 2.0);
        }

        public void mainLoop() {
            Console.WriteLine("Starting!");
            while (alive) {
                //handle pausing
                pause.WaitOne();
                pause.Release();

                //set deltaTime
                currentTick = System.Environment.TickCount;
                deltaTicks = currentTick - lastTick;
                lastTick = currentTick;
                
                //update mouse physics

            }
            Console.WriteLine("Dead.");
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
        }

        public void rebuildMaze() {
            maze.rebuildMaze();
            display.Invalidate();
        }

        internal void drawMaze(Graphics graphics, Rectangle clientRectangle) {
            maze.drawMaze(graphics, clientRectangle);
        }
    }
}
