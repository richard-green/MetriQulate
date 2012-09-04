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
			using (Profiler.Instance.Timer(String.Format("{0}.{1}", invocation.TargetType.Name, invocation.Method.Name)))
			{
				invocation.Proceed();
			}
		}

		#endregion IInterceptor Members
	}
}
