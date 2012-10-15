using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetriQulate.Core
{
	public class CounterReport
	{
		/// <summary>
		/// Counter name
		/// </summary>
		public string Name;

		/// <summary>
		/// Number of successful hits in this time interval
		/// </summary>
		public int SuccessCount;

		/// <summary>
		/// Number of failed hits in this time interval
		/// </summary>
		public int FailureCount;
	}
}
