﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseTester {
    class Maze {
        Vertex[,] maze;
        int sizeX;
        int sizeY;
        private static Brush brushVertex;
        private static Pen penWall;
        private static Pen penUnset;
        private static Pen penHighlight;
        private static int UNSET = -1;
        private static int WALL = 1;
        private static int EMPTY = 0;

        public Maze(int width, int height) {
            sizeX = width + 3; //+1 for both edges, +2 for an outer layer of empties
            sizeY = height + 3;
            penWall = new Pen(Color.Black, 0.05f);
            penUnset = new Pen(Color.Green, 0.05f);
            brushVertex = new SolidBrush(Color.Black);
            penHighlight = new Pen(Color.BlueViolet, 0.05f);
            init();
            buildMaze();
        }

        public void rebuildMaze() {
            init();
            buildMaze();
        }

        
        public void drawMaze(Graphics g, Rectangle bounds) {
            int size = Math.Min(bounds.Width, bounds.Height);
            
            //NOTE: this assumes a square maze with square cells
            Rectangle mazeRect = new Rectangle(
                (bounds.Width >> 1) - (size >> 1) + bounds.X,
                (bounds.Height >> 1) - (size >> 1) + bounds.Y,
                size, size);
            
            //Clear the maze
            g.FillRectangle(Brushes.LightBlue, mazeRect);
            var trans = g.Transform;
            trans.Translate(mazeRect.X, mazeRect.Y);
            trans.Translate(0, mazeRect.Height);
            trans.Scale(mazeRect.Width / (sizeX - 1), -mazeRect.Height / (sizeY - 1));
            g.Transform = trans;

            //testing circles. Bottom left, first cell top right, maze top right
            //g.DrawEllipse(penWall, -0.5f, -0.5f, 1.0f, 1.0f);
            //g.DrawEllipse(penWall, 0.5f, 0.5f, 1.0f, 1.0f);
            //g.DrawEllipse(penHighlight, sizeX - 1.5f, sizeY - 1.5f, 1, 1);
            
            for (int x = 1; x < sizeX - 1; ++x) {
                for (int y = 1; y < sizeY - 1; ++y) {
                    g.FillEllipse(brushVertex, x - 0.1f, y - 0.1f, 0.2f, 0.2f);
                    if (maze[x, y].right == WALL) {
                        g.DrawLine(penWall, x, y, x + 1, y);
                    } else if (maze[x, y].right == UNSET) {
                        g.DrawLine(penUnset, x, y, x + 1, y);
                    }
                    if (maze[x, y].up == WALL) {
                        g.DrawLine(penWall, x, y, x, y + 1);
                    } else if (maze[x, y].up == UNSET) {
                        g.DrawLine(penUnset, x, y, x, y + 1);
                    }
                }
            }
        }

        private void init() {
            maze = new Vertex[sizeX, sizeY];

            for (int x = 0; x < sizeX; ++x) {
                for (int y = 0; y < sizeY; ++y) {
                    maze[x, y] = new Vertex();

                    //set the border walls
                    if (x == 1 || x == sizeX - 2) {
                        maze[x, y].up = WALL;
                        if (x == sizeX - 2) {
                            maze[x, y].right = EMPTY;
                        }
                    }
                    if (y == 1 || y == sizeY - 2) {
                        maze[x, y].right = WALL;
                        if (y == sizeY - 2) {
                            maze[x, y].up = EMPTY;
                        }
                    }
                    if (x == 0 || x == sizeX - 1) {
                        maze[x, y].up = EMPTY;
                        maze[x, y].right = EMPTY;
                    }
                    if (y == 0 || y == sizeY - 1) {
                        maze[x, y].up = EMPTY;
                        maze[x, y].right = EMPTY;
                    }
                }
            }
            //fix the unnecessary corner edges.
            maze[sizeX - 2, 1].right = EMPTY;
            maze[sizeX - 2, sizeY - 2].right = EMPTY;
            maze[sizeX - 2, sizeY - 2].up = EMPTY;
            maze[1, sizeY - 2].up = EMPTY;
            
        }
        
        private void buildMaze() {
            if (sizeX < 3 || sizeY < 3)
                return;
            
            //track open tips in a list.
            List<Edge> options = new List<Edge>();
            options.Add(new Edge(1, 2, true));
            options.Add(new Edge(2, 1, false));

            
            //even-even are walls
            //odd-odd are spaces
            //grab random tip. activate it. add new tips to list.
            Random rng = new Random();
            
            while (options.Any()) {
                int numOptions = options.Count();

                //Choose a random edge to open, and remove it from the list
                int target = (int)(rng.NextDouble() * numOptions);
                Edge pos = options[target];
                options.RemoveAt(target);

                //If the edge isn't available (already a wall or empty), move on...
                if (pos.isHorizontal && maze[pos.X, pos.Y].right != UNSET) {
                    continue;
                }
                if (!pos.isHorizontal && maze[pos.X, pos.Y].up != UNSET) {
                    continue;
                }

                //Smash open the wall!
                if (!BreachWall(pos, options)) {
                    if (pos.isHorizontal)
                        maze[pos.X, pos.Y].right = WALL;
                    else
                        maze[pos.X, pos.Y].up = WALL;
                }

                //DrawToConsole();
            }

            //place the exit.
            maze[sizeX - 2, sizeY - 3].up = EMPTY;
        }

        private bool BreachWall(Edge edge, List<Edge> options) {
            //check destination is valid
            int direction = -1;
            if (edge.isHorizontal) {
                //so we're either going up or down
                //for the move to be valid, one of those sides must have no "EMPTY" edges.
                //check "up"
                if (maze[edge.X, edge.Y].right == EMPTY
                    || maze[edge.X, edge.Y].up == EMPTY
                    || maze[edge.X + 1, edge.Y].up == EMPTY
                    || maze[edge.X, edge.Y + 1].right == EMPTY) {
                    //moving up is invalid.
                    //check "down"
                    if (maze[edge.X, edge.Y].right == EMPTY
                        || maze[edge.X, edge.Y - 1].right == EMPTY
                        || maze[edge.X, edge.Y - 1].up == EMPTY
                        || maze[edge.X + 1, edge.Y - 1].up == EMPTY) {
                        //moving down is invalid.
                        return false;
                    } else {
                        direction = 1; //down
                    }
                } else {
                    direction = 0; //up
                }
            } else {
                //so we're going either left or right
                //for the move to be valid, one of those sides must have no "EMPTY" edges.
                //check "right"
                if (maze[edge.X, edge.Y].up == EMPTY
                    || maze[edge.X, edge.Y].right == EMPTY
                    || maze[edge.X, edge.Y + 1].right == EMPTY
                    || maze[edge.X + 1, edge.Y].up == EMPTY) {
                    //it wasn't right.
                    //check left
                    if (maze[edge.X, edge.Y].up == EMPTY
                        || maze[edge.X - 1, edge.Y].up == EMPTY
                        || maze[edge.X - 1, edge.Y].right == EMPTY
                        || maze[edge.X - 1, edge.Y + 1].right == EMPTY) {
                        //it wasn't left.
                        return false;
                    } else {
                        direction = 3; //left
                    }
                } else {
                    direction = 2; //right
                }
            }
            //The move is valid!

            //set wall to EMPTY
            if (edge.isHorizontal) {
                maze[edge.X, edge.Y].right = EMPTY;
            } else {
                maze[edge.X, edge.Y].up = EMPTY;
            }

            //add new places to options
            switch(direction) {
                case 0: //up
                    if (maze[edge.X, edge.Y].up == UNSET) {
                        options.Add(new Edge(edge.X, edge.Y, false));
                    }
                    if (maze[edge.X, edge.Y + 1].right == UNSET) {
                        options.Add(new Edge(edge.X, edge.Y + 1, true));
                    }
                    if (maze[edge.X + 1, edge.Y].up == UNSET) {
                        options.Add(new Edge(edge.X + 1, edge.Y, false));
                    }
                    break;
                case 1: //down
                    if (maze[edge.X, edge.Y - 1].up == UNSET) {
                        options.Add(new Edge(edge.X, edge.Y - 1, false));
                    }
                    if (maze[edge.X, edge.Y - 1].right == UNSET) {
                        options.Add(new Edge(edge.X, edge.Y - 1, true));
                    }
                    if (maze[edge.X + 1, edge.Y - 1].up == UNSET) {
                        options.Add(new Edge(edge.X + 1, edge.Y - 1, false));
                    }
                    break;
                case 2: //right
                    if (maze[edge.X, edge.Y].right == UNSET) {
                        options.Add(new Edge(edge.X, edge.Y, true));
                    }
                    if (maze[edge.X + 1, edge.Y].up == UNSET) {
                        options.Add(new Edge(edge.X + 1, edge.Y, false));
                    }
                    if (maze[edge.X, edge.Y + 1].right == UNSET) {
                        options.Add(new Edge(edge.X, edge.Y + 1, true));
                    }
                    break;
                case 3: //left
                    if (maze[edge.X - 1, edge.Y].right == UNSET) {
                        options.Add(new Edge(edge.X - 1, edge.Y, true));
                    }
                    if (maze[edge.X - 1, edge.Y].up == UNSET) {
                        options.Add(new Edge(edge.X - 1, edge.Y, false));
                    }
                    if (maze[edge.X - 1, edge.Y + 1].right == UNSET) {
                        options.Add(new Edge(edge.X - 1, edge.Y, true));
                    }
                    break;
                default:
                    break;
            }
            return true;
        }

        private class Vertex {
            public int up = UNSET;
            public int right = UNSET;
        }

        private class Edge {
            public int X;
            public int Y;
            public bool isHorizontal;

            public Edge(int x, int y, bool horizontal) {
                this.X = x;
                this.Y = y;
                this.isHorizontal = horizontal;
            }
        }
    }
}
