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
		private string name;
		private Profiler profiler;
		private Timing parent;
		private long startMilliseconds;
		private long elapsed;
		private List<Timing> subTimings;
		private int threadId;
		private bool disposed;

		#endregion Members

		#region Constructor

		public Timing(string name, Profiler profiler, Timing parent, int threadId)
		{
			this.name = name;
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
		/// Name of timing
		/// </summary>
		public string Name
		{
			get { return name; }
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

		internal Timing Parent
		{
			get { return parent; }
		}

		internal TimingResult GetResults()
		{
			TimingResult results = new TimingResult()
			{
				Name = name,
				Elapsed = Elapsed,
				ThreadId = threadId
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
