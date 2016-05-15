using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseTester {
    public class Ray {
        public PointF origin;
        public PointF direction;

        public Ray(PointF origin, PointF direction) {
            this.origin = origin;
            this.direction = direction;
        }

        public Ray(float originX, float originY, float dirX, float dirY) {
            origin = new PointF(originX, originY);
            direction = new PointF(dirX, dirY);
        }
    }
}
