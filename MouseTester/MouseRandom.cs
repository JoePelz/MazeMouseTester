using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseTester {
    public class MouseRandom : MouseAI {
        private Random rng = new Random();

        private int turnChangeRate = 100; //every n milliseconds
        private int nextTurnChange = 1;

        private int speedChangeRate = 1000; //every n milliseconds
        private int nextSpeedChange = 1;
        
        void MouseAI.update(Hardware h) {

            //speed changes
            nextSpeedChange -= h.getMillis();
            if (nextSpeedChange < 0) {
                nextSpeedChange += speedChangeRate; 
                float power = (float)(rng.NextDouble() * 1.5 - 0.5);
                h.setMotorPower(power);
            }

            //turning changes
            nextTurnChange -= h.getMillis();
            if (nextTurnChange < 0) {
                nextTurnChange += turnChangeRate;
                float turn = (float)(rng.NextDouble() * 2.0 - 1.0);
                h.setTurnPower(turn);
            }
        }
    }
}
