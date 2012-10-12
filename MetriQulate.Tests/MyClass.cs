using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MetriQulate.Core;

namespace MetriQulate.Test
{
	public class MyClass : IMyClass
	{
		public void BeginProcess()
		{
			Step1();
			Step2();
		}

		private void Step1()
		{
			Thread.Sleep(5);
			Step1a();
		}

		private void Step1a()
		{
			using (Profiler.Instance.Timer("Step1a"))
			{
				Thread.Sleep(5);
			}
		}

		private void Step2()
		{
			using (Profiler.Instance.Timer("Step2"))
			{
				Thread.Sleep(5);
				throw new NotImplementedException();
			}
		}
	}
}
