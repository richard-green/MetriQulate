using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MetriQulate.Core
{
	public sealed class Timing : IDisposable
	{
		#region Members

		private Stopwatch stopwatch;
		private string typeName;
		private string methodName;
		private string timerName;
		private Profiler profiler;
		private Timing parent;
		private long startMilliseconds;
		private long elapsed;
		private List<Timing> subTimings;
		private int threadId;
		private bool disposed;

		#endregion Members

		#region Constructor

		internal Timing(string typeName, string methodName, string timerName, Profiler profiler, Timing parent, int threadId)
		{
			this.typeName = typeName;
			this.methodName = methodName;
			this.timerName = timerName;
			this.profiler = profiler;
			this.parent = parent;
			this.threadId = threadId;
			this.disposed = false;

			if (parent == null)
			{
				stopwatch = Stopwatch.StartNew();
				startMilliseconds = 0;
			}
			else
			{
				if (parent.subTimings == null)
				{
					parent.subTimings = new List<Timing>();
				}
				parent.subTimings.Add(this);
				stopwatch = parent.stopwatch;
				startMilliseconds = stopwatch.ElapsedMilliseconds;
			}
		}

		#endregion Constructor

		#region Instance Members

		/// <summary>
		/// Type of the calling class
		/// </summary>
		public string TypeName
		{
			get { return typeName; }
		}

		/// <summary>
		/// Method of the calling class
		/// </summary>
		public string MethodName
		{
			get { return methodName; }
		}

		/// <summary>
		/// Name of the timer
		/// </summary>
		public string TimerName
		{
			get { return timerName; }
		}

		/// <summary>
		/// Elapsed time in milliseconds (live value if not disposed)
		/// </summary>
		public long Elapsed
		{
			get
			{
				if (disposed)
				{
					return elapsed;
				}
				else
				{
					return stopwatch.ElapsedMilliseconds - startMilliseconds;
				}
			}
		}

		/// <summary>
		/// If an unhandled exception occurred
		/// </summary>
		public bool ExceptionOccurred
		{
			get;
			set;
		}

		internal Timing Parent
		{
			get { return parent; }
		}

		internal TimingNotification GetResults()
		{
			TimingNotification results = new TimingNotification()
			{
				TypeName = typeName,
				MethodName = methodName,
				TimerName = timerName,
				Elapsed = Elapsed,
				ThreadId = threadId,
				ExceptionOccurred = ExceptionOccurred
			};

			if (subTimings != null && subTimings.Count > 0)
			{
				results.SubTimings = subTimings.Select(subTiming => subTiming.GetResults()).ToList();
			}

			return results;
		}

		#endregion Instance Members

		#region IDisposable Members

		public void Dispose()
		{
			if (disposed == false)
			{
				disposed = true;

				elapsed = stopwatch.ElapsedMilliseconds - startMilliseconds;
				profiler.StopTimer(this);
			}
		}

		#endregion IDisposable Members
	}
}
