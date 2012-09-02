using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetriQulate.Core
{
	public class TimingResult
	{
		/// <summary>
		/// Name of timing
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Elapsed time in milliseconds
		/// </summary>
		public long Elapsed { get; set; }

		/// <summary>
		/// List of sub-timings
		/// </summary>
		public List<TimingResult> SubTimings { get; set; }

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
	}
}
