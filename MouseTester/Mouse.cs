using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseTester {
    public class Mouse {
        public static PointF StartPosition = new PointF(1.5f, 1.5f);
        public static PointF StartDirection = new PointF(0.0f, 1.0f);

        public float radius = 0.275f; //based on draw size in MazePanel. TODO: other way around.
        public PointF position = new PointF(StartPosition.X, StartPosition.Y);  //is maze coordinates. 1,1 is lower left corner.
        public PointF direction = new PointF(StartDirection.X, StartDirection.Y); //this should be normalized
        public float speed = 0.0f;                        //should be vector, but this is easier right now.
        public float angle = (float)(Math.PI / 2.0);      //in radians. 0 is right, pi/2 is up.

    }
}
