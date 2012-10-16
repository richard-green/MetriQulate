using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetriQulate.Core;
using Castle.DynamicProxy;

namespace MetriQulate.Core
{
	public class MetriQInterceptor : IInterceptor
	{
		#region IInterceptor Members

		public void Intercept(IInvocation invocation)
		{
			Profiler profiler = Profiler.Instance;
			Counter counter = Counter.Instance;
			Timing timer = null;

			if (profiler != null)
			{
				timer = profiler.Timer(invocation.TargetType.Name, invocation.Method.Name);
			}

			try
			{
				invocation.Proceed();

				if (counter != null)
				{
					counter.ReportSuccess(invocation.Method.Name);
				}
			}
			catch (Exception ex)
			{
				if (timer != null)
				{
					timer.ExceptionOccurred = true;
				}
				if (counter != null)
				{
					counter.ReportFailure(invocation.Method.Name);
				}
				throw;
			}
			finally
			{
				if (timer != null)
				{
					timer.Dispose();
				}
			}
		}

		#endregion IInterceptor Members
	}
}
