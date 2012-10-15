using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetriQulate.Core
{
	public class TimingNotification
	{
		/// <summary>
		/// Type of the calling class
		/// </summary>
		public string TypeName { get; set; }

		/// <summary>
		/// Method of the calling class
		/// </summary>
		public string MethodName { get; set; }

		/// <summary>
		/// Name of the timer
		/// </summary>
		public string TimerName { get; set; }

		/// <summary>
		/// Elapsed time in milliseconds
		/// </summary>
		public long Elapsed { get; set; }

		/// <summary>
		/// List of sub-timings
		/// </summary>
		public List<TimingNotification> SubTimings { get; set; }

		/// <summary>
		/// ID of the Thread that the timer ran on
		/// </summary>
		public long ThreadId { get; set; }

		/// <summary>
		/// Total of all children's elapsed time, in milliseconds
		/// </summary>
		public long ChildrenElapsed
		{
			get
			{
				return (SubTimings == null || SubTimings.Count == 0) ? 0 : SubTimings.Aggregate(0L, (total, subItem) => total + subItem.Elapsed);
			}
		}

		/// <summary>
		/// If an unhandled exception occurred
		/// </summary>
		public bool ExceptionOccurred { get; set; }
	}
}
