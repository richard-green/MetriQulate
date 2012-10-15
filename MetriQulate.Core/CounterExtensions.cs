using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetriQulate.Core
{
	public static class CounterExtensions
	{
		public static void ReportSuccess(this Counter counter, string counterName)
		{
			if (counter != null)
			{
				counter.ReportSuccess(counterName);
			}
		}

		public static void ReportFailure(this Counter counter, string counterName)
		{
			if (counter != null)
			{
				counter.ReportFailure(counterName);
			}
		}
	}
}
