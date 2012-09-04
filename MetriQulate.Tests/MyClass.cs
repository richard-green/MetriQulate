using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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
			Thread.Sleep(5);
		}

		private void Step2()
		{
			Thread.Sleep(5);
		}
	}
}
