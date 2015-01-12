using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;

namespace sGridServer.Code.Utilities
{
    /// <summary>
    /// This class represents a simple, event driven timer. 
    /// </summary>
    public class Timer
    {
        /// <summary>
        /// The interval between ticks, in milliseconds.
        /// </summary>
        private int interval;

        /// <summary>
        /// A wait handle object.
        /// </summary>
        private AutoResetEvent waitHandle;

        /// <summary>
        /// The registered wait handle. 
        /// </summary>
        private RegisteredWaitHandle registeredHandle;

        /// <summary>
        /// Gets a boolean indicating whether the timer is currently running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return registeredHandle != null;
            }
        }

        /// <summary>
        /// Gets or sets the interval. If the interval is changed, the timer is restarted. 
        /// </summary>
        public TimeSpan Interval
        {
            get
            {
                return new TimeSpan(0, 0, 0, 0, interval);
            }
            set
            {
                interval = (int)value.TotalMilliseconds;

                //To refresh the interval, we have to restart the timer. 
                if (IsRunning)
                {
                    Stop();
                    Start();
                }
            }
        }

        /// <summary>
        /// Is raised whenever the specified timer interval has elapsed. 
        /// </summary>
        public event EventHandler Tick;

        /// <summary>
        /// Creates a new instance of this class, storing the interval. 
        /// </summary>
        /// <param name="interval">The interval which should pass between the timer ticks, in milliseconds.</param>
        public Timer(int interval)
        {
            this.waitHandle = new AutoResetEvent(false);
            this.interval = interval;
        }

        /// <summary>
        /// Creates a new instance of this class, storing the interval. 
        /// </summary>
        /// <param name="interval">The interval which should pass between the timer ticks, as timespan.</param>
        public Timer(TimeSpan interval)
            : this((int)interval.TotalMilliseconds)
        { }

        /// <summary>
        /// Starts the timer. 
        /// </summary>
        public void Start()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("The timer is already running.");
            }

            //Register with system threadpool
            registeredHandle = ThreadPool.RegisterWaitForSingleObject(waitHandle, new WaitOrTimerCallback(TimerCallback), null, interval, false);
        }

        /// <summary>
        /// Internally used timer callback.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="state">The state.</param>
        private void TimerCallback(object argument, bool state)
        {
            NotifyTimerTick();
        }

        /// <summary>
        /// Stops the timer. 
        /// </summary>
        public void Stop()
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("The timer is not running.");
            }
            registeredHandle.Unregister(waitHandle);

            registeredHandle = null;
        }

        /// <summary>
        /// Raises the Tick event. 
        /// </summary>
        protected void NotifyTimerTick()
        {
            if (Tick != null)
            {
                Tick(this, new EventArgs());
            }
        }

        /// <summary>
        /// Destruktor: Stops the timer. 
        /// </summary>
        ~Timer()
        {
            if (IsRunning)
            {
                Stop();
            }
        }
    }
}