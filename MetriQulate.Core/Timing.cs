using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;	

namespace MetriQulate.Core
{
	public sealed class Timing : IDisposable
	{
		#region Members

		private string name;
		private Profiler profiler;
		private Timing parent;
		private long startMilliseconds;
		private long elapsed;
		private List<Timing> subTimings;
		private bool disposed;

		#endregion Members

		#region Constructor

		public Timing(string name, Profiler profiler, Timing parent, long startMilliseconds)
		{
			this.name = name;
			this.profiler = profiler;
			this.parent = parent;
			this.startMilliseconds = startMilliseconds;
			this.disposed = false;

			if (parent != null)
			{
				if (parent.subTimings == null)
				{
					parent.subTimings = new List<Timing>();
				}
				parent.subTimings.Add(this);
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
					return profiler.Elapsed - startMilliseconds;
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
				Elapsed = Elapsed
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

				elapsed = profiler.Elapsed - startMilliseconds;
				profiler.StopTimer(this);
			}
		}

		#endregion IDisposable Members
	}
}
