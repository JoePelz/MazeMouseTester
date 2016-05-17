using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseTester {
    //TODO: implement randomness in functions to mimic unreliability. 
    public class Hardware {

        //TODO: should this be changed to be left and right motor powers 
        //      instead of forward and turn?
        private float turnPower;
        private float forwardPower;

        private int deltaTime;
        private Random rng;
        private Controller controller;

        public Hardware(Controller controller) {
            this.controller = controller;
            rng = new Random();
        }
        
        /// <summary>
        /// Activate the main motor. positive values for forward, 
        /// negative for reverse. Use range (-1 : 1)
        /// </summary>
        /// <param name="power">Motor power [-1.0 : 1.0]</param>
        public void setMotorPower(float power) {
            //too weak to power up the motors
            if (Math.Abs(power) < 0.1) {
                return;
            }

            forwardPower = clamp(power, -1, 1);
        }

        /// <summary>
        /// Initiate a turning action. positive turns clockwise, 
        /// negative turns counter clockwise. Use range (-1 : 1)
        /// </summary>
        /// <param name="power">Motor power [-1.0 : 1.0]</param>
        public void setTurnPower(float power) {
            //too weak to power up the motors
            if (Math.Abs(power) < 0.1) {
                return;
            }
            turnPower = clamp(power, -1, 1);
        }

        public float getForwardPower() {
            return forwardPower;
        }

        public float getTurnPower() {
            return turnPower;
        }

        /// <summary>
        /// Read the current signal from the forward-pointing sensor. 
        /// Value indicates approximate distance to a wall or +INF on no data.
        /// </summary>
        /// <returns>Distance to the nearest wall in the forward direction, or +INF if no data</returns>
        public float getForwardSensor() {
            return controller.CastRayFromMouse(0.0f);
        }

        /// <summary>
        /// Read the current signal from the left-pointing sensor.
        /// Value indicates approximate distance to a wall or +INF on no data.
        /// </summary>
        /// <returns>Distance to the nearest wall left of the mouse, or +INF if no data</returns>
        public float getLeftSensor() {
            return controller.CastRayFromMouse((float)(-Math.PI / 2.0));
        }

        /// <summary>
        /// Read the current signal from the right-pointing sensor.
        /// Value indicates approximate distance to a wall or +INF on no data.
        /// </summary>
        /// <returns>Distance to the nearest wall right of the mouse, or +INF if no data</returns>
        public float getRightSensor() {
            return controller.CastRayFromMouse((float)(Math.PI / 2.0));
        }

        /// <summary>
        /// Constrain a value within a range, and return the constrained value.
        /// </summary>
        /// <param name="value">The value to constrain</param>
        /// <param name="min">The minimum valid value</param>
        /// <param name="max">The maximum valid value</param>
        /// <returns></returns>
        private float clamp(float value, float min, float max) {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        internal void setMillis(int deltaTicks) {
            deltaTime = deltaTicks;
        }

        /// <summary>
        /// Get the number of milliseconds elapsed since the last time this function was called.
        /// </summary>
        /// <returns>milliseconds since last execution of this routine.</returns>
        public int getMillis() {
            return deltaTime;
        }
    }
}
