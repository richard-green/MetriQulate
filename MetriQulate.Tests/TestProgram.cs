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
using System.Threading.Tasks;
using System.IO;

namespace MetriQulate.Test
{
	class TestProgram
	{
		static WindsorContainer container;
		static object mutex = new object();

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

			container.AddFacility<MetriQInterceptFacility>(f => f.ForComponentsInNamespace("MetriQulate.Test"));

			container.Install(FromAssembly.InDirectory(new AssemblyFilter("")));

			container.Register(Component.For<IMyClass>()
				.ImplementedBy<MyClass>()
			);

			Stopwatch stopwatch = Stopwatch.StartNew();

			for (int i = 0; i < 100; i++)
			{
				Task asyncTask = new Task(() =>
				{
					IMyClass myClass = null;

					try
					{
						myClass = container.Resolve<IMyClass>();
						myClass.BeginProcess();
					}
					catch (Exception ex)
					{
						Console.Out.WriteLine(ex.ToString());
					}
					finally
					{
						container.Release(myClass);
					}
				});

				asyncTask.Start();
			}

			Console.WriteLine("InterceptorMetricTest Complete in {0:n0}ms", stopwatch.ElapsedMilliseconds);
		}

		private static void ExplicitMetricTest()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();

			for (int i = 0; i < 5; i++)
			{
				using (Profiler.Instance.Timer("TestProgram", "ExplicitMetricTest", "Timer 1"))
				{
					for (int j = 0; j < 5; j++)
					{
						using (Profiler.Instance.Timer("TestProgram", "ExplicitMetricTest", "Timer 2"))
						{
							Thread.Sleep(5);

							using (Profiler.Instance.Timer("TestProgram", "ExplicitMetricTest", "Timer 3"))
							{
								Thread.Sleep(5);
							}
						}
					}
				}
			}

			Console.WriteLine("ExplicitMetricTest Complete in {0:n0}ms", stopwatch.ElapsedMilliseconds);
		}

		private static void TimingReceived(TimingResult timing)
		{
			lock (mutex)
			{
				Console.WriteLine("Received:");

				if (Directory.Exists(@"D:\Temp\MetriQs") == false)
				{
					Directory.CreateDirectory(@"D:\Temp\MetriQs");
				}

				using (var output = File.CreateText(String.Format(@"D:\Temp\MetriQs\{0} {1} {2}.xml", timing.TypeName, timing.MethodName, Guid.NewGuid())))
				{
					DumpTiming(timing, output);
				}

				Console.ForegroundColor = ConsoleColor.Gray;
			}
		}

		private static void DumpTiming(TimingResult timing, StreamWriter output, int nestLevel = 0)
		{
			Console.ForegroundColor = timing.ExceptionOccurred ? ConsoleColor.Red : ConsoleColor.Green;
			string message = String.Format("{0} {1}.{2} - {3:n0}ms ({4:n0}ms)", new String('>', nestLevel * 2), timing.TypeName, timing.MethodName, timing.Elapsed, timing.ChildrenElapsed);
			Console.WriteLine(message);

			if (timing.SubTimings != null && timing.SubTimings.Count > 0)
			{
				output.WriteLine("<timing typeName='{0}' methodName='{1}' elapsed='{2}ms' children='{3}ms'>", timing.TypeName, timing.MethodName, timing.Elapsed, timing.ChildrenElapsed);
				foreach (var subTiming in timing.SubTimings)
				{
					DumpTiming(subTiming, output, nestLevel + 1);
				}
				output.WriteLine("</timing>");
			}
			else
			{
				output.WriteLine("<timing typeName='{0}' methodName='{1}' elapsed='{2}ms' children='{3}ms' />", timing.TypeName, timing.MethodName, timing.Elapsed, timing.ChildrenElapsed);
			}
		}
	}
}
