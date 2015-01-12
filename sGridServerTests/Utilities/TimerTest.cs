using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sGridServer.Code.Utilities;
using System.Threading;

namespace sGridServerTests.Utilities
{
    /// <summary>
    /// Tests the timer class
    /// </summary>
    [TestClass]
    public class TimerTest
    {
        private int ticks;
        private AutoResetEvent wait;

        /// <summary>
        /// Tests whether the basic operations of the timer work. 
        /// </summary>
        [TestMethod, Timeout(2000)]
        public void TestTimerTickAndRestart()
        {
            //Set up timer. 
            ticks = 0;
            wait = new AutoResetEvent(false);

            sGridServer.Code.Utilities.Timer t = new sGridServer.Code.Utilities.Timer(100);

            //Register an event handler to the timer. 
            t.Tick += new EventHandler(t_Tick);

            //Check timer state.
            Assert.IsFalse(t.IsRunning);

            //State timers.
            t.Start();

            Assert.IsTrue(t.IsRunning);

            //Check whether five ticks were done. 
            while (ticks < 5)
            {
                wait.WaitOne();
            }

            //Change timer timespan.
            t.Interval = new TimeSpan(0, 0, 0, 0, 101);

            //Check whether five ticks were done. 
            while (ticks < 5)
            {
                wait.WaitOne();
            }

            //Stop timer and check state. 
            t.Stop();

            Assert.IsFalse(t.IsRunning);

            wait.Dispose();

        }

        /// <summary>
        /// Timer callback.
        /// </summary>
        void t_Tick(object sender, EventArgs e)
        {
            ticks += 1;
            wait.Set();
        }

        /// <summary>
        /// Tests whether stopping a stopped timer is handled correctly.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestTimerExceptionStop()
        {
            sGridServer.Code.Utilities.Timer t = new sGridServer.Code.Utilities.Timer(100);

            t.Stop();
        }

        /// <summary>
        /// Tests whether starting a started timer is handled correctly.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestTimerExceptionStart()
        {
            sGridServer.Code.Utilities.Timer t = new sGridServer.Code.Utilities.Timer(100);

            t.Start();
            t.Start();
        }
    }
}
