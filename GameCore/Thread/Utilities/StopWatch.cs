using System;

namespace GameCore
{
	/// <summary>
	/// Provide a stop watch functionality that is similar to the one
	/// in .Net 2.0. The instances of this class are _not_ thread-safe.
	/// </summary>
	public class StopWatch
	{
		#region Public Constructor(s)

		public StopWatch()
		{
			// No-Op
		}

		#endregion
		#region Public Methods

		public void Start()
		{
			// Set our running flag, and re-start of ticks
			//
			_running = true;
			_ticks = _stopTicks = DateTime.Now.Ticks;
		}
		public void Stop()
		{
			// Set our running flag to false, and set our stop ticks.
			//
			_running = false;
			_stopTicks = DateTime.Now.Ticks;
		}

		#endregion
		#region Public Properties

		public bool Running
		{
			get
			{
				return _running;
			}
		}
		public long Ticks
		{
			// When we are not running the elapsed time represented by StopWatch
			// object is the time from its start to the time it is stopped.
			// When we are running the elapsed time represented by StopWatch object
			// is the time from its start to the current time.
			//
			get
			{
				if (_running == true)
					return DateTime.Now.Ticks - _ticks;
				else
					return _stopTicks - _ticks;
			}
		}
		public long Milliseconds
		{
			get
			{
				return this.Ticks/10000;
			}
		}
		public long Seconds
		{
			get
			{
				return (this.Ticks/10000)/1000;
			}
		}
		public long Minutes
		{
			get
			{
				return ((this.Ticks/10000)/1000)/60;
			}
		}

		#endregion
		#region Private Data Members
		
		private long _ticks = 0;
		private long _stopTicks = 0;
		private bool _running = false;

		#endregion
	}
}
