using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseTester {
    public class Edge {
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
