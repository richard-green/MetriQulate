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
			var timer = Profiler.Instance.Timer(String.Format("{0}.{1}", invocation.TargetType.Name, invocation.Method.Name));

			try
			{
				invocation.Proceed();
			}
			catch (Exception ex)
			{
				if (timer != null)
				{
					timer.ExceptionOccurred = true;
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
