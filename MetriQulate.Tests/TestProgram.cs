using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetriQulate.Core;
using System.Threading;
using EasyNetQ;
using System.Diagnostics;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using Castle.Core;
using Castle.Windsor.Installer;

namespace MetriQulate.Test
{
	class TestProgram
	{
		static WindsorContainer container;

		static void Main(string[] args)
		{
			var bus = RabbitHutch.CreateBus("host=localhost");
			bus.Subscribe<TimingResult>("Timing", TimingReceived);

			// Comment out this line of code to disable profiling - profiling is disabled by default
			Profiler.EnableProfiling(bus);

			InterceptorMetricTest();

			ExplicitMetricTest();
		}

		private static void InterceptorMetricTest()
		{
			container = new Castle.Windsor.WindsorContainer();

			container.Install(FromAssembly.InDirectory(new AssemblyFilter("")));

			container.Register(Component.For<IMyClass>()
				.ImplementedBy<MyClass>()
				.Interceptors(InterceptorReference.ForType<MetriQInterceptor>()).Anywhere
				);

			Stopwatch stopwatch = Stopwatch.StartNew();

			for (int i = 0; i < 5; i++)
			{
				var myClass = container.Resolve<IMyClass>();

				// Make the call
				myClass.BeginProcess();
			}

			Console.WriteLine("Complete in {0:n0}ms", stopwatch.ElapsedMilliseconds);
		}

		private static void ExplicitMetricTest()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();

			for (int i = 0; i < 5; i++)
			{
				using (Profiler.Instance.Timer("Timer 1"))
				{
					for (int j = 0; j < 5; j++)
					{
						using (Profiler.Instance.Timer("Timer 2"))
						{
							Thread.Sleep(5);

							using (Profiler.Instance.Timer("Timer 3"))
							{
								Thread.Sleep(5);
							}
						}
					}
				}
			}

			Console.WriteLine("Complete in {0:n0}ms", stopwatch.ElapsedMilliseconds);
		}

		private static void TimingReceived(TimingResult timing)
		{
			Console.WriteLine("Received:");
			Console.ForegroundColor = ConsoleColor.Green;
			DumpTiming(timing);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		private static void DumpTiming(TimingResult timing, int nestLevel = 0)
		{
			if (timing.SubTimings != null && timing.SubTimings.Count > 0)
			{
				Console.WriteLine("{0} {1} - {2:n0}ms ({3:n0}ms)", new String('>', nestLevel * 2), timing.Name, timing.Elapsed, timing.ChildrenElapsed);
				foreach (var subTiming in timing.SubTimings)
				{
					DumpTiming(subTiming, nestLevel + 1);
				}
			}
			else
			{
				Console.WriteLine("{0} {1} - {2:n0}ms", new String('>', nestLevel * 2), timing.Name, timing.Elapsed, timing.ChildrenElapsed);
			}
		}
	}
}
