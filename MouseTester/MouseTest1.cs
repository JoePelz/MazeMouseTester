using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseTester {
    class MouseTest1 : MouseAI {

        public void update(Hardware h) {
            if (h.getForwardSensor() > 1.0f) {
                h.setMotorPower(1.0f);
            } else if (h.getForwardSensor() > 0.5f) {
                h.setMotorPower(0.2f);
            } else if (h.getForwardSensor() < 0.4f) {
                h.setMotorPower(-0.2f);
            } else {
                h.setMotorPower(0);
            }
        }
    }
}
