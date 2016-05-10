using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseTester {
    public class MouseRandom : MouseAI {
        private Random rng = new Random();

        void MouseAI.update(Hardware h) {
            float power = (float)(rng.NextDouble() * 1.5 - 0.5);
            h.setMotorPower(power);

            float turn = (float)(rng.NextDouble() * 2.0 - 1.0);
            h.setTurnPower(turn);
        }
    }
}
