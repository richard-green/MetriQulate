using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetriQulate.Core
{
	public static class ProfilerExtensions
	{
		public static Timing Timer(this Profiler profiler, string typeName, string methodName, string timerName = null)
		{
			if (profiler != null)
			{
				return profiler.Timer(typeName, methodName, timerName);
			}

			return null;
		}
	}
}
